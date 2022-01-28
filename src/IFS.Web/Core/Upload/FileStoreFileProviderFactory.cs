// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileStoreFileProviderFactory.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Diagnostics;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace IFS.Web.Core.Upload {
    public interface IFileStoreFileProviderFactory {
        IFileProvider GetFileProvider();
    }

    public class FileStoreFileProviderFactory : IFileStoreFileProviderFactory {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly FileStoreOptions _fileStoreOptions;

        public FileStoreFileProviderFactory(IWebHostEnvironment hostingEnvironment, IOptions<FileStoreOptions> fileStoreOptions) {
            this._hostingEnvironment = hostingEnvironment;
            this._fileStoreOptions = fileStoreOptions.Value;
        }

        public IFileProvider GetFileProvider() {
            string path = Path.Combine(this._hostingEnvironment.ContentRootPath, this._fileStoreOptions.StorageDirectory);
            Debug.Assert(Path.IsPathRooted(path));

            return new PhysicalFileProvider(path);
        }
    }
}
