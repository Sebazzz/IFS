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
        public async Task<IActionResult> DownloadFile(FileIdentifier id) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);
                return this.NotFound("We lost it!");
            }

            await this._fileAccessLogger.LogFileAccessAsync(uploadedFile, this.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? "Unknown");

            return this.File(uploadedFile.GetStream(), "application/octet-stream", uploadedFile.Metadata.OriginalFileName);
        }
    }
}
