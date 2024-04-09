// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : CryptoStreamWrapper.cs
//  Project         : IFS.Web
// ******************************************************************************

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using IFS.Web.Core.Crypto;
using IFS.Web.Core.Upload;

namespace IFS.Web.Core.Download;

/// <summary>
/// Wraps the crypto stream to keep both algorithm and cryptostream alive
/// </summary>
internal sealed class CryptoStreamWrapper : Stream {
    private readonly Aes _crypto;
    private readonly CryptoStream _cryptoStream;
    private readonly long _originalSize;

    private CryptoStreamWrapper(Aes crypto, CryptoStream cryptoStream, long originalSize)
    {
        this._crypto = crypto;
        this._cryptoStream = cryptoStream;
        this._originalSize = originalSize;
    }

    public static Stream Create(UploadedFile file, string password) {
        Stream fileStream = file.GetStream();
        Aes? crypto = null;

        try {
            crypto = CryptoMetadata.ReadMetadataAndInitializeAlgorithm(fileStream, password);

            CryptoStream cryptoStream = new CryptoStream(
                fileStream,
                crypto.CreateDecryptor(),
                CryptoStreamMode.Read,
                false
            );

            return new CryptoStreamWrapper(crypto, cryptoStream, file.OriginalSize);
        }
        catch (Exception) {
            fileStream.Dispose();
            crypto?.Dispose();
            throw;
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        this._crypto.Dispose();
        this._cryptoStream.Dispose();
    }

    #region Delegated members

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) {
        return this._cryptoStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) {
        return this._cryptoStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override bool CanRead => this._cryptoStream.CanRead;

    public override bool CanSeek =>
        this._originalSize !=
        default; // Note: In practice the result of this is only used by ASP.NET to determine if Length won't throw

    public override bool CanTimeout => this._cryptoStream.CanTimeout;

    public override bool CanWrite => this._cryptoStream.CanWrite;

    public override void Close() {
        this._cryptoStream.Close();
    }

    public override void CopyTo(Stream destination, int bufferSize) {
        this._cryptoStream.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) {
        return this._cryptoStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override int EndRead(IAsyncResult asyncResult) {
        return this._cryptoStream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult) {
        this._cryptoStream.EndWrite(asyncResult);
    }

    public override void Flush() {
        this._cryptoStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken) {
        return this._cryptoStream.FlushAsync(cancellationToken);
    }

    public override long Length => this._originalSize != default ? this._originalSize : this._cryptoStream.Length;

    public override long Position
    {
        get => this._cryptoStream.Position;
        set => this._cryptoStream.Position = value;
    }

    public override int Read(Span<byte> buffer) {
        return this._cryptoStream.Read(buffer);
    }

    public override int Read(byte[] buffer, int offset, int count) {
        return this._cryptoStream.Read(buffer, offset, count);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) {
        return this._cryptoStream.ReadAsync(buffer, cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        return this._cryptoStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override int ReadByte() {
        return this._cryptoStream.ReadByte();
    }

    public override int ReadTimeout
    {
        get => this._cryptoStream.ReadTimeout;
        set => this._cryptoStream.ReadTimeout = value;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        return this._cryptoStream.Seek(offset, origin);
    }

    public override void SetLength(long value) {
        this._cryptoStream.SetLength(value);
    }

    public override void Write(ReadOnlySpan<byte> buffer) {
        this._cryptoStream.Write(buffer);
    }

    public override void Write(byte[] buffer, int offset, int count) {
        this._cryptoStream.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        return this._cryptoStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken()) {
        return this._cryptoStream.WriteAsync(buffer, cancellationToken);
    }

    public override void WriteByte(byte value) {
        this._cryptoStream.WriteByte(value);
    }

    public override int WriteTimeout
    {
        get => this._cryptoStream.WriteTimeout;
        set => this._cryptoStream.WriteTimeout = value;
    }

    #endregion
}