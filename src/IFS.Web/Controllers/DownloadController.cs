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
                if (uploadedFile.HasDownloadSecurity()) {
                    return this.Forbid(); // Not implemented yet
                }

                return this.RedirectToRoute("DownloadFileRaw");
            }

            if (uploadedFile.HasDownloadSecurity()) {
                return this.View("DownloadFilePassword", new DownloadPasswordModel());
            }

            return this.View(uploadedFile);
        }

        [Route("download/file/{id}", Name = "DownloadFile")]
        [FileLock]
        [HttpPost]
        public async Task<IActionResult> DownloadFileSplash(FileIdentifier id, DownloadPasswordModel passwordInfo) {
            await Task.Delay(500, this.HttpContext.RequestAborted);

            if (!this.ModelState.IsValid) {
                await Task.Delay(500, this.HttpContext.RequestAborted);

                return this.View("DownloadFilePassword", passwordInfo);
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (!uploadedFile.HasDownloadSecurity()) {
                // How? Don't know, but execute regular procedure.
                return await this.DownloadFileSplash(id);
            }

            bool passwordIsValid = uploadedFile.Metadata.DownloadSecurity.Verify(passwordInfo.Password);
            if (!passwordIsValid) {
                await Task.Delay(1500, this.HttpContext.RequestAborted);

                this.ModelState.AddModelError(nameof(passwordInfo.Password), "Invalid password");

                return this.View("DownloadFilePassword", passwordInfo);
            }

            this.ViewBag.HashedPassword = uploadedFile.Metadata.DownloadSecurity.HashedPassword;
            return this.View(uploadedFile);
        }

        [Route("download/file/{id}/raw", Name = "DownloadFileRaw")]
        [FileLock]
        public async Task<IActionResult> DownloadFileRaw(FileIdentifier id, string hashedPassword1, [Bind(Prefix="_")]string hashedPassword2) {
            string hashedPassword = hashedPassword2 ?? hashedPassword1;

            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);
                return this.NotFound("404: File has not been found or download link has expired");
            }

            if (uploadedFile.HasDownloadSecurity()) {
                if (uploadedFile.Metadata.DownloadSecurity.HashedPassword != hashedPassword) {
                    return this.Forbid();
                }
            }

            await this._fileAccessLogger.LogFileAccessAsync(uploadedFile, this.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? "Unknown");

            return this.File(uploadedFile.GetStream(), "application/octet-stream", uploadedFile.Metadata.OriginalFileName);
        }
    }
}
