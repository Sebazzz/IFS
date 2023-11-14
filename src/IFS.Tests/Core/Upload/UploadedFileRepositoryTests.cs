// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadedFileRepositoryTests.cs
//  Project         : IFS.Tests
// ******************************************************************************

using System.Threading.Tasks;
using IFS.Tests.Support;
using IFS.Web.Core.Upload;
using IFS.Web.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace IFS.Tests.Core.Upload;

[TestFixture]
public class UploadedFileRepositoryTests
{
    [Test]
    public async Task UploadedFileRepository_MetadataFileMissing_GetFile_ReturnsNull()
    {
        // Given
        var fileStore = Substitute.For<IFileStore>();
        fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new NotFoundFileInfo("_"));

        // When
        IUploadedFileRepository testObject =
            new UploadedFileRepository(fileStore, null, null, FakeLogger.Get<UploadedFileRepository>());
        var returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

        // Then
        Assert.That(returnedValue, Is.Null);
    }

    [Test]
    public async Task UploadedFileRepository_DataFileMissing_GetFile_ReturnsNull()
    {
        // Given
        var fileStore = Substitute.For<IFileStore>();
        fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile());
        fileStore.GetDataFile(Arg.Any<FileIdentifier>()).Returns(new NotFoundFileInfo("_"));

        // When
        IUploadedFileRepository testObject =
            new UploadedFileRepository(fileStore, null, null, FakeLogger.Get<UploadedFileRepository>());
        var returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

        // Then
        Assert.That(returnedValue, Is.Null);
    }

    [Test]
    public async Task UploadedFileRepository_Success_ReturnsFileWithMetadata()
    {
        // Given
        var fileStore = Substitute.For<IFileStore>();
        IMetadataReader metadataReader = new MetadataReader(Substitute.For<ILogger<MetadataReader>>());

        var metadataString = new StoredMetadata { OriginalFileName = "TestFile.txt" }.Serialize();
        fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile(metadataString));

        fileStore.GetDataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile());

        // When
        IUploadedFileRepository testObject =
            new UploadedFileRepository(fileStore, null, metadataReader, FakeLogger.Get<UploadedFileRepository>());
        var returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

        // Then
        Assert.That(returnedValue.Metadata.OriginalFileName, Is.EqualTo("TestFile.txt"));
    }
}