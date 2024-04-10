// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FakeFileStoreFileProviderFactory.cs
//  Project         : IFS.Tests
// ******************************************************************************

using IFS.Web.Core.Upload;
using Microsoft.Extensions.FileProviders;

namespace IFS.Tests.Unit.Support;

public sealed class FakeFileStoreFileProviderFactory : IFileStoreFileProviderFactory
{
    private readonly IFileProvider _fileProvider;

    public FakeFileStoreFileProviderFactory(IFileProvider fileProvider)
    {
        this._fileProvider = fileProvider;
    }

    public IFileProvider GetFileProvider()
    {
        return this._fileProvider;
    }
}