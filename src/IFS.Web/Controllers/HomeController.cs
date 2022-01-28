// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : HomeController.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Mvc;

namespace IFS.Web.Controllers;

public sealed class HomeController : Controller {
    public IActionResult Index() {
        return this.RedirectToAction("Index", "Upload");
    }
}