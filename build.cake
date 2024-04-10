#addin nuget:?package=Cake.Compression&version=0.2.6
#addin nuget:?package=Cake.GitVersioning&version=3.4.255
#addin nuget:?package=SharpZipLib&version=1.3.3

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosity = Argument<Verbosity>("verbosity", Verbosity.Minimal);

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var baseName = "IFS.Web";
var buildDir = Directory("./build");
var publishDir = Directory("./build/publish");
var assemblyInfoFile = Directory($"./src/{baseName}/Properties") + File("AssemblyInfo.cs");
var nodeEnv = configuration == "Release" ? "production" : "development";
var mainProjectPath = Directory("./src/IFS.Web");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
    CleanDirectory(buildDir);
	CleanDirectory(publishDir);
	CleanDirectories("./src/IFS.Web/bin");
	CleanDirectories("./src/IFS.Web/obj");
});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");
	
void CheckToolVersion(string name, string executable, string argument, Version wantedVersion) {
	try {
		Information($"Checking {name} version...");
	
		var processSettings = new ProcessSettings()
			.WithArguments(args => args.Append("/C").AppendQuoted(executable + " " + argument))
			.SetRedirectStandardOutput(true)
		;
		
		var process = StartAndReturnProcess("cmd", processSettings);
		
		process.WaitForExit();
		
		string line = null;
		foreach (var output in process.GetStandardOutput()) {
			line = output;
			Debug(output);
		}
		
		if (String.IsNullOrEmpty(line)) {
			throw new CakeException("Didn't get any output from " + executable);
		}
	
		Version actualVersion = Version.Parse(line.Trim('v'));
		
		Information("Got version {0} - we want at least version {1}", actualVersion, wantedVersion);
		if (wantedVersion > actualVersion) {
			throw new CakeException($"{name} version {actualVersion} does not satisfy the requirement of {name}>={wantedVersion}");
		}
	} catch (Exception e) when (!(e is CakeException)) {
		throw new CakeException($"Unable to check {name} version. Please check whether {name} is available in the current %PATH%.", e);
	}
}
	
Task("Check-Node-Version")
	.Does(() => {
	CheckToolVersion("node.js", "node", "--version", new Version(10,16,0));
});

Task("Check-Yarn-Version")
	.Does(() => {
	CheckToolVersion("yarn package manager", "yarn", "--version", new Version(1,16,0) /*Minimum supported on appveyor*/);
});

Task("Restore-NuGet-Packages")
    .Does(() => {
    DotNetRestore(new DotNetRestoreSettings {
		IgnoreFailedSources = true,
		ForceEvaluate = true,
		NoCache = true
	});
});

Task("Set-NodeEnvironment")
	.Does(() => {
		Information("Setting NODE_ENV to {0}", nodeEnv);
		
		System.Environment.SetEnvironmentVariable("NODE_ENV", nodeEnv);
	});

Task("Restore-Node-Packages")
	.IsDependentOn("Check-Node-Version")
	.IsDependentOn("Check-Yarn-Version")
	.Does(() => {
	
	int exitCode;
	
	Information("Trying to restore packages using yarn");
	
	exitCode = 
			StartProcess("cmd", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("/C").AppendQuoted("yarn --production=false --frozen-lockfile --non-interactive")));
		
	if (exitCode != 0) {
		throw new CakeException($"'yarn' returned exit code {exitCode} (0x{exitCode:x8})");
	}
});


Task("Build")
	.IsDependentOn("Set-NodeEnvironment")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Restore-Node-Packages")
    .Does(() => {
        DotNetBuild($"./IFS.sln", new() {
            Configuration = configuration
        });
});

Task("Run")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetRun($"IFS.Web.csproj", null, new DotNetRunSettings { WorkingDirectory = "./src/IFS.Web" });
});

string GetVersionString() {
	var version = GitVersioningGetVersion();
	
	return version.SemVer1;
}

Action<string,string> PublishSelfContained = (string platform, string folder) => {
	Information("Publishing self-contained for platform {0}", platform);

	DotNetPublishSettings settings = new() {
        Configuration = configuration,
        OutputDirectory = publishDir + Directory(folder ?? platform),
        Runtime = platform
    };
	
    DotNetPublish($"./src/IFS.Web/IFS.Web.csproj", settings);
};

Task("Run-Webpack")
	.IsDependentOn("Restore-Node-Packages")
	.IsDependentOn("Set-NodeEnvironment")
	.Does(() => {
		var exitCode = 
			StartProcess("cmd", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("/C").AppendQuoted("yarn run build")));
		
		if (exitCode != 0) {
			throw new CakeException($"'yarn run build' returned exit code {exitCode} (0x{exitCode:x2})");
		}
	});

Task("Publish-Common")
	.Description("Internal task - do not use")
    .IsDependentOn("Rebuild")
	.IsDependentOn("Run-Webpack");
	
Task("Publish-Windows-Core")
    .IsDependentOn("Publish-Common")
    .Does(() => PublishSelfContained("win-x64", null));

Task("Publish-Windows")
    .IsDependentOn("Publish-Windows-Core")
	.Description("Publish for Windows 64-bit")
    .Does(() => {
       ZipCompress(publishDir + Directory("win-x64/"), publishDir + File($"ifs-{GetVersionString()}-win-x64.zip"));
	});

Task("Publish-Linux-Core")
	.Description("Internal task - do not use")
    .IsDependentOn("Publish-Common")
    .Does(() => PublishSelfContained("linux-x64", "linux-x64/app"));

Task("Publish-Linux")
    .IsDependentOn("Publish-Linux-Core")
	.Description("Publish for Linux x64 (universal)")
    .Does(() => {
       GZipCompress(publishDir + Directory("linux-x64/"), publishDir + File($"ifs-{GetVersionString()}-linux-x64.tar.gz"));
	});
	
Task("Publish")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-Linux");
    
Task("Test-Prep")
    .IsDependentOn("Build")
	.IsDependentOn("Run-Webpack")
    .Does(() => {
        string playWrightScript = MakeAbsolute(buildDir + Directory("bin") + Directory("IFS.Tests.Integration") + Directory(configuration) + File("playwright.ps1")).ToString();
        Information($"Running playwright install script at {playWrightScript}");
        StartProcess("pwsh", $"-NoProfile -NonInteractive -File {playWrightScript} install");
    });

	
Task("Test")
    .IsDependentOn("Test-Prep")
    .Does(() => {
        DotNetTest("./IFS.sln", new() { NoBuild = true });
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("None");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);