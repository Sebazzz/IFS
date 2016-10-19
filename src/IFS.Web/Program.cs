// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Program.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web {
    using System.IO;

    using Microsoft.AspNetCore.Hosting;

    public sealed class Program {
        public static void Main(string[] args) {
            IWebHost host = new WebHostBuilder()
                .CaptureStartupErrors(true)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
