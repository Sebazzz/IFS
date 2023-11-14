// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Program.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IFS.Web;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(wc => wc.UseStartup<Startup>().CaptureStartupErrors(true))
            .ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.local.json", true))
            .UseSerilog((context, svc, config) => 
                config.ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(svc)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "IFS")
                    .WriteTo.Console()
                    .WriteTo.Debug()
            )
            .UseContentRoot(Directory.GetCurrentDirectory())
            .Build();

        host.Run();
    }
}