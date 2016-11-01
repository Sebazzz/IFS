// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Controllers {
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    using Core;
    using Core.Upload;

    using Humanizer;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Logging;

    using Models;

    [Authorize(KnownPolicies.Upload, ActiveAuthenticationSchemes = KnownAuthenticationScheme.PassphraseScheme)]
    public sealed class UploadController : Controller {
        private readonly IUploadProgressManager _uploadProgressManager;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IUploadProgressManager uploadProgressManager, ILogger<UploadController> logger) {
            this._uploadProgressManager = uploadProgressManager;
            this._logger = logger;
        }

        public IActionResult Index() {
            Func<TimeSpan, SelectListItem> createItem = 
                timespan => new SelectListItem {
                    Value = (DateTime.UtcNow + timespan).ToString("O"),
                    Text = timespan.Humanize()
                };

            Func<int, SelectListItem> createMonthItem =
                month => new SelectListItem {
                    Value = CultureInfo.CurrentCulture.Calendar.AddMonths(DateTime.UtcNow, month).ToString("O"),
                    Text = $"{month} months"
                };

            UploadModel uploadModel = new UploadModel {
                FileIdentifier = FileIdentifier.CreateNew(),
                Expiration = DateTime.UtcNow.AddDays(7),
                AvailableExpiration = new[] {
                    createItem(TimeSpan.FromHours(1)),
                    createItem(TimeSpan.FromHours(4)),
                    createItem(TimeSpan.FromHours(8)),
                    createItem(TimeSpan.FromDays(1)),
                    createItem(TimeSpan.FromDays(2)),
                    createItem(TimeSpan.FromDays(7)),
                    createMonthItem(1),
                    createMonthItem(2),
                    createMonthItem(3),
                    createMonthItem(6),
                    createItem(TimeSpan.FromDays(CultureInfo.CurrentCulture.Calendar.GetDaysInYear(DateTime.UtcNow.Year))),
                }
            };

            return this.View(uploadModel);
        }

        [HttpGet]
        public async Task<IActionResult> Complete(FileIdentifier id, [FromServices] IUploadedFileRepository uploadedFileRepository) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadedFile uploadedFile = await uploadedFileRepository.GetFile(id);
            if (uploadedFile == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find uploaded file '{0}'", id);
                return this.NotFound("We lost it!");
            }

            return this.View(uploadedFile);
        }

        [HttpPost]
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
        [Route("upload/tracker/{trackerId}/progress", Name = "UploadTrackerApi")]
        [PreventHttpCache]
        public IActionResult TrackerApi(FileIdentifier trackerId) {
            if (!this.ModelState.IsValid) {
                return this.BadRequest();
            }

            UploadProgress current = this._uploadProgressManager.GetProgress(trackerId);
            if (current == null) {
                this._logger.LogWarning(LogEvents.UploadNotFound, "Unable to find upload by id {0}", trackerId);

                return this.NotFound("Cannot find upload with id: " + trackerId);
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
        [Route("upload/tracker/{trackerId}", Name = "UploadTracker")]
        [PreventHttpCache]
        public IActionResult Tracker(FileIdentifier trackerId) {
            UploadFileInProgressModel model = new UploadFileInProgressModel {
                FileIdentifier = trackerId,
                FileName = this._uploadProgressManager.GetProgress(trackerId)?.FileName
            };

            return this.PartialView(model);
        }
    }
}
