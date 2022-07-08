using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake websocket manager
/// </summary>
public class FakeWebSocketManager : WebSocketManager, IFake
{
    /// <inheritdoc />
    public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
    {
        var handler = _webSocketAcceptHandler;
        return handler is null
            ? DefaultWebSocketHandler(subProtocol)
            : handler(subProtocol);
    }

    /// <inheritdoc />
    public override bool IsWebSocketRequest => _isWebSocketRequest;
    private bool _isWebSocketRequest;

    /// <inheritdoc />
    public override IList<string> WebSocketRequestedProtocols => _protocols;
    private List<string> _protocols = new();

    private Func<string, Task<WebSocket>> _webSocketAcceptHandler = DefaultWebSocketHandler;

    private static Task<WebSocket> DefaultWebSocketHandler(string subProtocol)
    {
        return Task.FromResult<WebSocket>(new FakeWebSocket());
    }

    /// <summary>
    /// Set the accept handler
    /// </summary>
    /// <param name="handler"></param>
    public void SetWebSocketAcceptHandler(
        Func<string, Task<WebSocket>> handler
    )
    {
        _webSocketAcceptHandler = handler;
    }

    /// <summary>
    /// Set the IsWebSocketRequest property
    /// </summary>
    /// <param name="b"></param>
    public void SetIsWebSocketRequest(bool b)
    {
        _isWebSocketRequest = b;
    }

    /// <summary>
    /// Set the requested protocols
    /// </summary>
    /// <param name="protocols"></param>
    public void SetProtocols(IEnumerable<string> protocols)
    {
        _protocols = new List<string>(protocols);
    }
}