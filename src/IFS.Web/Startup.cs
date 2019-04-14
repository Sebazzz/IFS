// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Startup.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Framework.Middleware.Fail2Ban;
using IFS.Web.Framework.Services;

namespace IFS.Web {
    using System;

    using Core;
    using Core.Authentication;
    using Core.Authorization;
    using Core.Upload;
    using Core.Upload.Http;
    using Framework;
    using Hangfire;
    using Hangfire.Dashboard;
    using Hangfire.MemoryStorage;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
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
                opt.AddUploadPolicy(this.Configuration);
                opt.AddAdministrationPolicy(this.Configuration);
            });

            services.AddAuthentication()
                .AddFromSettings(this.Configuration);

            // Hangfire
            services.AddHangfire(config => {
                config.UseColouredConsoleLogProvider();
                config.UseMemoryStorage();
            });

            services.AddTransient<ExpiredFileRemovalJob>();

            // Add app services
            services.AddScoped<IAuthenticationProvider, AuthenticationProvider>();
            services.AddScoped<IAdministrationAuthenticationProvider, AuthenticationProvider>();

            // ... Security
            services.AddScoped<ITransitPasswordProtector, TransitPasswordProtector>();
            services.AddFail2Ban();

            // ... Configuration
            services.AddOptions();
            services.Configure<AuthenticationOptions>(this.Configuration.GetSection("Authentication"));
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
            app.UseFail2BanRecording();

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
            RecurringJob.AddOrUpdate<ExpiredFileRemovalJob>(
                x => x.Execute(JobCancellationToken.Null),
                "*/30 * * * *");
        }

        private sealed class AdministratorDashboardFilter : IDashboardAuthorizationFilter {
            private readonly IServiceProvider _serviceProvider;

            public AdministratorDashboardFilter(IServiceProvider serviceProvider) {
                this._serviceProvider = serviceProvider;
            }

            public bool Authorize(DashboardContext context) {
                var auth = this._serviceProvider.GetRequiredService<IOptions<Core.Authentication.AuthenticationOptions>>().Value?.Static.Administration;

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
