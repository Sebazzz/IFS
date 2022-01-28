// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadFileLock.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.Threading;

using IFS.Web.Models;

namespace IFS.Web.Core.Upload {
    public interface IUploadFileLock {
        /// <summary>
        /// Acquires a system-wide lock on the file identifiers
        /// </summary>
        IDisposable Acquire(FileIdentifier fileIdentifier, CancellationToken cancellationToken);
    }

    public sealed class UploadFileLock : IUploadFileLock {
        private static int Locker = 0;

        /// <summary>
        /// Acquires a system-wide lock on the file identifiers
        /// </summary>
        public IDisposable Acquire(FileIdentifier fileIdentifier, CancellationToken cancellationToken) {
            while (Interlocked.CompareExchange(ref Locker, 0, 1) == 1) {
                Thread.Sleep(10);

                cancellationToken.ThrowIfCancellationRequested();
            }

            return new LockInstance();
        }

        private sealed class LockInstance : IDisposable {
            private bool _isDisposed;

            ~LockInstance() {
                this.Dispose(false);
            }


            private void Dispose(bool disposing) {
                if (!this._isDisposed) {
                    Interlocked.CompareExchange(ref Locker, 1, 0);

                    this._isDisposed = true;
                }
            }

            public void Dispose() {
                this.Dispose(true);

                GC.SuppressFinalize(this);
            }
        }
    }
}
