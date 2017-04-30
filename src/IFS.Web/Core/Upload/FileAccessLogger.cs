// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileAccessLogger.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public interface IFileAccessLogger {
        Task LogFileAccessAsync(UploadedFile uploadedFile, string ipAddress);
    }

    public class FileAccessLogger : IFileAccessLogger {
        private readonly IFileStore _fileStore;
        private readonly IFileWriter _fileWriter;

        public FileAccessLogger(IFileStore fileStore, IFileWriter fileWriter) {
            this._fileStore = fileStore;
            this._fileWriter = fileWriter;
        }

        public async Task LogFileAccessAsync(UploadedFile uploadedFile, string ipAddress) {
            var metadata = uploadedFile.Metadata;

            metadata.Access.LogEntries.Add(new FileAccessLogEntry {
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            });

            using (Stream fileStream = this._fileWriter.OpenWriteStream(this._fileStore.GetMetadataFile(uploadedFile.Id))) {
                using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8)) {
                    await sw.WriteAsync(metadata.Serialize()).ConfigureAwait(false);

                    await fileStream.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
