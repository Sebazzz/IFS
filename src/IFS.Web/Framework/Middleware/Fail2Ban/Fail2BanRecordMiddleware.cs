﻿// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : Fail2BanRecordMiddleware.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Threading.Tasks;
using IFS.Web.Core.Authentication;
using Microsoft.AspNetCore.Http;

namespace IFS.Web.Framework.Middleware.Fail2Ban;

public class Fail2BanRecordMiddleware : IMiddleware
{
    private readonly IFail2Ban _fail2Ban;

    public Fail2BanRecordMiddleware(IFail2Ban fail2Ban)
    {
        this._fail2Ban = fail2Ban;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        await next.Invoke(context);

        this.CheckAndApplyFail2Ban(context);
    }

    private void CheckAndApplyFail2Ban(HttpContext httpContext)
    {
        Fail2BanFeature fail2BanInfo = httpContext.Features.Get<Fail2BanFeature>();

        if (fail2BanInfo != null)
        {
            if (fail2BanInfo.IsSuccess)
            {
                this._fail2Ban.RecordSuccess(httpContext);
            }
            else
            {
                this._fail2Ban.RecordFailure(httpContext);
            }
        }
    }
}