// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadedFileRepository.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;

    using Models;

    public interface IUploadedFileRepository {
        Task<UploadedFile> GetFile(FileIdentifier id);
    }

    public sealed class UploadedFileRepository : IUploadedFileRepository {
        private readonly FileStore _fileStore;
        private readonly ILogger<UploadedFileRepository> _logger;

        public UploadedFileRepository(FileStore fileStore, ILogger<UploadedFileRepository> logger) {
            this._fileStore = fileStore;
            this._logger = logger;
        }

        public async Task<UploadedFile> GetFile(FileIdentifier id) {
            IFileInfo metadataFile = this._fileStore.GetMetadataFile(id);
            if (!metadataFile.Exists) {
                return null;
            }

            IFileInfo dataFile = this._fileStore.GetDataFile(id);
            if (!dataFile.Exists) {
                this._logger.LogError(LogEvents.UploadIncomplete, "Unable to find data of uploaded file {0}, the metadata was found at path {1}", id, metadataFile.PhysicalPath);
                return null;
            }

            StoredMetadata metadata;
            try {
                string json;
                using (StreamReader sw = new StreamReader(metadataFile.CreateReadStream())) {
                    json = await sw.ReadToEndAsync();
                }

                metadata = StoredMetadata.Deserialize(json);
            }
            catch (Exception ex) {
                this._logger.LogError(LogEvents.UploadCorrupted, ex, "Metadata of upload {0} at location {1} is corrupted", id, metadataFile.PhysicalPath);
                return null;
            }

            return new UploadedFile(id, dataFile, metadata);
        }
    }

    public class UploadedFile {
        private readonly IFileInfo _fileInfo;
        public UploadedFile(FileIdentifier id, IFileInfo fileInfo, StoredMetadata metadata) {
            this._fileInfo = fileInfo;
            this.Metadata = metadata;
        }

        public FileIdentifier Id { get; }

        public StoredMetadata Metadata { get; }

        public Stream GetStream() => this._fileInfo.CreateReadStream();
    }
}
