// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FaleFileStore.cs
//  Project         : IFS.Tests
// ******************************************************************************

namespace IFS.Tests.Support {
    using Microsoft.Extensions.FileProviders;

    using Web.Core.Upload;

    public sealed class FakeFileStoreFileProviderFactory : IFileStoreFileProviderFactory {
        private readonly IFileProvider _fileProvider;

        public FakeFileStoreFileProviderFactory(IFileProvider fileProvider) {
            this._fileProvider = fileProvider;
        }

        public IFileProvider GetFileProvider() => this._fileProvider;
    }
}
