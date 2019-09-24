// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : MockAuthorizationPolicyProvider.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IFS.Web.Framework
{
    public sealed class MockAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _defaultAuthorizationPolicyProvider;

        public MockAuthorizationPolicyProvider(IOptions<AuthorizationOptions> opts)
        {
            this._defaultAuthorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(opts);
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return this._defaultAuthorizationPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            return this._defaultAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return this._defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();
        }
    }
}