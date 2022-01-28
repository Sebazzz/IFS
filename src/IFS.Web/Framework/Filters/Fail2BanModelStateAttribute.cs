using IFS.Web.Core.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IFS.Web.Framework.Filters;

public sealed class Fail2BanModelStateAttribute : ActionFilterAttribute
{
    public string ModelStateProperty { get; }

    public Fail2BanModelStateAttribute(string modelStateProperty)
    {
        ModelStateProperty = modelStateProperty;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        HttpContext httpContext = context.HttpContext;
        IFail2Ban fail2Ban = httpContext.RequestServices.GetFail2Ban();

        if (fail2Ban.IsRateLimitApplied(httpContext))
        {
            context.ModelState.TryAddModelError(
                this.ModelStateProperty,
                "You have attempted this too many times. Please try again later."
            );
        }
    }
}