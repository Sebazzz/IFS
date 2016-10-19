// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ErrorController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller {
        public IActionResult Error() {
            return this.View();
        }

        public IActionResult AccessDenied() {
            return this.View();
        }
    }
}
