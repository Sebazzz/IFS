namespace IFS.Web.Core.Upload {
    using System;
    using System.IO;

    using Microsoft.Extensions.FileProviders;

    public interface IFileWriter {
        Stream OpenWriteStream(IFileInfo fileInfo);
        void Delete(IFileInfo fileInfo);
    }


    public sealed class FileWriter : IFileWriter {
        public Stream OpenWriteStream(IFileInfo fileInfo) {
            string physicalPath = GetPhysicalPath(fileInfo);

            return new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
        }

        private static string GetPhysicalPath(IFileInfo fileInfo) {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            string physicalPath = fileInfo.PhysicalPath;
            if (physicalPath == null) {
                throw new FileNotFoundException($"File '{fileInfo.Name}' doesn't have a physical path");
            }
            return physicalPath;
        }

        public void Delete(IFileInfo fileInfo) {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

            if (!fileInfo.Exists) {
                return;
            }

            string physicalPath = GetPhysicalPath(fileInfo);

            File.Delete(physicalPath);
        }
    }
}