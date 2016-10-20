// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ExpireFileRemoval.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Hangfire;

    using Microsoft.Extensions.Logging;

    public sealed class ExpiredFileRemovalJob {
        private readonly ILogger<ExpiredFileRemovalJob> _logger;
        private readonly IUploadedFileRepository _uploadedFileRepository;

        public ExpiredFileRemovalJob(IUploadedFileRepository uploadedFileRepository, ILogger<ExpiredFileRemovalJob> logger) {
            this._uploadedFileRepository = uploadedFileRepository;
            this._logger = logger;
        }

        [AutomaticRetry(LogEvents = true, Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task Execute(IJobCancellationToken cancellationToken) {
            using (this._logger.BeginScope("Executing removal of expired files")) {
                IList<UploadedFile> uploadedFiles = await this._uploadedFileRepository.GetFiles().ConfigureAwait(false);

                foreach (UploadedFile uploadedFile in uploadedFiles) {
                    this.ProcessSingleFile(uploadedFile);
                }
            }
        }

        private void ProcessSingleFile(UploadedFile uploadedFile) {
            if (DateTime.UtcNow > uploadedFile.Metadata.Expiration) {
                this._logger.LogInformation(LogEvents.UploadExpired, "Removing expired uploaded file {0}", uploadedFile.Id);

                try {
                    this._uploadedFileRepository.Delete(uploadedFile.Id);
                }
                catch (Exception ex) {
                    this._logger.LogError(LogEvents.CleanupFailed, ex, "Unable to remove expired uploaded file {0}", uploadedFile.Id);
                }
            }
        }
    }
}
