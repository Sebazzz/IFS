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
    using Core.Upload.Http;

    using Hangfire;
    using Hangfire.Dashboard;
    using Hangfire.MemoryStorage;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public sealed class Startup {
        public Startup(IConfiguration env)
        {
            this.Configuration = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.
            services.AddRouting(routing => {
                routing.LowercaseUrls = true;

            });

            services.AddDataProtection();

            services.AddMvc(mvc => mvc.ModelBinderProviders.Insert(0, new FileIdentifierModelBinderProvider()));

            services.AddAuthorization(opt => {
                opt.AddPolicy(KnownPolicies.Upload,
                    b => b.AddAuthenticationSchemes(KnownAuthenticationScheme.PassphraseScheme)
                          .RequireAuthenticatedUser()
                          .RequireUserName(KnownPolicies.Upload)
                          .AddRequirements(new RestrictedUploadRequirement()));

                opt.AddPolicy(KnownPolicies.Administration,
                    b => b.AddAuthenticationSchemes(KnownAuthenticationScheme.AdministrationScheme)
                          .RequireAuthenticatedUser()
                          .RequireUserName(this.Configuration.GetSection("Authentication").GetSection("Administration").GetValue<string>("UserName")));
            });

            services.AddAuthentication()
                .AddCookie(KnownAuthenticationScheme.PassphraseScheme, 
                            opt => {
                                opt.LoginPath = new PathString("/authenticate/login");
                                opt.AccessDeniedPath = new PathString("/error/accessDenied");
                                opt.ReturnUrlParameter = "returnUrl";
                            })
                .AddCookie(KnownAuthenticationScheme.AdministrationScheme, 
                    opt => {
                        opt.LoginPath = new PathString("/administration/authenticate/login");
                        opt.AccessDeniedPath = new PathString("/error/accessDenied");
                        opt.ReturnUrlParameter = "returnUrl";
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
            services.AddScoped<ITransitPasswordProtector, TransitPasswordProtector>();

            // ... Configuration
            services.AddOptions();
            services.Configure<Core.Authentication.AuthenticationOptions>(this.Configuration.GetSection("Authentication"));
            services.Configure<FileStoreOptions>(this.Configuration.GetSection("FileStore"));

            // ... Upload
            services.AddSingleton<IMetadataReader, MetadataReader>();
            services.AddSingleton<IUploadProgressManager, UploadProgressManager>();
            services.AddSingleton<IUploadManager, UploadManager>();
            services.AddSingleton<IFileWriter, FileWriter>();
            services.AddSingleton<IFileStore, FileStore>();
            services.AddSingleton<IFileAccessLogger, FileAccessLogger>();
            services.AddSingleton<IUploadedFileRepository, UploadedFileRepository>();
            services.AddSingleton<IUploadFileLock, UploadFileLock>();
            services.AddSingleton<MetadataReader>();

            services.AddSingleton<IFileStoreFileProviderFactory, FileStoreFileProviderFactory>();

            services.AddUploadHandler();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.UseStatusCodePagesWithReExecute("/Error/Http-{0}");

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error/Error");

                // Come on, it is >= 2019, so anything stuff should be done over HTTPS
                app.UseHttpsRedirection();
                app.UseHsts();
            }
            
            app.UseStaticFiles();

            app.UseAuthentication();

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
                    // Async uploads - need to map this here as MVC won't generate routes to other IRouter
                    routes.MapUploadHandler("upload/handler/{fileIdentifier}");

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

                var ctx = (context as AspNetCoreDashboardContext)?.HttpContext;

                if (ctx == null) {
                    return false;
                }

                return ctx.User.Identity.Name == auth.UserName;
            }
        }
    }
}
