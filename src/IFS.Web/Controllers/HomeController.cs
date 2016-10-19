// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HomeController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using Microsoft.AspNetCore.Mvc;

    public sealed class HomeController : Controller {
        public IActionResult Index() {
            return this.RedirectToAction("Index", "Upload");
        }
    }
}
