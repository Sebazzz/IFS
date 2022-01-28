// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : MetadataReader.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace IFS.Web.Core.Upload {
    public interface IMetadataReader {
        Task<StoredMetadata?> GetMetadataAsync(IFileInfo file);
    }

    public class MetadataReader : IMetadataReader {
        private readonly ILogger<MetadataReader> _logger;

        public MetadataReader(ILogger<MetadataReader> logger) {
            this._logger = logger;
        }


        public async Task<StoredMetadata?> GetMetadataAsync(IFileInfo file) {
            StoredMetadata metadata;
            try {
                string json;
                using (StreamReader sw = new StreamReader(file.CreateReadStream())) {
                    json = await sw.ReadToEndAsync().ConfigureAwait(false);
                }

                metadata = StoredMetadata.Deserialize(json);
            } catch (Exception ex) {
                this._logger.LogError(LogEvents.UploadCorrupted, ex, "Metadata of upload at location {0} is corrupted", file.PhysicalPath);
                return null;
            }

            return metadata;
        }
    }
}
