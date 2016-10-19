﻿// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Startup.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web {
    using Core;
    using Core.Authentication;
    using Core.Upload;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public sealed class Startup {
        public Startup(IHostingEnvironment env) {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.
            services.AddRouting(routing => {
                    routing.LowercaseUrls = true;
            });

            services.AddMvc(mvc => mvc.ModelBinderProviders.Insert(0, new FileIdentifierModelBinderProvider()));

            services.AddAuthorization(opt => 
                opt.AddPolicy(KnownPolicies.Upload, 
                    b => b.AddAuthenticationSchemes(KnownAuthenticationScheme.PassphraseScheme)
                          .RequireAuthenticatedUser()
                          .RequireUserName(KnownPolicies.Upload)));

            // Add app services
            services.AddScoped<IAuthenticationProvider, AuthenticationProvider>();

            // ... Configuration
            services.AddOptions();
            services.Configure<Core.Authentication.AuthenticationOptions>(this.Configuration.GetSection("Authentication"));
            services.Configure<FileStoreOptions>(this.Configuration.GetSection("FileStore"));

            // ... Upload
            services.AddSingleton<IUploadManager, UploadManager>();
            services.AddSingleton<FileWriter>();
            services.AddSingleton<FileStore>();
            services.AddSingleton<IUploadedFileRepository, UploadedFileRepository>();
            services.AddSingleton<IFileStoreFileProviderFactory, FileStoreFileProviderFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            } else {
                app.UseExceptionHandler("/Error/Error");
            }

            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = KnownAuthenticationScheme.PassphraseScheme,
                LoginPath = new PathString("/Authenticate/Login"),
                AccessDeniedPath = new PathString("/Error/AccessDenied"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ReturnUrlParameter = "returnUrl"
            });

            app.UseMvc(
                routes => {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}
