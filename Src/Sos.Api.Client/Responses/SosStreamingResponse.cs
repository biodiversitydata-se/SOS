using System.Net;

namespace SOS.Api.Client.Responses;

/// <summary>
/// Streaming response that owns the underlying HTTP response and stream.
/// </summary>
public sealed class SosStreamingResponse : IDisposable, IAsyncDisposable
{
    private readonly HttpResponseMessage _response;
    private bool _disposed;

    internal SosStreamingResponse(HttpResponseMessage response, Stream content)
    {
        _response = response;
        Content = new ResponseOwningStream(content, response);
        StatusCode = response.StatusCode;
        ContentType = response.Content.Headers.ContentType?.MediaType;
        FileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
        ContentLength = response.Content.Headers.ContentLength;
    }

    /// <summary>The response content stream.</summary>
    public Stream Content { get; }
    /// <summary>The HTTP status code.</summary>
    public HttpStatusCode StatusCode { get; }
    /// <summary>The response media type.</summary>
    public string? ContentType { get; }
    /// <summary>The suggested download file name.</summary>
    public string? FileName { get; }
    /// <summary>The response content length, when supplied by the server.</summary>
    public long? ContentLength { get; }

    /// <summary>Disposes the response stream and its underlying HTTP response.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Content.Dispose();
    }

    /// <summary>Asynchronously disposes the response stream and its underlying HTTP response.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await Content.DisposeAsync();
    }

    private sealed class ResponseOwningStream : Stream
    {
        private readonly Stream _inner;
        private readonly HttpResponseMessage _owner;
        private bool _disposed;

        public ResponseOwningStream(Stream inner, HttpResponseMessage owner)
        {
            _inner = inner;
            _owner = owner;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => _inner.Read(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.WriteAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _owner.Dispose();
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;
            await _inner.DisposeAsync();
            _owner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
