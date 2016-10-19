// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using System.Threading.Tasks;

    using Core;
    using Core.Upload;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using Models;

    [Authorize(KnownPolicies.Upload, ActiveAuthenticationSchemes = KnownAuthenticationScheme.PassphraseScheme)]
    public sealed class UploadController : Controller {
        private readonly IUploadManager _uploadManager;
        private readonly IUploadedFileRepository _uploadedFileRepository;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IUploadManager uploadManager, IUploadedFileRepository uploadedFileRepository, ILogger<UploadController> logger) {
            this._uploadManager = uploadManager;
            this._logger = logger;
            this._uploadedFileRepository = uploadedFileRepository;
        }

        public IActionResult Index() {
            UploadModel uploadModel = new UploadModel {
                FileIdentifier = FileIdentifier.CreateNew()
            };

            return this.View(uploadModel);
        }

        [HttpGet]
        public async Task<IActionResult> Complete(FileIdentifier id) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file '{0}'", id);
                return this.NotFound("We lost it!");
            }

            return this.View(uploadedFile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Frame([FromForm] UploadModel model) {
            if (!this.ModelState.IsValid) {
                return this.View("FrameError", model);
            }

            await this._uploadManager.StoreAsync(model.FileIdentifier, model.File, this.HttpContext.RequestAborted);

            return this.View("FrameComplete", model);
        }

        [HttpGet]
        [Route("upload/tracker/{trackerId}/progress", Name = "UploadTrackerApi")]
        [PreventHttpCache]
        public IActionResult TrackerApi(FileIdentifier trackerId) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadProgress current = this._uploadManager.GetProgress(trackerId);
            if (current == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find upload by id {0}", trackerId);

                return this.NotFound("Cannot find upload with id: " + trackerId);
            }

            return this.Ok(current);
        }

        [HttpGet]
        [Route("upload/tracker/{trackerId}", Name = "UploadTracker")]
        [PreventHttpCache]
        public IActionResult Tracker(FileIdentifier trackerId) {
            UploadFileInProgressModel model = new UploadFileInProgressModel {
                FileIdentifier = trackerId,
                FileName = this._uploadManager.GetProgress(trackerId)?.FileName
            };

            return this.PartialView(model);
        }
    }
}
