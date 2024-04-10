// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FakeFile.cs
//  Project         : IFS.Tests
// ******************************************************************************

using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace IFS.Tests.Unit.Support;

public sealed class FakeFile : IFileInfo
{
    private readonly string _contents;

    public FakeFile(string contents)
    {
        this._contents = contents;
    }

    public FakeFile()
    {
    }

    public Stream CreateReadStream()
    {
        if (this._contents == null) throw new NotImplementedException();

        return new MemoryStream(Encoding.UTF8.GetBytes(this._contents));
    }

    public bool Exists => true;

    public long Length => throw new NotImplementedException();

    public string PhysicalPath => "Physical";

    public string Name => throw new NotImplementedException();

    public DateTimeOffset LastModified => throw new NotImplementedException();

    public bool IsDirectory => throw new NotImplementedException();
}