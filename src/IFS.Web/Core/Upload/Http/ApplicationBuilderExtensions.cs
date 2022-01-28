// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ApplicationBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Framework.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IFS.Web.Core.Upload.Http {
    public static class ApplicationBuilderExtensions {
        public static void AddUploadHandler(this IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<UploadHandler>();
        }

        public static void MapUploadHandler(this IEndpointRouteBuilder endpoints, string routeTemplate) {
            SharedUploadRouteHandler uploadHandler = new SharedUploadRouteHandler();

            const string routeName = "AsyncUploadHandler";
            endpoints.MapMethods(routeTemplate, new []{ "PUT", "POST" }, uploadHandler.ExecuteAsync)
                     .WithName(routeName);
        }
    }
}
