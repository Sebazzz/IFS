// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HomeController.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Core;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IFS.Web.Areas.Administration.Controllers;

[Authorize(KnownPolicies.Administration, AuthenticationSchemes = KnownAuthenticationScheme.AdministrationScheme)]
[Area(nameof(Administration))]
public sealed class HomeController : Controller {
    public IActionResult Index() {
        return this.RedirectToAction("Index", "Files");
    }
}