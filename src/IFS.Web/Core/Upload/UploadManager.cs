// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadManager.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using IFS.Web.Core.Crypto;
using IFS.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace IFS.Web.Core.Upload;

public interface IUploadManager {
    Task StoreAsync(FileIdentifier id, MultipartReader reader, CancellationToken cancellationToken);
}


public interface IUploadProgressManager {
    UploadProgress? GetProgress(FileIdentifier id);

    void SetProgress(FileIdentifier id, UploadProgress uploadProgress);
}

public class UploadProgressManager : IUploadProgressManager {
    private readonly ConcurrentDictionary<FileIdentifier, UploadProgress> _uploadsByFileIdentifier;

    public UploadProgressManager() {
        this._uploadsByFileIdentifier = new ConcurrentDictionary<FileIdentifier, UploadProgress>();
    }

    public UploadProgress? GetProgress(FileIdentifier id) {
        if (!this._uploadsByFileIdentifier.TryGetValue(id, out var value)) {
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
    private readonly IUploadFileLock _uploadFileLock;
    private readonly ILogger<UploadManager> _logger;
    private readonly MetadataReader _metadataReader;

    public UploadManager(IFileStore fileStore, IFileWriter fileWriter, IUploadProgressManager uploadProgressManager, IUploadFileLock uploadFileLock, MetadataReader metadataReader, ILogger<UploadManager> logger) {
        this._fileStore = fileStore;
        this._fileWriter = fileWriter;
        this._logger = logger;
        this._metadataReader = metadataReader;
        this._uploadFileLock = uploadFileLock;
        this._uploadProgressManager = uploadProgressManager;
    }

    public async Task StoreAsync(FileIdentifier id,  MultipartReader reader, CancellationToken cancellationToken) {
        using (this._uploadFileLock.Acquire(id, cancellationToken)) {
            await this.StoreAsyncCore(id, reader, cancellationToken);
        }
    }

    private async Task StoreAsyncCore(FileIdentifier id, MultipartReader reader, CancellationToken cancellationToken) {
        // We need to manually read each part of the request. The browser may do as it likes and send whichever part first, 
        // which means we have to build up the metadata incrementally and can only write it later

        UploadContext uploadContext = new UploadContext(id);
        StoredMetadata? metadata = null;

        try {
            MultipartSection section = await reader.ReadNextSectionAsync(cancellationToken);

            while (section != null) {
                await this.ProcessSectionAsync(uploadContext, section, cancellationToken);

                section = await reader.ReadNextSectionAsync(cancellationToken);
            }

            if (uploadContext.MetadataFactory.IsComplete()) {
                metadata = uploadContext.MetadataFactory.Build();
            }

            if (metadata == null) {
                throw new InvalidOperationException("Metadata is incomplete - file is not uploaded or expiration missing");
            }

            await this.StoreMetadataAsync(id, metadata, cancellationToken);

            this._logger.LogInformation(LogEvents.NewUpload, "Completed: New upload of file {FileName} to id {Identifier} [{Timestamp}]", metadata.OriginalFileName, id, DateTime.UtcNow);
        }
        catch (OperationCanceledException ex) {
            this._logger.LogWarning(LogEvents.UploadCancelled, ex, "Upload failed due to cancellation");

            this.TryCleanup(id);

            // No use rethrowing the exception, we're done anyway.
        }
        catch (InvalidDataException ex) {
            this._logger.LogError(LogEvents.UploadFailed, ex, "Upload failed due to file size exceeding");

            this.TryCleanup(id);

            throw new UploadFailedException("File size exceeded of " + reader.BodyLengthLimit.GetValueOrDefault().Bytes().Megabytes);
        }
        catch (Exception ex) when (!(ex is UploadCryptoArgumentOrderException)) {
            this._logger.LogError(LogEvents.UploadFailed, ex, "Upload failed due to exception");

            this.TryCleanup(id);

            throw new UploadFailedException("Unable to complete upload", ex);
        }
    }

    private async Task ProcessSectionAsync(UploadContext uploadContext, MultipartSection section, CancellationToken cancellationToken) {
        ContentDispositionHeaderValue contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);

        if (contentDisposition.FileName != null) {
            await this.ProcessFileSectionAsync(uploadContext, section, contentDisposition, cancellationToken);
            return;
        }

        await this.ProcessFormSectionAsync(uploadContext, section, contentDisposition);
    }

    private async Task ProcessFormSectionAsync(UploadContext uploadContext, MultipartSection section, ContentDispositionHeaderValue contentDisposition) {
        string cleanName = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

        StoredMetadataFactory metadataFactory = uploadContext.MetadataFactory;
        UploadPassword passwordSetting = uploadContext.PasswordSetting;

        switch (cleanName) {
            case nameof(UploadModel.IsReservation):
                string isReservationRaw = await ReadString();

                metadataFactory.SetIsReservation(Boolean.Parse(isReservationRaw));
                break;

            case nameof(UploadModel.Expiration):
                string dateTimeRaw = await ReadString();

                // MVC we send date as roundtrip
                metadataFactory.SetExpiration(DateTime.ParseExact(dateTimeRaw, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                return;

            case nameof(UploadModel.FileIdentifier):
                string rawId = await ReadString();
                FileIdentifier formId = FileIdentifier.FromString(rawId);

                if (formId != uploadContext.Identifier) {
                    throw new InvalidOperationException($"ID mismatch: '{formId}' (received) != '{uploadContext.Identifier}' (expected)");
                }
                return;

            case nameof(UploadModel.Password):
                string password = await ReadString();
                metadataFactory.SetPassword(password);
                passwordSetting.Password = password;

                EnsureFileNotUploaded();
                return;

            case nameof(UploadModel.EnablePasswordProtection):
                bool passwordSettingWasSet = passwordSetting.IsSet;

                string enablePasswordProtectionRaw = await ReadString();
                bool enablePasswordProtection = String.Equals(Boolean.TrueString, enablePasswordProtectionRaw, StringComparison.OrdinalIgnoreCase);
                metadataFactory.SetEnablePasswordProtection(enablePasswordProtection);
                passwordSetting.SetEnabled(enablePasswordProtection);

                if (!passwordSettingWasSet) EnsureFileNotUploaded();
                return;

            case nameof(UploadModel.Sender) + "." + nameof(ContactInformation.Name):
                string name = await ReadString();
                metadataFactory.SetSenderName(name);
                return;

            case nameof(UploadModel.Sender) + "." + nameof(ContactInformation.EmailAddress):
                string emailAddress = await ReadString();
                metadataFactory.SetSenderEmail(emailAddress);
                return;

            // Browsers don't actually send the file size in the request, but we can derive it from the Content-Length.
            // However, that is not very accurate and if we use some javascript to send a more accurate file size, we use that.
            case nameof(UploadModel.SuggestedFileSize):
                if (Int64.TryParse(await ReadString(), out var size) && size > 0) {
                    this.GetProgressObject(uploadContext.Identifier).Total = size;
                }
                return;

            default:
                this._logger.LogWarning(LogEvents.UploadIncomplete, "{Identifier}: Unknown form field '{Field}'", uploadContext.Identifier, contentDisposition.Name);
                break;
        }

        return;

        async Task<string> ReadString()
        {
            using var sr = new StreamReader(section.Body);
            return await sr.ReadToEndAsync();
        }

        void EnsureFileNotUploaded() {
            bool needToValidate = !String.IsNullOrEmpty(passwordSetting.Password) && passwordSetting.Enable == true;

            if (needToValidate && uploadContext.HasUploadedFile) {
                this._logger.LogError(LogEvents.UploadPasswordAfterFileUpload, "{Identifier}: The upload password is set after the file is uploaded. The file is not encrypted. Terminating upload.", uploadContext.Identifier);

                throw new UploadCryptoArgumentOrderException("File uploaded before password has been set");
            }
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

    private async Task ProcessFileSectionAsync(UploadContext uploadContext, MultipartSection section, ContentDispositionHeaderValue contentDisposition, CancellationToken cancellationToken) {
        string cleanName = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
        string fileName = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value;

        if (fileName is null) throw new ArgumentException("Expected a filename in the payload");
        
        if (cleanName == nameof(UploadModel.File)) {
            this._logger.LogInformation(LogEvents.NewUpload, "New upload of file {FileName} to id {Identifier} [{Timestamp:s}]", fileName, uploadContext.Identifier, DateTime.UtcNow);

            uploadContext.MetadataFactory.SetOriginalFileName(fileName);

            await this.StoreDataAsync(uploadContext.Identifier, section.Body, uploadContext.PasswordSetting, cancellationToken).ConfigureAwait(false);

            uploadContext.HasUploadedFile = true;
        } else {
            this._logger.LogWarning(LogEvents.UploadIncomplete, "{Identifier}: Unknown file '{FileName}' with file name '{CleanName}' - Skipping", uploadContext.Identifier, fileName, cleanName);
        }
    }


    private async Task StoreDataAsync(FileIdentifier id, Stream dataStream, UploadPassword passwordSetting, CancellationToken cancellationToken) {
        UploadProgress progress = this.GetProgressObject(id);

        // Copy with progress
        await using (var outputStream = this._fileWriter.OpenWriteStream(this._fileStore.GetDataFile(id)))
        {
            if (passwordSetting.Enable == true && !string.IsNullOrEmpty(passwordSetting.Password))
            {
                using var crypto = CryptoFactory.CreateCrypto(passwordSetting.Password);
                var encryptor = crypto.CreateEncryptor();

                CryptoMetadata.WriteMetadata(outputStream, crypto);

                await using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write, true);
                await CopyStreamWithProgress(cryptoStream);
            } else {
                await CopyStreamWithProgress(outputStream);
            }
        }

        return;

        async Task CopyStreamWithProgress(Stream outputStream)
        {
            await using var inputStream = dataStream;
            int read;
            var buffer = new byte[4096];
            while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                       .ConfigureAwait(false)) != 0)
            {
                progress.Current += read;

                await outputStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task StoreMetadataAsync(FileIdentifier id, StoredMetadata metadata, CancellationToken cancellationToken) {
        IFileInfo metadataFile = this._fileStore.GetMetadataFile(id);

        if (metadata.IsReservation) {
            StoredMetadata? originalStoredMetadata = await this._metadataReader.GetMetadataAsync(metadataFile);

            if (originalStoredMetadata == null) {
                this._logger.LogWarning(LogEvents.UploadFailed, "{Identifier}: Metadata file expected for reservation at {PhysicalPath}", id, metadataFile.PhysicalPath);

                throw new UploadFailedException("Missing metadata for reserved upload");
            }

            // Sync critical metadata
            metadata.Expiration = originalStoredMetadata.Expiration;
            metadata.Sender = originalStoredMetadata.Sender;

            // Delete metadata so it can be recreated again
            this._fileWriter.Delete(metadataFile);
        } else {
            // Correct the timestamp with the upload time
            TimeSpan diff = DateTime.UtcNow - this.GetProgressObject(id).StartTime;
            metadata.Expiration += diff;
        }

        // Write away
        await using var fileStream = this._fileWriter.OpenWriteStream(metadataFile);
        await using var sw = new StreamWriter(fileStream, Encoding.UTF8);
        await sw.WriteAsync(metadata.Serialize()).ConfigureAwait(false);

        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private void TryCleanup(FileIdentifier id) {
        using (this._logger.BeginScope("Clean-up: {Identifier}", id)) {
            try {
                this._fileWriter.Delete(this._fileStore.GetDataFile(id));
                this._fileWriter.Delete(this._fileStore.GetMetadataFile(id));
            } catch (Exception ex) {
                this._logger.LogError(LogEvents.CleanupFailed, ex, "Unable to clean up file '{Identifier}'", id);
            }
        }
    }

    private sealed class UploadContext {
        public UploadContext(FileIdentifier id) {
            this.Identifier = id;
        }

        public FileIdentifier Identifier { get; }

        public StoredMetadataFactory MetadataFactory { get; } = new();

        public UploadPassword PasswordSetting { get; } = new();

        public bool HasUploadedFile { get; set; }
    }

    private sealed class UploadPassword {
        public string? Password { get;set; }
        public bool? Enable { get; private set; }
        public bool IsSet => this.Enable != null;

        public void SetEnabled(bool enable) {
            if (this.Enable == null) {
                this.Enable = enable;
            }
        }
    }

    private sealed class StoredMetadataFactory {
        private readonly StoredMetadata _metadata = new();
        private bool? _enablePasswordProtection;

        public void SetExpiration(DateTime expiration) {
            this._metadata.Expiration = expiration;
        }

        public void SetOriginalFileName(string fileName) {
            this._metadata.OriginalFileName = Path.GetFileName(fileName);
        }

        public void SetSenderName(string value) {
            if (String.IsNullOrEmpty(value)) {
                return;
            }

            ContactInformation contactInformation = this.GetContactInformationObject();
            contactInformation.Name = value;
        }

        public void SetSenderEmail(string value) {
            if (String.IsNullOrEmpty(value)) {
                return;
            }

            ContactInformation contactInformation = this.GetContactInformationObject();
            contactInformation.Name = value;
        }

        private ContactInformation GetContactInformationObject() {
            return this._metadata.Sender = this._metadata.Sender ?? new ContactInformation();
        }

        public bool IsComplete() {
            if (String.IsNullOrEmpty(this._metadata.OriginalFileName)) {
                return false;
            }

            return this._metadata.Expiration != default(DateTime);
        }

        public StoredMetadata Build() {
            this._metadata.UploadedOn = DateTime.UtcNow;

            if (this._enablePasswordProtection != null && !this._enablePasswordProtection.Value) {
                this._metadata.DownloadSecurity = null;
            }

            return this._metadata;
        }

        public void SetIsReservation(bool value) {
            this._metadata.IsReservation = value;
        }

        public void SetPassword(string password) {
            this._metadata.DownloadSecurity = String.IsNullOrWhiteSpace(password) ? null : DownloadSecurity.CreateNew(password.Trim());
        }

        public void SetEnablePasswordProtection(bool enable) {
            if (this._enablePasswordProtection != null) {
                // Ignore - this is an helper value set by MVC
                return;
            }

            this._enablePasswordProtection = enable;
        }
    }
}