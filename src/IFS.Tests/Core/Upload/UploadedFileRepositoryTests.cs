// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadedFileRepository.cs
//  Project         : IFS.Tests
// ******************************************************************************

namespace IFS.Tests.Core.Upload {
    using System.Threading.Tasks;

    using Microsoft.Extensions.FileProviders;

    using NSubstitute;

    using NUnit.Framework;

    using Support;

    using Web.Core.Upload;
    using Web.Models;

    [TestFixture]
    public class UploadedFileRepositoryTests {
        [Test]
        public async Task UploadedFileRepository_MetadataFileMissing_GetFile_ReturnsNull() {
            // Given
            IFileStore fileStore = Substitute.For<IFileStore>();
            fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new NotFoundFileInfo("_"));

            // When
            IUploadedFileRepository testObject = new UploadedFileRepository(fileStore, null, null, FakeLogger.Get<UploadedFileRepository>());
            UploadedFile returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

            // Then
            Assert.That(returnedValue, Is.Null);
        }

        [Test]
        public async Task UploadedFileRepository_DataFileMissing_GetFile_ReturnsNull() {
            // Given
            IFileStore fileStore = Substitute.For<IFileStore>();
            fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile());
            fileStore.GetDataFile(Arg.Any<FileIdentifier>()).Returns(new NotFoundFileInfo("_"));

            // When
            IUploadedFileRepository testObject = new UploadedFileRepository(fileStore, null, null, FakeLogger.Get<UploadedFileRepository>());
            UploadedFile returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

            // Then
            Assert.That(returnedValue, Is.Null);
        }

        [Test]
        public async Task UploadedFileRepository_Success_ReturnsFileWithMetadata() {
            // Given
            IFileStore fileStore = Substitute.For<IFileStore>();

            var metadataString = (new StoredMetadata {OriginalFileName = "TestFile.txt"}).Serialize();
            fileStore.GetMetadataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile(contents: metadataString));

            fileStore.GetDataFile(Arg.Any<FileIdentifier>()).Returns(new FakeFile());

            // When
            IUploadedFileRepository testObject = new UploadedFileRepository(fileStore, null, null, FakeLogger.Get<UploadedFileRepository>());
            UploadedFile returnedValue = await testObject.GetFile(FileIdentifier.CreateNew());

            // Then
            Assert.That(returnedValue.Metadata.OriginalFileName, Is.EqualTo("TestFile.txt"));
        }
    }
}
