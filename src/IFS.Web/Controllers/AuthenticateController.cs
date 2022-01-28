// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : AuthenticateController.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Framework.Filters;
using IFS.Web.Framework.Middleware.Fail2Ban;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

using IFS.Web.Core;
using IFS.Web.Core.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using IFS.Web.Models;

namespace IFS.Web.Controllers;

// Force authentication, or we will never redirect
[Authorize(KnownPolicies.Upload, AuthenticationSchemes = KnownAuthenticationScheme.PassphraseScheme)]
[AllowAnonymous]
public sealed class AuthenticateController : Controller {
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IOptions<Core.Authentication.AuthenticationOptions> _authenticateOptions;

    public AuthenticateController(IAuthenticationProvider authenticationProvider, IOptions<Core.Authentication.AuthenticationOptions> authenticateOptions) {
        this._authenticationProvider = authenticationProvider;
        this._authenticateOptions = authenticateOptions;
    }

    public IActionResult Index() {
        return this.RedirectToAction("Login");
    }

    [Fail2BanModelState(nameof(LoginModel.Passphrase))]
    [StaticAuthenticationAction]
    [ActionName("Login")]
    public IActionResult LoginStatic(string returnUrl) {
        if (this.User.Identity.IsAuthenticated) {
            return this.RedirectToAction("Index", "Upload");
        }

        LoginModel loginModel = new LoginModel();
        this.SetHelpText(loginModel);

        return this.View(loginModel);
    }

    [OpenIdAuthenticationAction]
    [ActionName("Login")]
    public IActionResult LoginOpenId(string returnUrl) {
        if (this.User.Identity.IsAuthenticated) {
            return this.RedirectToAction("Index", "Upload");
        }

        LoginModel loginModel = new LoginModel();
        this.SetHelpText(loginModel);

        return this.View("LoginOpenId", loginModel);
    }

    private void SetHelpText(LoginModel loginModel) {
        loginModel.HelpText = this._authenticateOptions.Value.LoginHelpText;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [OpenIdAuthenticationAction]
    [ActionName("Login")]
    public async Task LoginOpenId(string returnUrl, IFormCollection form) {
        await this.HttpContext.ChallengeAsync(KnownAuthenticationScheme.OpenIdConnect.PassphraseScheme, new OpenIdConnectChallengeProperties {
            Prompt = "Sign in to upload files",
            RedirectUri = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Fail2BanModelState(nameof(LoginModel.Passphrase))]
    [StaticAuthenticationAction]
    [ActionName("Login")]
    public async Task<IActionResult> LoginStatic(LoginModel model) {
        // Due to MVC model binding, model will never be null here
        if (!this.ModelState.IsValid) {
            this.SetHelpText(model);
            return this.View(model);
        }

        // Validate password
        bool isValid = this._authenticationProvider.IsValidPassphrase(model.Passphrase);

        if (!isValid) {
            this.HttpContext.RecordFail2BanFailure();
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

        this.HttpContext.RecordFail2BanSuccess();
        await this.HttpContext.SignInAsync(KnownAuthenticationScheme.PassphraseScheme, userPrincipal, authenticationOptions);

        string returnUrl = String.IsNullOrEmpty(model?.ReturnUrl) ? this.Url.Action("Index", "Upload") : model.ReturnUrl;
        return this.Redirect(returnUrl);
    }
}