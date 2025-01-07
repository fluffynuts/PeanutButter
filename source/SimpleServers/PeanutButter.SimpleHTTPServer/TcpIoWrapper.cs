using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer;

/// <summary>
/// Wraps a TcpClient for easier IO
/// </summary>
public class TcpIoWrapper : IDisposable
{
    /// <summary>
    /// Provides access to the raw stream
    /// </summary>
    public Stream RawStream => GetRawStream();

    /// <summary>
    /// Provides access to the stream writer
    /// </summary>
    public StreamWriter StreamWriter => GetStreamWriter();

    private StreamWriter _outputStreamWriter;
    private TcpClient _client;
    private BufferedStream _rawStream;
    private readonly SemaphoreSlim _streamLock = new(1);
    private readonly SemaphoreSlim _instanceLock = new (1);

    /// <summary>
    /// Wraps a TcpClient to provide IO methods
    /// </summary>
    /// <param name="client"></param>
    public TcpIoWrapper(TcpClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        using var _ = new AutoLocker(_instanceLock);

        DisposeStreamWriter();
        DisposeRawStream();
        ShutdownClient();
    }

    private void ShutdownClient()
    {
        _client?.Close();
        _client = null;
    }

    private void DisposeStreamWriter()
    {
        try
        {
            _outputStreamWriter?.Flush();
            _outputStreamWriter?.Dispose();
        }
        catch
        {
            /* intentionally left blank */
        }

        _outputStreamWriter = null;
    }

    private void DisposeRawStream()
    {
        try
        {
            _rawStream?.Flush();
            _rawStream?.Dispose();
        }
        catch
        {
            /* intentionally left blank */
        }

        _rawStream = null;
    }

    private StreamWriter GetStreamWriter()
    {
        using var _ = new AutoLocker(_instanceLock);
        if (_client is null)
        {
            return null;
        }

        return _outputStreamWriter ??= new StreamWriter(
            RawStream
        );
    }

    private Stream GetRawStream()
    {
        using var _ = new AutoLocker(_streamLock);
        if (_client == null)
        {
            return null;
        }

        return _rawStream ??= new BufferedStream(
            _client.GetStream()
        );
    }
}