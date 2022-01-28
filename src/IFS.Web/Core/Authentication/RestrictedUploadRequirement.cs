// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : RestrictedUploadRequirement.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core.Authorization;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace IFS.Web.Core.Authentication {
    public class RestrictedUploadRequirement : AuthorizationHandler<RestrictedUploadRequirement>, IAuthorizationRequirement {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RestrictedUploadRequirement requirement) {
            Debug.Assert(context.Resource is RouteEndpoint endpoint, "Unknown resource");

            ClaimsPrincipal user = context.User;
            string? allowedFileIdentifier = user.FindFirst(x => x.Type == KnownClaims.RestrictionId)?.Value;
            if (allowedFileIdentifier == null) {
                context.Succeed(requirement);

                return Task.CompletedTask;
            }

            HttpContext httpContext = HttpContextPolicyEvaluator.PolicyEvaluationHttpContext;
            RouteData routeData = httpContext.GetRouteData();

            string? requestFileIdentifier = (routeData.Values["fileIdentifier"] ?? routeData.Values["id"])?.ToString();
            if (requestFileIdentifier == null) {
                context.Fail();

                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
