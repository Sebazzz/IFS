// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : FakeLogger.cs
//  Project         : IFS.Tests
// ******************************************************************************

namespace IFS.Tests.Support {
    using Microsoft.Extensions.Logging;

    using NSubstitute;

    public static class FakeLogger {
        public static ILogger<T> Get<T>() {
            ILogger<T> fakeLogger = Substitute.For<ILogger<T>>();

            return fakeLogger;
        }
    }
}
