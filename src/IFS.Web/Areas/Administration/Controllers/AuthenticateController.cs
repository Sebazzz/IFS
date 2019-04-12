// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticateController.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Framework.Filters;
using IFS.Web.Framework.Middleware.Fail2Ban;

namespace IFS.Web.Areas.Administration.Controllers {
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Core;
    using Core.Authentication;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    using Models;

    [Area(nameof(Administration))]
    public sealed class AuthenticateController : Controller {
        private readonly IAdministrationAuthenticationProvider _authenticationProvider;

        public AuthenticateController(IAdministrationAuthenticationProvider authenticationProvider) {
            this._authenticationProvider = authenticationProvider;
        }

        public IActionResult Index() {
            return this.RedirectToAction("Login");
        }

        [Fail2BanModelState(nameof(LoginModel.Password))]
        public async Task<IActionResult> Login(string returnUrl) {
            if (this.User.Identity.IsAuthenticated) {
                await this.HttpContext.SignOutAsync(KnownAuthenticationScheme.PassphraseScheme);

                return this.RedirectToAction("Index");
            }

            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logoff() {
            await this.HttpContext.SignOutAsync(KnownAuthenticationScheme.AdministrationScheme);

            return this.RedirectToAction("Index", "Home", new {area = ""});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Fail2BanModelState(nameof(LoginModel.Password))]
        public async Task<IActionResult> Login(LoginModel model) {
            if (model == null) {
                return this.View();
            }

            if (!this.ModelState.IsValid) {
                return this.View(model);
            }

            bool isValid = this._authenticationProvider.IsValidCredentials(model.UserName, model.Password);

            if (!isValid) {
                this.ModelState.AddModelError(nameof(model.Password), "Invalid username or password. Please try again.");
                this.HttpContext.RecordFail2BanFailure();
                return this.View();
            }

            ClaimsIdentity userIdentity = new ClaimsIdentity(KnownAuthenticationScheme.AdministrationScheme);
            userIdentity.AddClaims(new[] {
                new Claim(ClaimTypes.Name, model.UserName, ClaimValueTypes.String, "https://ifs")
            });

            ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);

            AuthenticationProperties authenticationOptions = new AuthenticationProperties {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = false
            };

            this.HttpContext.RecordFail2BanSuccess();
            await this.HttpContext.SignInAsync(KnownAuthenticationScheme.AdministrationScheme, userPrincipal, authenticationOptions);

            string returnUrl = String.IsNullOrEmpty(model.ReturnUrl) ? this.Url.Action("Index", "Upload") : model.ReturnUrl;

            return this.Redirect(returnUrl);
        }
    }
}
