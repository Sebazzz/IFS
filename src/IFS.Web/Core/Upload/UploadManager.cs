// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadManager.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Humanizer;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;

    using Models;

    public interface IUploadManager {
        Task StoreAsync(FileIdentifier id, MultipartReader reader, CancellationToken cancellationToken);
    }


    public interface IUploadProgressManager {
        UploadProgress GetProgress(FileIdentifier id);

        void SetProgress(FileIdentifier id, UploadProgress uploadProgress);
    }

    public class UploadProgressManager : IUploadProgressManager {
        private readonly ConcurrentDictionary<FileIdentifier, UploadProgress> _uploadsByFileIdentifier;

        public UploadProgressManager() {
            this._uploadsByFileIdentifier = new ConcurrentDictionary<FileIdentifier, UploadProgress>();
        }

        public UploadProgress GetProgress(FileIdentifier id) {
            UploadProgress value;

            if (!this._uploadsByFileIdentifier.TryGetValue(id, out value)) {
                return null;
            }

            return value;
        }

        public void SetProgress(FileIdentifier id, UploadProgress uploadProgress) {
            this._uploadsByFileIdentifier[id] = uploadProgress;
        }
    }


    public class UploadManager : IUploadManager {
        private readonly IFileStore _fileStore;
        private readonly IFileWriter _fileWriter;
        private readonly IUploadProgressManager _uploadProgressManager;
        private readonly ILogger<UploadManager> _logger;

        public UploadManager(IFileStore fileStore, IFileWriter fileWriter, IUploadProgressManager uploadProgressManager, ILogger<UploadManager> logger) {
            this._fileStore = fileStore;
            this._fileWriter = fileWriter;
            this._logger = logger;
            this._uploadProgressManager = uploadProgressManager;
        }

        public async Task StoreAsync(FileIdentifier id,  MultipartReader reader, CancellationToken cancellationToken) {
            // We need to manually read each part of the request. The browser may do as it likes and send whichever part first, 
            // which means we have to build up the metadata incrementally and can only write it later

            StoredMetadataFactory metadataFactory = new StoredMetadataFactory();
            StoredMetadata metadata = null;

            try {
                MultipartSection section = await reader.ReadNextSectionAsync(cancellationToken);

                while (section != null) {
                    await this.ProcessSectionAsync(id, section, metadataFactory, cancellationToken);

                    if (metadataFactory.IsComplete() && metadata == null) {
                        metadata = metadataFactory.Build();

                        await this.StoreMetadataAsync(id, metadata, cancellationToken);
                    }

                    section = await reader.ReadNextSectionAsync(cancellationToken);
                }

                if (metadata == null) {
                    throw new InvalidOperationException("Metadata is incomplete - file is not uploaded or expiration missing");
                }

                this._logger.LogInformation(LogEvents.NewUpload, "Completed: New upload of file {0} to id {1} [{2:s}]", metadata.OriginalFileName, id, DateTime.UtcNow);
            }
            catch (OperationCanceledException ex) {
                this._logger.LogWarning(LogEvents.UploadCancelled, ex, "Upload failed due to cancellation");

                this.TryCleanup(id);
            }
            catch (InvalidDataException ex) {
                this._logger.LogError(LogEvents.UploadFailed, ex, "Upload failed due to file size exceeding");

                this.TryCleanup(id);

                throw new UploadFailedException("File size exceeded of " + reader.BodyLengthLimit.GetValueOrDefault().Bytes().Megabytes);
            } catch (Exception ex) {
                this._logger.LogError(LogEvents.UploadFailed, ex, "Upload failed due to exception");

                this.TryCleanup(id);

                throw new UploadFailedException("Unable to complete upload", ex);
            }
        }

        private async Task ProcessSectionAsync(FileIdentifier id, MultipartSection section, StoredMetadataFactory metadataFactory, CancellationToken cancellationToken) {
            ContentDispositionHeaderValue contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);

            if (contentDisposition.FileName != null) {
                await this.ProcessFileSectionAsync(id, section, metadataFactory, cancellationToken, contentDisposition);
                return;
            }

            await this.ProcessFormSectionAsync(id, section, metadataFactory, contentDisposition);
        }

        private async Task ProcessFormSectionAsync(FileIdentifier id, MultipartSection section, StoredMetadataFactory metadataFactory, ContentDispositionHeaderValue contentDisposition) {
            string cleanName = HeaderUtilities.RemoveQuotes(contentDisposition.Name);

            switch (cleanName) {
                case nameof(UploadModel.Expiration):
                    using (StreamReader sr = new StreamReader(section.Body)) {
                        string dateTimeRaw = await sr.ReadToEndAsync();

                        // MVC we send date as roundtrip
                        metadataFactory.SetExpiration(DateTime.ParseExact(dateTimeRaw, "o", CultureInfo.InvariantCulture));
                    }
                    return;

                case nameof(UploadModel.FileIdentifier):
                    using (StreamReader sr = new StreamReader(section.Body)) {
                        string rawId = await sr.ReadToEndAsync();
                        FileIdentifier formId = FileIdentifier.FromString(rawId);

                        if (formId != id) {
                            throw new InvalidOperationException($"ID mismatch: '{formId}' (received) != '{id}' (expected)");
                        }
                    }
                    return;

                // Browsers don't actually send the file size in the request, but we can derive it from the Content-Length.
                // However, that is not very accurate and if we use some javascript to send a more accurate file size, we use that.
                case nameof(UploadModel.SuggestedFileSize):
                    using (StreamReader sr = new StreamReader(section.Body)) {
                        long size;
                        if (Int64.TryParse(await sr.ReadToEndAsync(), out size) && size > 0) {
                            this.GetProgressObject(id).Total = size;
                        }
                    }
                    return;

                default:
                    this._logger.LogWarning(LogEvents.UploadIncomplete, "{0}: Unknown form field '{1}'", id, contentDisposition.Name);
                    break;
            }
        }

        private UploadProgress GetProgressObject(FileIdentifier id) {
            var progressObject = this._uploadProgressManager.GetProgress(id);

            if (progressObject == null) {
                progressObject = new UploadProgress();

                Debug.Fail("Unable to retrieve progress object - which should have been set by the handler.");
            }

            return progressObject;
        }

        private async Task ProcessFileSectionAsync(FileIdentifier id, MultipartSection section, StoredMetadataFactory metadataFactory, CancellationToken cancellationToken, ContentDispositionHeaderValue contentDisposition) {
            string cleanName = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
            string fileName = HeaderUtilities.RemoveQuotes(contentDisposition.FileName);

            if (cleanName == nameof(UploadModel.File)) {
                this._logger.LogInformation(LogEvents.NewUpload, "New upload of file {0} to id {1} [{2:s}]", fileName, id, DateTime.UtcNow);

                metadataFactory.SetOriginalFileName(fileName);

                await this.StoreDataAsync(id, section.Body, cancellationToken).ConfigureAwait(false);
            } else {
                this._logger.LogWarning(LogEvents.UploadIncomplete, "{0}: Unknown file '{1}' with file name '{2}'. Skipping.", id, fileName, cleanName);
            }
        }


        private async Task StoreDataAsync(FileIdentifier id, Stream dataStream, CancellationToken cancellationToken) {
            UploadProgress progress = this.GetProgressObject(id);

            // Copy with progress
            using (Stream outputStream = this._fileWriter.OpenWriteStream(this._fileStore.GetDataFile(id))) {
                using (Stream inputStream = dataStream) {
                    int read;
                    byte[] buffer = new byte[4096];
                    while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                        progress.Current += read;

                        await outputStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task StoreMetadataAsync(FileIdentifier id, StoredMetadata metadata, CancellationToken cancellationToken) {
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

        private sealed class StoredMetadataFactory {
            private readonly StoredMetadata _metadata = new StoredMetadata();

            public void SetExpiration(DateTime expiration) {
                this._metadata.Expiration = expiration;
            }

            public void SetOriginalFileName(string fileName) {
                this._metadata.OriginalFileName = Path.GetFileName(fileName);
            }

            public bool IsComplete() {
                if (String.IsNullOrEmpty(this._metadata.OriginalFileName)) {
                    return false;
                }

                return this._metadata.Expiration != default(DateTime);
            }

            public StoredMetadata Build() {
                this._metadata.UploadedOn = DateTime.UtcNow;

                return this._metadata;
            }
        }
    }
}
