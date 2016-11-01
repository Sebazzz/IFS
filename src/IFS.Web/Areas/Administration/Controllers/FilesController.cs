// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FilesController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Controllers {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Core;
    using Core.Upload;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using Models;

    using Web.Models;

    [Authorize(KnownPolicies.Administration)]
    [Area(nameof(Administration))]
    public sealed class FilesController : Controller {
        private readonly IUploadedFileRepository _uploadedFileRepository;

        public FilesController(IUploadedFileRepository uploadedFileRepository) {
            this._uploadedFileRepository = uploadedFileRepository;
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index() {
            IList<UploadedFile> allFiles = await this._uploadedFileRepository.GetFiles();

            FilesOverviewModel model = new FilesOverviewModel {
                Files = allFiles
            };

            return this.View(model);
        }

        public async Task<IActionResult> Log(FileIdentifier id) {
            UploadedFile file = await this._uploadedFileRepository.GetFile(id);

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
