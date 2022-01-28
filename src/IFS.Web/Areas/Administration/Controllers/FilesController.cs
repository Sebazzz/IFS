// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FilesController.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Collections.Generic;
using System.Threading.Tasks;

using IFS.Web.Core;
using IFS.Web.Core.Upload;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using IFS.Web.Areas.Administration.Models;

using IFS.Web.Models;

namespace IFS.Web.Areas.Administration.Controllers {
    [Authorize(KnownPolicies.Administration, AuthenticationSchemes = KnownAuthenticationScheme.AdministrationScheme)]
    [Area(nameof(Administration))]
    public sealed class FilesController : Controller {
        private readonly IUploadedFileRepository _uploadedFileRepository;

        public FilesController(IUploadedFileRepository uploadedFileRepository) {
            this._uploadedFileRepository = uploadedFileRepository;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index() {
            IList<UploadedFile> allFiles = await this._uploadedFileRepository.GetFiles();

            FilesOverviewModel model = new FilesOverviewModel(allFiles);
            return this.View(model);
        }

        public async Task<IActionResult> Log(FileIdentifier id) {
            UploadedFile? file = await this._uploadedFileRepository.GetFile(id);

            if (file == null) {
                return this.NotFound();
            }

            return this.View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(FileIdentifier id) {
            this._uploadedFileRepository.Delete(id);

            return this.RedirectToAction("Index");
        }
    }
}
