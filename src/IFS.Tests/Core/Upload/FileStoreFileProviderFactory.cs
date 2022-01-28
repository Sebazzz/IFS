// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FileStoreFileProviderFactory.cs
//  Project         : IFS.Tests
// ******************************************************************************

using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

using IFS.Web.Core.Upload;

namespace IFS.Tests.Core.Upload {
    [TestFixture]
    public sealed class FileStoreFileProviderFactoryTests {
        [SetUp]
        public void SetUp() {
            string workerDir = TestContext.CurrentContext.WorkDirectory;
            string testStoreDir = Path.Combine(workerDir, "TestStore");

            if (Directory.Exists(testStoreDir)) Directory.Delete(testStoreDir, true);
            Directory.CreateDirectory(testStoreDir);

            File.WriteAllText(Path.Combine(testStoreDir, "temp.txt"), "TEST");
        }

        [TearDown]
        public void TearDown() {
            string workerDir = TestContext.CurrentContext.WorkDirectory;
            string testStoreDir = Path.Combine(workerDir, "TestStore");

            if (Directory.Exists(testStoreDir)) Directory.Delete(testStoreDir, true);
        }

        [Test]
        public void FileStoreFileProviderFactory_GetFileProvider_ReturnsPhysicalPathCombined() {
            // Given
            IWebHostEnvironment env = Substitute.For<IWebHostEnvironment>();
            env.ContentRootPath.Returns(TestContext.CurrentContext.WorkDirectory);

            IOptions<FileStoreOptions> options = new OptionsWrapper<FileStoreOptions>(new FileStoreOptions {
                StorageDirectory = "TestStore"
            });

            // When
            IFileStoreFileProviderFactory store = new FileStoreFileProviderFactory(env, options);
            IFileProvider provider = store.GetFileProvider();

            // Then
            Assert.That(provider.GetFileInfo("temp.txt").Exists, "provider.GetFileInfo('temp.txt').Exists");
        }
    }
}
