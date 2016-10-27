// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ApplicationBuilderExtensions.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload.Http {
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Constraints;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public static class ApplicationBuilderExtensions {
        public static void AddUploadHandler(this IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<UploadHandler>();
        }

        public static void MapUploadHandler(this IRouteBuilder routeBuilder, string routeTemplate) {
            SharedUploadRouteHandler uploadHandler = new SharedUploadRouteHandler();
            RouteHandler routeHandler = new RouteHandler(uploadHandler.ExecuteAsync);

            Route route = 
                new Route(
                    target: routeHandler,
                    routeName: "AsyncUploadHandler",
                    routeTemplate: routeTemplate,
                    defaults: new RouteValueDictionary(),
                    constraints: new Dictionary<string, object> {
                        ["httpMethod"] = new HttpMethodRouteConstraint("POST", "PUT")
                    },
                    dataTokens: new RouteValueDictionary(),
                    inlineConstraintResolver: new NullInlineConstraintResolver());

            routeBuilder.Routes.Add(route);
        }

        private sealed class NullInlineConstraintResolver : IInlineConstraintResolver {
            public IRouteConstraint ResolveConstraint(string inlineConstraint) {
                throw new System.NotImplementedException();
            }
        }
    }
}
