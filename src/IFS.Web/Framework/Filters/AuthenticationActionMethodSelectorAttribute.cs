// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticationActionMethodSelectorAttribute.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core.Authentication;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IFS.Web.Framework.Filters {
    public abstract class AuthenticationActionMethodSelectorAttribute : ActionMethodSelectorAttribute {
        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action) {
            AuthenticationOptions authOptions = routeContext.HttpContext.RequestServices.GetRequiredService<IOptions<AuthenticationOptions>>().Value;

            return this.IsValidForRequest(authOptions);
        }

        protected abstract bool IsValidForRequest(AuthenticationOptions authOptions);
    }

    public sealed class OpenIdAuthenticationActionAttribute : AuthenticationActionMethodSelectorAttribute {
        protected override bool IsValidForRequest(AuthenticationOptions authOptions) {
            return authOptions.OpenIdConnect?.Enable == true;
        }
    }

    public sealed class StaticAuthenticationActionAttribute : AuthenticationActionMethodSelectorAttribute {
        protected override bool IsValidForRequest(AuthenticationOptions authOptions) {
            return authOptions.OpenIdConnect?.Enable != true;
        }
    }
}
