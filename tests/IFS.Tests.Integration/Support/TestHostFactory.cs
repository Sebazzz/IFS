// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : TestHostFactory.cs
//  Project         : IFS.Tests.Integration
// ******************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using IFS.Web;
using IFS.Web.Core.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Serilog;

namespace IFS.Tests.Integration.Support;

internal static class TestHostFactory
{
    public const string TestAdminPassword = "integration test admin password";
    public const string TestAdminUser = "test-admin";
    public const string TestUploadPassword = "test upload password";

    public static IHostBuilder MakeHostBuilder()
    {
        // Find free TCP port to configure Kestel on
        IPEndPoint endPoint;
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            socket.Listen(1);
            endPoint = (IPEndPoint)socket.LocalEndPoint;
        }

        // Create host
        var webProjectDirectory = FindWebDirectory();
        var wwwrootDirectory = Path.Combine(webProjectDirectory, "wwwroot");
        var testLogFile = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            $"test-{TestContext.CurrentContext.Test.ClassName}.log");

        return Host.CreateDefaultBuilder()
            .UseSerilog((ctx, log) =>
                log.MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.File(testLogFile))
            .UseContentRoot(webProjectDirectory)
            .ConfigureWebHostDefaults(wh =>
                wh.UseStartup<Startup>()
                    .UseWebRoot(wwwrootDirectory)
                    .UseKestrel(k => k.Listen(endPoint!))
            )
            .ConfigureServices(services =>
            {
                services.Configure<AuthenticationOptions>(auth =>
                {
                    auth.Static = new()
                    {
                        Administration = new()
                        {
                            UserName = TestAdminUser,
                            Password = TestAdminPassword
                        },

                        Passphrase = TestUploadPassword
                    };
                });

                services.Configure<Fail2BanOptions>(opts =>
                {
                    opts.DebounceTime = TimeSpan.FromSeconds(60);
                    opts.MaximumAttempts = 2;
                });
            })
            .UseEnvironment("Test");
    }

    private static string FindWebDirectory()
    {
        var workDirectory = TestContext.CurrentContext.WorkDirectory;
        while (!Directory.EnumerateFiles(workDirectory, "*.sln").Any())
            workDirectory = Directory.GetParent(workDirectory)?.FullName ??
                            throw new InvalidOperationException("Unable to find solution directory");

        workDirectory = Path.Combine(workDirectory, "src");

        var webProjectDirectory = Directory.EnumerateDirectories(workDirectory, "*Web").FirstOrDefault() ??
                                  throw new InvalidOperationException(
                                      $"Unable to find web project directory in {workDirectory}");
        return webProjectDirectory;
    }
}