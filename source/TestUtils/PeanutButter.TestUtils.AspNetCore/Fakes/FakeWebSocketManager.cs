using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public class FakeWebSocketManager : WebSocketManager
{
    public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
    {
        var handler = _webSocketAcceptHandler;
        return handler is null
            ? DefaultWebSocketHandler(subProtocol)
            : handler(subProtocol);
    }

    public override bool IsWebSocketRequest => _isWebSocketRequest;
    private bool _isWebSocketRequest;

    public override IList<string> WebSocketRequestedProtocols => _protocols;
    private List<string> _protocols = new();

    private Func<string, Task<WebSocket>> _webSocketAcceptHandler = DefaultWebSocketHandler;

    private static Task<WebSocket> DefaultWebSocketHandler(string subProtocol)
    {
        return Task.FromResult<WebSocket>(new FakeWebSocket());
    }

    public void SetWebSocketAcceptHandler(
        Func<string, Task<WebSocket>> handler
    )
    {
        _webSocketAcceptHandler = handler;
    }

    public void SetIsWebSocketRequest(bool b)
    {
        _isWebSocketRequest = b;
    }

    public void SetProtocols(IEnumerable<string> protocols)
    {
        _protocols = new List<string>(protocols);
    }
}