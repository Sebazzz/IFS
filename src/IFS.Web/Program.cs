// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Program.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.IO;
using IFS.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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