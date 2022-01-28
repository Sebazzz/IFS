// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ErrorController.cs
//  Project         : IFS.Web
// ******************************************************************************

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using IFS.Web.Models;

namespace IFS.Web.Controllers {
    public sealed class ErrorController : Controller {
        [Route("error/http-500")]
        public IActionResult Error() {
            return this.View(this.GetModel());
        }

        [Route("error/http-401")]
        [Route("error/http-403")]
        public IActionResult AccessDenied() {
            return this.View(this.GetModel());
        }

        [Route("error/http-404")]
        public new IActionResult NotFound() {
            return this.View(this.GetModel());
        }

        private ErrorInformation GetModel() {
            IStatusCodeReExecuteFeature statusCodeFeature = this.HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            StatusCodeReExecuteFeature? specific = statusCodeFeature as StatusCodeReExecuteFeature;

            if (statusCodeFeature == null) {
                return new ErrorInformation {
                    OriginalPath = null,
                    QueryString = null
                };
            }

            return new ErrorInformation {
                OriginalPath = statusCodeFeature.OriginalPath,
                QueryString = specific?.OriginalQueryString
            };
        }
    }
}
