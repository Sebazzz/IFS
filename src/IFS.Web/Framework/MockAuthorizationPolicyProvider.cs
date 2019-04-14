using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IFS.Web.Framework {
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Options;

    public class MockAuthorizationPolicyProvider : IAuthorizationPolicyProvider {
        private readonly DefaultAuthorizationPolicyProvider _defaultAuthorizationPolicyProvider;

        public MockAuthorizationPolicyProvider(IOptions<AuthorizationOptions> opts) {
            this._defaultAuthorizationPolicyProvider = new DefaultAuthorizationPolicyProvider(opts);
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName) {
            return this._defaultAuthorizationPolicyProvider.GetPolicyAsync(policyName);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() {
            return this._defaultAuthorizationPolicyProvider.GetDefaultPolicyAsync();
        }
    }
}
