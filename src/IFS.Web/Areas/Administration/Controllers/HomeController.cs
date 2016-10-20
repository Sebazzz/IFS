// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HomeController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Controllers {
    using Core;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(KnownPolicies.Administration)]
    [Area(nameof(Administration))]
    public sealed class HomeController : Controller {
        public IActionResult Index() {
            return this.RedirectToAction("Index", "Files");
        }
    }
}
