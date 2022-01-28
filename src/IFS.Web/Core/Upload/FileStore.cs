// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileStore.cs
//  Project         : IFS.Web
// ******************************************************************************

using System.Collections.Generic;

using Microsoft.Extensions.FileProviders;

using IFS.Web.Models;

namespace IFS.Web.Core.Upload;

public interface IFileStore {
    IFileInfo GetDataFile(FileIdentifier id);
    IFileInfo GetMetadataFile(FileIdentifier id);
    IEnumerable<IFileInfo> GetFiles();
}

public sealed class FileStore : IFileStore {
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