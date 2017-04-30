// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadLinkController.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Areas.Administration.Controllers {
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Core;
    using Core.ModelFactory;
    using Core.Upload;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using Web.Models;

    [Authorize(KnownPolicies.Administration)]
    [Area(nameof(Administration))]
    public class UploadLinkController : Controller {
        private readonly IFileStore _fileStore;
        private readonly IFileWriter _fileWriter;

        public UploadLinkController(IFileStore fileStore, IFileWriter fileWriter) {
            this._fileStore = fileStore;
            this._fileWriter = fileWriter;
        }

        // GET: /<controller>/
        [HttpGet]
        public IActionResult Index() {
            UploadLinkModel model = UploadModelFactory.CreateLink();
            return this.View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormCollection form) {
            UploadLinkModel model = UploadModelFactory.CreateLink();

            if (!await this.TryUpdateModelAsync(model)) {
                return this.View(model);
            }

            await this.ReserveUploadAsync(model);

            return this.View("LinkCreated", model);
        }

        private async Task ReserveUploadAsync(UploadLinkModel model) {
            CancellationToken cancellationToken = this.HttpContext.RequestAborted;
            StoredMetadata metadata = new StoredMetadata {
                Expiration = model.Expiration,
                UploadedOn = DateTime.UtcNow,
                IsReservation = true,
                Sender = model.Sender
            };

            // Ensure no empty contact information
            if (model.Sender != null && String.IsNullOrEmpty(model.Sender.Name) && String.IsNullOrEmpty(model.Sender.EmailAddress)) {
                metadata.Sender = null;
            }

            // Write away
            using (Stream fileStream = this._fileWriter.OpenWriteStream(this._fileStore.GetMetadataFile(model.FileIdentifier))) {
                using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8)) {
                    await sw.WriteAsync(metadata.Serialize()).ConfigureAwait(false);

                    await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
