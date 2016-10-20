// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileStore.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace IFS.Web.Core.Upload {
    using System.Collections.Generic;

    using Microsoft.Extensions.FileProviders;

    using Models;

    public sealed class FileStore {
        private readonly IFileProvider _fileProvider;

        public FileStore(IFileStoreFileProviderFactory fileProviderFactory) {
            this._fileProvider = fileProviderFactory.GetFileProvider();
        }

        public IFileInfo GetDataFile(FileIdentifier id) => this.GetFile($"{id}.dat");

        private IFileInfo GetFile(string name) {
            IFileInfo fileInfo = this._fileProvider.GetFileInfo(name);

            return fileInfo;
        }

        public IFileInfo GetMetadataFile(FileIdentifier id) => this.GetFile($"{id}.metadata");

        public IEnumerable<IFileInfo> GetFiles() {
            return this._fileProvider.GetDirectoryContents("");
        }
    }
}
