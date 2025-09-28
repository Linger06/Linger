using MimeKit;

namespace Linger.Email;

/// <summary>
///     附件信息
/// </summary>
public class AttachmentInfo : IDisposable
{
    private Stream? _stream;

    /// <summary>
    ///     附件类型，比如application/pdf
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     文件名称
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    ///     文件传输编码方式，默认ContentEncoding.Default
    /// </summary>
    public ContentEncoding ContentTransferEncoding { get; set; } = ContentEncoding.Default;

    /// <summary>
    ///     文件数组
    /// </summary>
    public byte[]? Data { get; set; }

    /// <summary>
    ///     文件数据流，获取数据时优先采用此部分
    /// </summary>
    public Stream? Stream
    {
        get
        {
            if (_stream == null && Data != null)
            {
                _stream = new MemoryStream(Data);
            }

            return _stream;
        }
        set => _stream = value;
    }

    /// <summary>
    ///     释放Stream
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _stream?.Dispose();
        }
        _disposed = true;
    }
}
