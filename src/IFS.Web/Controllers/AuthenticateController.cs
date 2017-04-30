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
    using System.Threading.Tasks;

    using Core;
    using Core.Authentication;

    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using Models;

    public sealed class AuthenticateController : Controller {
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly IOptions<AuthenticationOptions> _authenticateOptions;

        public AuthenticateController(IAuthenticationProvider authenticationProvider, IOptions<AuthenticationOptions> authenticateOptions) {
            this._authenticationProvider = authenticationProvider;
            this._authenticateOptions = authenticateOptions;
        }

        public IActionResult Index() {
            return this.RedirectToAction("Login");
        }

        public IActionResult Login(string returnUrl) {
            if (this.User.Identity.IsAuthenticated) {
                return this.RedirectToAction("Index", "Upload");
            }

            LoginModel loginModel = new LoginModel();
            this.SetHelpText(loginModel);

            return this.View(loginModel);
        }

        private void SetHelpText(LoginModel loginModel) {
            loginModel.HelpText = this._authenticateOptions.Value.LoginHelpText;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model) {
            // Due to MVC model binding, model will never be null here
            if (!this.ModelState.IsValid) {
                this.SetHelpText(model);
                return this.View(model);
            }

            // Validate password
            bool isValid = this._authenticationProvider.IsValidPassphrase(model?.Passphrase);

            if (!isValid) {
                this.SetHelpText(model);
                this.ModelState.AddModelError(nameof(model.Passphrase), "Invalid passphrase. Please try again.");
                return this.View(model);
            }

            // Create log-in
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

            string returnUrl = String.IsNullOrEmpty(model?.ReturnUrl) ? this.Url.Action("Index", "Upload") : model.ReturnUrl;
            return this.Redirect(returnUrl);
        }
    }
}
