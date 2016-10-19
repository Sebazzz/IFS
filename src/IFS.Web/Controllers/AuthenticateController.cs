// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticateController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Core;
    using Core.Authentication;

    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Mvc;

    using Models;

    public sealed class AuthenticateController : Controller {
        private readonly IAuthenticationProvider _authenticationProvider;

        public AuthenticateController(IAuthenticationProvider authenticationProvider) {
            this._authenticationProvider = authenticationProvider;
        }

        public IActionResult Index() {
            return this.RedirectToAction("Login");
        }

        public IActionResult Login(string returnUrl) {
            if (this.User.Identity.IsAuthenticated) {
                return this.RedirectToAction("Index", "Upload");
            }

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model) {
            if (model == null) {
                return this.View();
            }

            bool isValid = this._authenticationProvider.IsValidPassphrase(model.Passphrase);

            if (!isValid) {
                this.ModelState.AddModelError(nameof(model.Passphrase), "Invalid passphrase. Please try again.");
                return this.View();
            }

            ClaimsIdentity userIdentity = new ClaimsIdentity(KnownAuthenticationScheme.PassphraseScheme);
            userIdentity.AddClaims(new[] {
                new Claim(ClaimTypes.Name, KnownPolicies.Upload, ClaimValueTypes.String, "https://ifs")
            });

            ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);

            AuthenticationProperties authenticationOptions = new AuthenticationProperties {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = false
            };

            await this.HttpContext.Authentication.SignInAsync(KnownAuthenticationScheme.PassphraseScheme, userPrincipal, authenticationOptions);

            string returnUrl = String.IsNullOrEmpty(model.ReturnUrl) ? this.Url.Action("Index", "Upload") : model.ReturnUrl;

            return this.Redirect(returnUrl);
        }
    }
}
