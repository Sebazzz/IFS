// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : RestrictedUploadRequirement.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Authentication {
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;

    public class RestrictedUploadRequirement : AuthorizationHandler<RestrictedUploadRequirement>, IAuthorizationRequirement {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RestrictedUploadRequirement requirement) {
            AuthorizationFilterContext filterContext = context.Resource as AuthorizationFilterContext;

            Debug.Assert(filterContext != null, "Unknown resource");

            ClaimsPrincipal user = context.User;
            string allowedFileIdentifier = user.FindFirst(x => x.Type == KnownClaims.RestrictionId)?.Value;
            if (allowedFileIdentifier == null) {
                context.Succeed(requirement);

                return Task.CompletedTask;
            }

            HttpContext httpContext = filterContext.HttpContext;
            RouteData routeData = httpContext.GetRouteData();

            string requestFileIdentifier = (routeData.Values["fileIdentifier"] ?? routeData.Values["id"])?.ToString();
            if (requestFileIdentifier == null) {
                context.Fail();

                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
