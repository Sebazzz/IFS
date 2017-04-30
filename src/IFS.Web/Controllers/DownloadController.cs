// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using System.Threading.Tasks;

    using Core;
    using Core.Upload;
    using Core.Upload.Http;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using Models;

    public sealed class DownloadController : Controller {
        private readonly IUploadedFileRepository _uploadedFileRepository;
        private readonly IFileAccessLogger _fileAccessLogger;
        private readonly ILogger<DownloadController> _logger;

        public DownloadController(IUploadedFileRepository uploadedFileRepository, IFileAccessLogger fileAccessLogger, ILogger<DownloadController> logger) {
            this._logger = logger;
            this._fileAccessLogger = fileAccessLogger;
            this._uploadedFileRepository = uploadedFileRepository;
        }

        public IActionResult Index() {
            return this.NotFound();
        }

        [Route("download/file/{id}", Name = "DownloadFile")]
        [FileLock]
        public async Task<IActionResult> DownloadFileSplash(FileIdentifier id) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);

                this.Response.StatusCode = 404;
                return this.View("NotFound");
            }

            if (DirectDownloadClientDetector.IsDirectDownloadClient(this.Request.Headers["User-Agent"].ToString())) {
                return this.RedirectToRoute("DownloadFileRaw");
            }

            return this.View(uploadedFile);
        }

        [Route("download/file/{id}/raw", Name = "DownloadFileRaw")]
        public async Task<IActionResult> DownloadFileRaw(FileIdentifier id) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);
                return this.NotFound("404: File has not been found or download link has expired");
            }

            await this._fileAccessLogger.LogFileAccessAsync(uploadedFile, this.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? "Unknown");
            return this.File(uploadedFile.GetStream(), "application/octet-stream", uploadedFile.Metadata.OriginalFileName);
        }
    }
}
