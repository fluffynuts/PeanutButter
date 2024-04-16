using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes.Internal;

namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes.Internal;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <inheritdoc />
internal class FakeResponsePipeWriter
    : PipeWriter
{
    private readonly StreamPipeWriter _streamPipeWriter;

    /// <inheritdoc />
    public FakeResponsePipeWriter(
        FakeHttpResponse response
    )
    {
        _streamPipeWriter = new StreamPipeWriter(
            response.Body,
            new StreamPipeWriterOptions()
        );
    }

    /// <inheritdoc />
    public override void Advance(int bytes)
    {
        _streamPipeWriter.Advance(bytes);
    }

    /// <inheritdoc />
    public override Memory<byte> GetMemory(int sizeHint = 0)
    {
        return _streamPipeWriter.GetMemory(sizeHint);
    }

    /// <inheritdoc />
    public override Span<byte> GetSpan(int sizeHint = 0)
    {
        return _streamPipeWriter.GetSpan(sizeHint);
    }

    /// <inheritdoc />
    public override void CancelPendingFlush()
    {
        _streamPipeWriter.CancelPendingFlush();
    }

    /// <inheritdoc />
    public override void Complete(Exception exception = null)
    {
        _streamPipeWriter.Complete(exception);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override ValueTask<FlushResult> FlushAsync(
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        return _streamPipeWriter.FlushAsync(cancellationToken);
    }
}