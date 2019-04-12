// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Program.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web {
    using System.IO;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public sealed class Program {
        public static void Main(string[] args) {
            IWebHost host = 
                WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x => x.AddJsonFile("appsettings.local.json", true))
                .CaptureStartupErrors(true)
                .ConfigureLogging((wc,logging) => {
                      var env = wc.HostingEnvironment;
                      var config = wc.Configuration;

                      logging.AddConfiguration(config.GetSection("Logging"));
                      logging.AddConsole();

                      if (env.IsDevelopment()) {
                          logging.AddDebug();
                      }
                 })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
