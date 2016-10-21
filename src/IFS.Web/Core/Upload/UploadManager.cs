// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadManager.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    using Models;

    public class UploadManager : IUploadManager {
        private readonly ConcurrentDictionary<FileIdentifier, UploadProgress> _uploadsByFileIdentifier;

        private readonly IFileStore _fileStore;
        private readonly FileWriter _fileWriter;
        private readonly ILogger<UploadManager> _logger;

        public UploadManager(IFileStore fileStore, FileWriter fileWriter, ILogger<UploadManager> logger) {
            this._fileStore = fileStore;
            this._fileWriter = fileWriter;
            this._logger = logger;

            this._uploadsByFileIdentifier = new ConcurrentDictionary<FileIdentifier, UploadProgress>();
        }

        public async Task StoreAsync(FileIdentifier id, IFormFile file, DateTime expiration, CancellationToken cancellationToken) {
            this._logger.LogInformation(LogEvents.NewUpload, "New upload of file {0} to id {1}", file.FileName, id);

            try {
                await this.StoreMetadataAsync(id, file, expiration, cancellationToken).ConfigureAwait(false);
                await this.StoreDataAsync(id, file, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) {
                this._logger.LogWarning(LogEvents.UploadCancelled, ex, "Upload failed due to cancellation");

                this.TryCleanup(id);
            } catch (Exception ex) {
                this._logger.LogError(LogEvents.UploadFailed, ex, "Upload failed due to exception");

                this.TryCleanup(id);

                throw new UploadFailedException("Unable to complete upload", ex);
            }
        }

        public UploadProgress GetProgress(FileIdentifier id) {
            UploadProgress value;

            if (!this._uploadsByFileIdentifier.TryGetValue(id, out value)) {
                return null;
            }

            return value;
        }

        private async Task StoreDataAsync(FileIdentifier id, IFormFile file, CancellationToken cancellationToken) {
            // Register progress token
            UploadProgress progress = new UploadProgress {
                Current = 0,
                Total = file.Length
            };

            this._uploadsByFileIdentifier[id] = progress;

            // Copy with progress
            using (Stream outputStream = this._fileWriter.OpenWriteStream(this._fileStore.GetDataFile(id))) {
                using (Stream inputStream = file.OpenReadStream()) {
                    int read;
                    byte[] buffer = new byte[4096];
                    while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                        progress.Current += read;

                        await outputStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task StoreMetadataAsync(FileIdentifier id, IFormFile file, DateTime expiration, CancellationToken cancellationToken) {
            StoredMetadata metadata = new StoredMetadata {
                Expiration = expiration,
                UploadedOn = DateTime.UtcNow,
                OriginalFileName = Path.GetFileName(file.FileName)
            };

            using (Stream fileStream = this._fileWriter.OpenWriteStream(this._fileStore.GetMetadataFile(id))) {
                using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8)) {
                    await sw.WriteAsync(metadata.Serialize()).ConfigureAwait(false);

                    await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void TryCleanup(FileIdentifier id) {
            using (this._logger.BeginScope("Clean-up: {0}", id)) {
                try {
                    this._fileWriter.Delete(this._fileStore.GetDataFile(id));
                    this._fileWriter.Delete(this._fileStore.GetMetadataFile(id));
                } catch (Exception ex) {
                    this._logger.LogError(LogEvents.CleanupFailed, ex, "Unable to clean up file '{0}'", id);
                }
            }
        }


    }
}
