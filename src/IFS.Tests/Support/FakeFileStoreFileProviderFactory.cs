// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FaleFileStore.cs
//  Project         : IFS.Tests
// ******************************************************************************

using Microsoft.Extensions.FileProviders;

using IFS.Web.Core.Upload;

namespace IFS.Tests.Support {
    public sealed class FakeFileStoreFileProviderFactory : IFileStoreFileProviderFactory {
        private readonly IFileProvider _fileProvider;

        public FakeFileStoreFileProviderFactory(IFileProvider fileProvider) {
            this._fileProvider = fileProvider;
        }

        public IFileProvider GetFileProvider() => this._fileProvider;
    }
}
