// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadedFileRepository.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

using IFS.Web.Models;
using System.Linq;

namespace IFS.Web.Core.Upload {
    public interface IUploadedFileRepository {
        Task<UploadedFile?> GetFile(FileIdentifier id);
        Task<UploadedFile?> GetFileReservation(FileIdentifier id);

        Task<IList<UploadedFile>> GetFiles();
        void Delete(FileIdentifier id);
    }

    public sealed class UploadedFileRepository : IUploadedFileRepository {
        private readonly IFileStore _fileStore;
        private readonly IFileWriter _fileWriter;

        private readonly IMetadataReader _metadataReader;
        private readonly ILogger<UploadedFileRepository> _logger;

        public UploadedFileRepository(IFileStore fileStore, IFileWriter fileWriter, IMetadataReader metadataReader, ILogger<UploadedFileRepository> logger) {
            this._fileStore = fileStore;
            this._logger = logger;
            this._metadataReader = metadataReader;
            this._fileWriter = fileWriter;
        }

        public async Task<UploadedFile?> GetFile(FileIdentifier id) {
            IFileInfo metadataFile = this._fileStore.GetMetadataFile(id);
            if (!metadataFile.Exists) {
                return null;
            }

            IFileInfo dataFile = this._fileStore.GetDataFile(id);
            if (!dataFile.Exists) {
                this._logger.LogError(LogEvents.UploadIncomplete, "Unable to find data of uploaded file {0}, the metadata was found at path {1}", id, metadataFile.PhysicalPath);
                return null;
            }

            StoredMetadata? metadata = await this._metadataReader.GetMetadataAsync(metadataFile);

            if (metadata == null)
            {
                return null;
            }

            return new UploadedFile(id, dataFile, metadata);
        }

        public async Task<UploadedFile?> GetFileReservation(FileIdentifier id) {
            IFileInfo metadataFile = this._fileStore.GetMetadataFile(id);
            if (!metadataFile.Exists) {
                return null;
            }

            IFileInfo dataFile = this._fileStore.GetDataFile(id);
            if (dataFile.Exists) {
                this._logger.LogError(LogEvents.UploadReservationTaken, "Data of uploaded file {0} has been uploaded at {1}", id, metadataFile.PhysicalPath);
                return null;
            }

            StoredMetadata? metadata = await this._metadataReader.GetMetadataAsync(metadataFile);

            if (metadata == null)
            {
                return null;
            }

            return new UploadedFile(id, dataFile, metadata);
        }

        private async Task<UploadedFile?> GetFileInternal(FileIdentifier id) {
            IFileInfo metadataFile = this._fileStore.GetMetadataFile(id);
            if (!metadataFile.Exists) {
                return null;
            }

            IFileInfo dataFile = this._fileStore.GetDataFile(id);
            StoredMetadata? metadata = await this._metadataReader.GetMetadataAsync(metadataFile);

            if (metadata == null)
            {
                return null;
            }

            return new UploadedFile(id, dataFile, metadata);
        }

        public async Task<IList<UploadedFile>> GetFiles() {
            var fileIds = from fileInfo in this._fileStore.GetFiles()
                          let plainName = Path.GetFileNameWithoutExtension(fileInfo.Name)
                          where plainName != null && FileIdentifier.IsValid(plainName)
                          select FileIdentifier.FromString(plainName);

            List<UploadedFile> uploadedFiles = new List<UploadedFile>();
            foreach (FileIdentifier id in fileIds.Distinct()) {
                UploadedFile? file = await this.GetFileInternal(id).ConfigureAwait(false);

                if (file != null) {
                    uploadedFiles.Add(file);
                }
            }

            return uploadedFiles;
        }

        public void Delete(FileIdentifier id) {
            this._fileWriter.Delete(this._fileStore.GetDataFile(id));
            this._fileWriter.Delete(this._fileStore.GetMetadataFile(id));
        }
    }

    public class UploadedFile {
        private readonly IFileInfo _fileInfo;
        public UploadedFile(FileIdentifier id, IFileInfo fileInfo, StoredMetadata metadata) {
            this._fileInfo = fileInfo;

            this.Metadata = metadata;
            this.Id = id;
        }

        public bool IsPasswordProtected => this.Metadata.DownloadSecurity != null;

        public FileIdentifier Id { get; }

        public StoredMetadata Metadata { get; }

        public bool IsUnusedReservation => !this._fileInfo.Exists && this.Metadata.IsReservation;

        public Stream GetStream() => this._fileInfo.CreateReadStream();

        public long Size => this._fileInfo.Length;
    }
}
