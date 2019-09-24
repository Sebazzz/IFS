// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : DownloadController.cs
//  Project         : IFS.Web
// ******************************************************************************

using IFS.Web.Framework.Filters;
using IFS.Web.Framework.Middleware.Fail2Ban;

namespace IFS.Web.Controllers {
    using System.Threading.Tasks;

    using Core;
    using Core.Crypto;
    using Core.Download;
    using Core.Upload;
    using Core.Upload.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using Models;

    public sealed class DownloadController : Controller {
        private readonly IUploadedFileRepository _uploadedFileRepository;
        private readonly IFileAccessLogger _fileAccessLogger;
        private readonly ITransitPasswordProtector _passwordProtector;
        private readonly ILogger<DownloadController> _logger;

        public DownloadController(IUploadedFileRepository uploadedFileRepository, IFileAccessLogger fileAccessLogger, ITransitPasswordProtector passwordProtector, ILogger<DownloadController> logger) {
            this._logger = logger;
            this._passwordProtector = passwordProtector;
            this._fileAccessLogger = fileAccessLogger;
            this._uploadedFileRepository = uploadedFileRepository;
        }

        public IActionResult Index() {
            return this.NotFound();
        }

        [Route("download/file/{id}", Name = "DownloadFile")]
        [Fail2BanModelState(nameof(DownloadPasswordModel.Password))]
        [FileLock]
        public async Task<IActionResult> DownloadFileSplash(FileIdentifier id) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile? uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);

                this.Response.StatusCode = 404;
                return this.View("NotFound");
            }

            if (this.IsDirectDownloadClient()) {
                // Only support POST requests for direct download in case of password protected file
                if (uploadedFile.HasDownloadSecurity()) {
                    return this.BadRequest("To download password protected files with curl or wget, please make a POST request with 'password' parameter");
                }

                return this.RedirectToRoute("DownloadFileRaw");
            }

            if (uploadedFile.HasDownloadSecurity()) {
                return this.View("DownloadFilePassword", new DownloadPasswordModel());
            }

            return this.View(uploadedFile);
        }

        [Route("download/file/{id}", Name = "DownloadFile")]
        [Fail2BanModelState(nameof(DownloadPasswordModel.Password))]
        [FileLock]
        [HttpPost]
        public async Task<IActionResult> DownloadFileSplash(FileIdentifier id, DownloadPasswordModel passwordInfo) {
            await Task.Delay(500, this.HttpContext.RequestAborted);

            if (!this.ModelState.IsValid) {
                await Task.Delay(500, this.HttpContext.RequestAborted);

                return this.View("DownloadFilePassword", passwordInfo);
            }

            // Get the file
            UploadedFile? uploadedFile = await this._uploadedFileRepository.GetFile(id);

            if (uploadedFile == null || !uploadedFile.HasDownloadSecurity()) {
                // How? Don't know, but execute regular procedure.
                return await this.DownloadFileSplash(id);
            }

            // Directly validate password and return file for wget
            if (this.IsDirectDownloadClient()) {
                return await this.DownloadFileRaw(id, passwordInfo.Password!, null);
            }

            // In web browser case validate password and show regular download view
            bool passwordIsValid = uploadedFile.Metadata.DownloadSecurity?.Verify(passwordInfo.Password) != false;
            if (!passwordIsValid) {
                await this.MakeBadPasswordDelayTask();

                this.ModelState.AddModelError(nameof(passwordInfo.Password), "Invalid password");
                this.HttpContext.RecordFail2BanFailure();

                return this.View("DownloadFilePassword", passwordInfo);
            }

            this.HttpContext.RecordFail2BanSuccess();

            // Show download prompt, protect password to prevent reuse
            this.ViewBag.ProtectedPassword = this._passwordProtector.Protect(passwordInfo.Password!);
            return this.View(uploadedFile);
        }
        

        [Route("download/file/{id}/raw", Name = "DownloadFileRaw")]
        [FileLock]
        public async Task<IActionResult> DownloadFileRaw(FileIdentifier id, string password, [Bind(Prefix="_")]string? protectedPassword) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            // Get file
            UploadedFile? uploadedFile = await this._uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file for download '{0}'", id);
                return this.NotFound("404: File has not been found or download link has expired");
            }

            // Decrypt password if given
            if (password == null && protectedPassword != null) {
                password = this._passwordProtector.Unprotect(protectedPassword);

                // Handle no password (protectedPassword might be expired)
                if (password == null)
                {
                    DownloadPasswordModel passwordInfo = new DownloadPasswordModel();
                    this.ModelState.AddModelError(nameof(passwordInfo.Password), "Password authentication expired");
                    return this.View("DownloadFilePassword", passwordInfo);
                }
            }

            // Direct-download support
            if (uploadedFile.HasDownloadSecurity()) {
                if (password == null) {
                    await this.MakeBadPasswordDelayTask();
                    return this.StatusCode(401, "This resource is protected by a password");
                }

                if (uploadedFile.Metadata.DownloadSecurity?.Verify(password) == false) {
                    await this.MakeBadPasswordDelayTask();
                    this.HttpContext.RecordFail2BanFailure();
                    return this.StatusCode(401, "This resource is protected by a password");
                }

                this.HttpContext.RecordFail2BanSuccess();
            }

            // Do download
            await this._fileAccessLogger.LogFileAccessAsync(uploadedFile, this.HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString() ?? "Unknown");

            return this.File(DownloadStreamFactory.GetDownloadStream(uploadedFile, password), "application/octet-stream", uploadedFile.Metadata.OriginalFileName);
        }

        private bool IsDirectDownloadClient() {
            return DirectDownloadClientDetector.IsDirectDownloadClient(this.Request.Headers["User-Agent"].ToString());
        }

        private Task MakeBadPasswordDelayTask() {
            return Task.Delay(1500, this.HttpContext.RequestAborted);
        }
    }
}
