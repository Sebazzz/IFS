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

namespace IFS.Web;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(wc => wc.UseStartup<Startup>().CaptureStartupErrors(true))
            .ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.local.json", true))
            .ConfigureLogging((wc, logging) =>
            {
                var env = wc.HostingEnvironment;
                var config = wc.Configuration;

                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();

                if (env.IsDevelopment()) logging.AddDebug();
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .Build();

        host.Run();
    }
}