#addin nuget:?package=Cake.Compression&version=0.1.4
#addin nuget:?package=SharpZipLib

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
var buildDir = Directory("./build") + Directory(configuration);
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
	
Task("Check-Node-Version")
	.Does(() => {
	try {
		Information("Checking node.js version...");
	
		var processSettings = new ProcessSettings()
			.WithArguments(args => args.Append("--version"))
			.SetRedirectStandardOutput(true)
		;
		
		var process = StartAndReturnProcess("node", processSettings);
		
		process.WaitForExit();
		
		string line = null;
		foreach (var output in process.GetStandardOutput()) {
			line = output;
			Debug(output);
		}
		
		if (String.IsNullOrEmpty(line)) {
			throw new CakeException("Didn't get any output from Node.js");
		}
	
		Version actualVersion = Version.Parse(line.Substring(1));
		Version wantedVersion = new Version(6,1,0);
		
		Information("Got version {0} - we want at least version {1}", actualVersion, wantedVersion);
		if (wantedVersion > actualVersion) {
			throw new CakeException($"Node version {actualVersion} does not satisfy the requirement of Node>={wantedVersion}");
		}
	} catch (Exception e) when (!(e is CakeException)) {
		throw new CakeException("Unable to check Node.js version");
	}
});

Task("Restore-NuGet-Packages")
    .Does(() => {
    DotNetCoreRestore(new DotNetCoreRestoreSettings {
		IgnoreFailedSources = true
	});
});

Task("Set-NodeEnvironment")
	.Does(() => {
		Information("Setting NODE_ENV to {0}", nodeEnv);
		
		System.Environment.SetEnvironmentVariable("NODE_ENV", nodeEnv);
	});

Task("Restore-Node-Packages")
	.IsDependentOn("Check-Node-Version")
	.Does(() => {
	
	int exitCode;
	
	try {
		Information("Trying to restore packages using npm-cache");
		
		exitCode = 
			StartProcess("cmd", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("/C").AppendQuoted("npm-cache install npm")));
		
		if (exitCode != 0) {
			Warning("npm-cache returned error code {0}. Falling back to npm.", exitCode);
			throw new CakeException();
		}
	} catch {
		Warning("Could not restore packages using npm-cache. Falling back to npm.");
	
		exitCode = 
			StartProcess("cmd", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("/C").AppendQuoted("npm install")));
	}
		
	if (exitCode != 0) {
		throw new CakeException($"'npm install' returned exit code {exitCode} (0x{exitCode:x2})");
	}
});

Task("Build")
	.IsDependentOn("Set-NodeEnvironment")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Restore-Node-Packages")
    .Does(() => {
        DotNetCoreBuild($"./IFS.sln");
});

Task("Run")
    .IsDependentOn("Build")
    .Does(() => {
        DotNetCoreRun($"IFS.Web.csproj", null, new DotNetCoreRunSettings { WorkingDirectory = "./src/IFS.Web" });
});

Action<string,string> PublishSelfContained = (string platform, string folder) => {
	Information("Publishing self-contained for platform {0}", platform);

	var settings = new DotNetCorePublishSettings
			 {
				 Configuration = configuration,
				 OutputDirectory = publishDir + Directory(folder ?? platform),
				 Runtime = platform
			 };
	
        DotNetCorePublish($"./src/IFS.Web/IFS.Web.csproj", settings);
};

Task("Run-Webpack")
	.IsDependentOn("Restore-Node-Packages")
	.IsDependentOn("Set-NodeEnvironment")
	.Does(() => {
		var exitCode = 
			StartProcess("cmd", new ProcessSettings()
			.UseWorkingDirectory(mainProjectPath)
			.WithArguments(args => args.Append("/C").AppendQuoted("npm run-script build")));
		
		if (exitCode != 0) {
			throw new CakeException($"'npm run-script build' returned exit code {exitCode} (0x{exitCode:x2})");
		}
	});

Task("Publish-Common")
	.Description("Internal task - do not use")
    .IsDependentOn("Rebuild")
	.IsDependentOn("Run-Webpack");
	
Task("Publish-Win10")
	.Description("Publish for Windows 10 / Windows Server 2016")
    .IsDependentOn("Publish-Common")
    .Does(() => PublishSelfContained("win10-x64", null));

Task("Publish-Ubuntu-Core")
	.Description("Internal task - do not use")
    .IsDependentOn("Publish-Common")
    .Does(() => PublishSelfContained("ubuntu.14.04-x64", "ubuntu.14.04-x64/app"));

Task("Publish-Ubuntu")
    .IsDependentOn("Publish-Ubuntu-Core")
	.Description("Publish for Ubuntu 14.04")
    .Does(() => {
       GZipCompress(publishDir + Directory("ubuntu.14.04-x64/"), publishDir + File("financial-app-ubuntu-14.04-x64.tar.gz"));
	});
	
Task("Publish")
    .IsDependentOn("Publish-Win10")
    .IsDependentOn("Publish-Ubuntu");

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