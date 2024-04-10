﻿// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : ExpiredFileRemovalJobTests.cs
//  Project         : IFS.Tests
// ******************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using IFS.Tests.Unit.Support;
using IFS.Web.Core.Upload;
using IFS.Web.Models;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace IFS.Tests.Unit.Core.Upload;

[TestFixture]
public sealed class ExpiredFileRemovalJobTests
{
    [Test]
    [Ignore("Possible bug in NSubstitute")]
    public async Task ExpiredFileRemovalJob_RemovesObsoleteFiles()
    {
        // Given
        var files = new List<UploadedFile>();

        var job = GetTestObject(files);

        files.Add(new UploadedFile(FileIdentifier.FromString("a"), new FakeFile(),
            new StoredMetadata { Expiration = DateTime.UtcNow.AddDays(1) }));
        files.Add(new UploadedFile(FileIdentifier.FromString("b"), new FakeFile(),
            new StoredMetadata { Expiration = DateTime.UtcNow.AddDays(-1) }));
        files.Add(new UploadedFile(FileIdentifier.FromString("c"), new FakeFile(),
            new StoredMetadata { Expiration = DateTime.UtcNow.AddMilliseconds(-1) }));

        // When
        await job.Execute(new JobCancellationToken(false));

        // Then
        Assert.That(files, ContainsFileIdentifier(FileIdentifier.FromString("a")));
        Assert.That(files, Has.Count.EqualTo(1));
    }

    private static ExpiredFileRemovalJob GetTestObject(List<UploadedFile> files)
    {
        var repository = Substitute.For<IUploadedFileRepository>();

        repository.GetFiles().Returns(Task.FromResult<IList<UploadedFile>>(files.ToList()));
        repository.When(r => r.Delete(Arg.Any<FileIdentifier>()))
            .Do(c => files.RemoveAll(f => c.Arg<FileIdentifier>().Equals(f.Id)));

        return new ExpiredFileRemovalJob(repository, FakeLogger.Get<ExpiredFileRemovalJob>());
    }


    private static IConstraint ContainsFileIdentifier(FileIdentifier id)
    {
        return new SomeItemsConstraint(Has.Property("Id").EqualTo(id));
    }
}