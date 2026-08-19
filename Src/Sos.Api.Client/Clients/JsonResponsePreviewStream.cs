using System.Buffers;
using System.Text;

namespace SOS.Api.Client.Clients;

/// <summary>
/// Read-only stream wrapper that keeps a bounded preview while JSON is deserialized.
/// </summary>
internal sealed class JsonResponsePreviewStream : Stream
{
    private const string TruncatedMarker = "\n...[truncated]";

    private readonly Stream _innerStream;
    private byte[]? _previewBuffer;
    private int _previewLength;
    private long _totalBytesRead;

    public JsonResponsePreviewStream(Stream innerStream, int previewSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(innerStream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(previewSizeBytes);

        _innerStream = innerStream;
        _previewBuffer = ArrayPool<byte>.Shared.Rent(previewSizeBytes);
        PreviewSizeBytes = previewSizeBytes;
    }

    public bool HasNonWhitespaceContent { get; private set; }

    private int PreviewSizeBytes { get; }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _innerStream.Read(buffer, offset, count);
        Capture(buffer.AsSpan(offset, bytesRead));
        return bytesRead;
    }

    public override int Read(Span<byte> buffer)
    {
        var bytesRead = _innerStream.Read(buffer);
        Capture(buffer[..bytesRead]);
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var bytesRead = await _innerStream.ReadAsync(buffer, cancellationToken);
        Capture(buffer.Span[..bytesRead]);
        return bytesRead;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        var bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        Capture(buffer.AsSpan(offset, bytesRead));
        return bytesRead;
    }

    public async Task DrainAsync(CancellationToken cancellationToken)
    {
        await CopyToAsync(Stream.Null, cancellationToken);
    }

    public string GetPreview()
    {
        ObjectDisposedException.ThrowIf(_previewBuffer is null, this);

        var preview = Encoding.UTF8.GetString(_previewBuffer, 0, _previewLength);
        return _totalBytesRead > PreviewSizeBytes
            ? preview + TruncatedMarker
            : preview;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ReturnPreviewBuffer();
            _innerStream.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        ReturnPreviewBuffer();
        await _innerStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private void Capture(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            return;
        }

        _totalBytesRead += bytes.Length;
        if (!HasNonWhitespaceContent)
        {
            foreach (var value in bytes)
            {
                if (value is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n'))
                {
                    HasNonWhitespaceContent = true;
                    break;
                }
            }
        }

        var bytesToCapture = Math.Min(bytes.Length, PreviewSizeBytes - _previewLength);
        if (bytesToCapture <= 0)
        {
            return;
        }

        bytes[..bytesToCapture].CopyTo(_previewBuffer!.AsSpan(_previewLength));
        _previewLength += bytesToCapture;
    }

    private void ReturnPreviewBuffer()
    {
        var buffer = Interlocked.Exchange(ref _previewBuffer, null);
        if (buffer is not null)
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }
}
