// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FakeLogger.cs
//  Project         : IFS.Tests
// ******************************************************************************

using System;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IFS.Tests.Unit.Support;

public static class FakeLogger
{
    public static ILogger<T> Get<T>()
    {
        var fakeLogger = Substitute.For<ILogger<T>>();

        fakeLogger.BeginScope(Arg.Any<string>()).Returns(Substitute.For<IDisposable>());

        return fakeLogger;
    }
}