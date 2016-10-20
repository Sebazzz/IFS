// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Startup.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web {
    using System;

    using Core;
    using Core.Authentication;
    using Core.Upload;

    using Hangfire;
    using Hangfire.Dashboard;
    using Hangfire.MemoryStorage;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

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

            services.AddAuthorization(opt => {
                opt.AddPolicy(KnownPolicies.Upload,
                    b => b.AddAuthenticationSchemes(KnownAuthenticationScheme.PassphraseScheme)
                          .RequireAuthenticatedUser()
                          .RequireUserName(KnownPolicies.Upload));

                opt.AddPolicy(KnownPolicies.Administration,
                    b => b.AddAuthenticationSchemes(KnownAuthenticationScheme.AdministrationScheme)
                          .RequireAuthenticatedUser()
                          .RequireUserName(this.Configuration.GetSection("Authentication").GetSection("Administration").GetValue<string>("UserName")));
            });

            // Hangfire
            services.AddHangfire(config => {
                config.UseColouredConsoleLogProvider();
                config.UseMemoryStorage();
            });

            services.AddTransient<ExpiredFileRemovalJob>();

            // Add app services
            services.AddScoped<IAuthenticationProvider, AuthenticationProvider>();
            services.AddScoped<IAdministrationAuthenticationProvider, AuthenticationProvider>();

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
                LoginPath = new PathString("/authenticate/login"),
                AccessDeniedPath = new PathString("/error/accessDenied"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ReturnUrlParameter = "returnUrl"
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = KnownAuthenticationScheme.AdministrationScheme,
                LoginPath = new PathString("/administration/authenticate/login"),
                AccessDeniedPath = new PathString("/error/accessdenied"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ReturnUrlParameter = "returnUrl"
            });

            // Hangfire
            app.UseHangfireDashboard(
                pathMatch: "/administration/jobs", 
                options: new DashboardOptions {
                    AppPath = "/administration/",
                    Authorization = new[] {
                        new AdministratorDashboardFilter(app.ApplicationServices)
                    }
                });

            app.UseHangfireServer();

            // MVC and API
            app.UseMvc(
                routes => {
                    routes.MapAreaRoute(
                        name: "areaRoute",
                        areaName: "Administration",
                        template: "administration/{controller=Home}/{action=Index}/{id?}");

                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });

            // Configure hangfire jobs (not sure where to do this else)
            RecurringJob.AddOrUpdate<ExpiredFileRemovalJob>(x => x.Execute(JobCancellationToken.Null), Cron.MinuteInterval(30));
        }

        private sealed class AdministratorDashboardFilter : IDashboardAuthorizationFilter {
            private readonly IServiceProvider _serviceProvider;

            public AdministratorDashboardFilter(IServiceProvider serviceProvider) {
                this._serviceProvider = serviceProvider;
            }

            public bool Authorize(DashboardContext context) {
                var auth = this._serviceProvider.GetRequiredService<IOptions<Core.Authentication.AuthenticationOptions>>().Value?.Administration;

                if (auth == null) {
                    return false;
                }

                var ctx = this._serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (ctx == null) {
                    return false;
                }

                return ctx.User.Identity.Name == auth.UserName;
            }
        }
    }
}
