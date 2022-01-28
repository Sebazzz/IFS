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

namespace IFS.Tests.Support;

public sealed class FakeFile : IFileInfo {
    private readonly string _contents;

    public FakeFile(string contents) {
        this._contents = contents;
    }

    public FakeFile() {}

    public Stream CreateReadStream() {
        if (this._contents == null) {
            throw new System.NotImplementedException();
        }

        return new MemoryStream(Encoding.UTF8.GetBytes(this._contents));
    }

    public bool Exists => true;

    public long Length {
        get { throw new System.NotImplementedException(); }
    }

    public string PhysicalPath => "Physical";

    public string Name {
        get { throw new System.NotImplementedException(); }
    }

    public DateTimeOffset LastModified {
        get { throw new System.NotImplementedException(); }
    }

    public bool IsDirectory {
        get { throw new System.NotImplementedException(); }
    }
}