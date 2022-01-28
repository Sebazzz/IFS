using System;
using System.Security.Claims;
using System.Threading.Tasks;

using IFS.Web.Core;
using IFS.Web.Core.ModelFactory;
using IFS.Web.Core.Upload;
using IFS.Web.Framework.Filters;
using Humanizer;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using IFS.Web.Models;

// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers;

[Authorize(KnownPolicies.Upload, AuthenticationSchemes = KnownAuthenticationScheme.PassphraseScheme)]
public sealed class UploadController : Controller {
    private readonly IUploadedFileRepository _uploadedFileRepository;
    private readonly IUploadProgressManager _uploadProgressManager;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IUploadProgressManager uploadProgressManager, IUploadedFileRepository uploadedFileRepository, ILogger<UploadController> logger) {
        this._uploadProgressManager = uploadProgressManager;
        this._logger = logger;
        this._uploadedFileRepository = uploadedFileRepository;
    }

    [AutoFillSenderInformation]
    public IActionResult Index() {
        UploadModel uploadModel = UploadModelFactory.Create();

        return this.View(uploadModel);
    }

    [HttpGet("upload/{id}", Name = "UploadFile")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadFileHandler(FileIdentifier id) {
        var file = await this._uploadedFileRepository.GetFileReservation(id);

        if (file == null) {
            this.Response.StatusCode = 404 /* Not Found */;
            return this.View("UploadLinkExpired");
        }

        // Set-up authentication
        ClaimsIdentity userIdentity = new ClaimsIdentity(KnownAuthenticationScheme.PassphraseScheme);
        userIdentity.AddClaims(new[] {
            new Claim(ClaimTypes.Name, KnownPolicies.Upload, ClaimValueTypes.String, "https://ifs"),
            new Claim(KnownClaims.RestrictionId, id.ToString(), ClaimValueTypes.String, "https://ifs"),
        });

        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);

        AuthenticationProperties authenticationOptions = new AuthenticationProperties {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
            IsPersistent = false
        };

        await this.HttpContext.SignInAsync(KnownAuthenticationScheme.PassphraseScheme, userPrincipal, authenticationOptions);

        // Create model for upload
        UploadModel uploadModel = UploadModelFactory.Create();
        uploadModel.FileIdentifier = file.Id;
        uploadModel.Expiration = file.Metadata.Expiration;
        uploadModel.IsReservation = true;
        uploadModel.Sender = file.Metadata.Sender;

        return this.View("Index", uploadModel);
    }

    [HttpGet]
    public async Task<IActionResult> Complete(FileIdentifier id, [FromServices] IUploadedFileRepository uploadedFileRepository) {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        UploadedFile? uploadedFile = await uploadedFileRepository.GetFile(id);
        if (uploadedFile == null) {
            this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file '{0}'", id);
            return this.NotFound("A system error occurred - unable to find just uploaded file");
        }

        if (this.User.HasClaim(KnownClaims.RestrictionId, id.ToString())) {
            await this.HttpContext.SignOutAsync(KnownAuthenticationScheme.PassphraseScheme);
        }

        return this.View(uploadedFile);
    }

    [HttpPost]
    [Route("upload/frame/{fileIdentifier}", Name = "AfterUploadCompletionFrame")]
    public IActionResult Frame(FileIdentifier id, UploadErrorsModel model) {
        if (model?.Errors != null) {
            foreach (string modelError in model.Errors) {
                this.ModelState.AddModelError(modelError, modelError);
            }
        }

        if (!this.ModelState.IsValid) {
            return this.View("FrameError", model);
        }

        return this.View("FrameComplete", id);
    }

    [HttpGet]
    [Route("upload/tracker/{fileIdentifier}/progress", Name = "UploadTrackerApi")]
    [PreventHttpCache]
    public IActionResult TrackerApi(FileIdentifier fileIdentifier) {
        if (!this.ModelState.IsValid) {
            return this.BadRequest();
        }

        UploadProgress? current = this._uploadProgressManager.GetProgress(fileIdentifier);
        if (current == null) {
            this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find upload by id {0}", fileIdentifier);

            return this.NotFound("Cannot find upload with id: " + fileIdentifier);
        }

        UploadProgressModel model = new UploadProgressModel {
            Current = current.Current,
            Total = current.Total,
            FileName = current.FileName,
            Percent = (int) Math.Round(((double) current.Current / current.Total) * 100),
            Performance = current.Current.Bytes().Per(DateTime.UtcNow - current.StartTime).Humanize("#.##")
        };

        return this.Ok(model);
    }

    [HttpGet]
    [Route("upload/tracker/{fileIdentifier}", Name = "UploadTracker")]
    [PreventHttpCache]
    public IActionResult Tracker(FileIdentifier fileIdentifier) {
        UploadFileInProgressModel model = new UploadFileInProgressModel {
            FileIdentifier = fileIdentifier,
            FileName = this._uploadProgressManager.GetProgress(fileIdentifier)?.FileName
        };

        return this.PartialView(model);
    }
}