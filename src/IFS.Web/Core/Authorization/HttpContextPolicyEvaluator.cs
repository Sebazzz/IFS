// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HttpContextPolicyEvaluator.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace IFS.Web.Core.Authorization;

public class HttpContextPolicyEvaluator : IPolicyEvaluator
{
    private static readonly AsyncLocal<HttpContext?> PolicyEvaluationHttpContextHolder = new();
    private readonly IPolicyEvaluator _defaultPolicyEvaluator;

    public static HttpContext PolicyEvaluationHttpContext => PolicyEvaluationHttpContextHolder.Value ?? throw new InvalidOperationException("This value is currently not available.");

    public HttpContextPolicyEvaluator(IAuthorizationService authorizationService)
    {
        this._defaultPolicyEvaluator = new PolicyEvaluator(authorizationService);
    }

    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        try
        {
            PolicyEvaluationHttpContextHolder.Value = context;

            return this._defaultPolicyEvaluator.AuthenticateAsync(policy, context);
        }
        finally
        {
            PolicyEvaluationHttpContextHolder.Value = null;
        }
    }

    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context,
        object resource)
    {
        try
        {
            PolicyEvaluationHttpContextHolder.Value = context;

            return this._defaultPolicyEvaluator.AuthorizeAsync(policy, authenticationResult, context, resource);
        }
        finally
        {
            PolicyEvaluationHttpContextHolder.Value = null;
        }
    }
}