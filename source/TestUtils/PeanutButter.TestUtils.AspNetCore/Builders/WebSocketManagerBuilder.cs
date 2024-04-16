using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Builds a fake websocket manager
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class WebSocketManagerBuilder : RandomizableBuilder<WebSocketManagerBuilder, WebSocketManager>
{
    /// <summary>
    /// Does nothing - just here to ensure GetRandom works
    /// </summary>
    /// <returns></returns>
    public override WebSocketManagerBuilder Randomize()
    {
        return this; // nothing really to randomize on this
    }

    /// <summary>
    /// Constructs a fake websocket manager
    /// </summary>
    /// <returns></returns>
    protected override WebSocketManager ConstructEntity()
    {
        return new FakeWebSocketManager();
    }

    /// <summary>
    /// Sets the websocket accept handler
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public WebSocketManagerBuilder WithWebSocketAcceptHandler(
        Func<string, Task<WebSocket>> handler
    )
    {
        return With<FakeWebSocketManager>(
            o => o.SetWebSocketAcceptHandler(handler)
        );
    }

    /// <summary>
    /// Sets the IsWebSocketRequest property
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public WebSocketManagerBuilder WithIsWebSocketRequest(bool flag)
    {
        return With<FakeWebSocketManager>(
            o => o.SetIsWebSocketRequest(true)
        );
    }

    /// <summary>
    /// Sets the protocols "requested" for the request
    /// </summary>
    /// <param name="protocols"></param>
    /// <returns></returns>
    public WebSocketManagerBuilder WithWebSocketRequestedProtocols(
        IEnumerable<string> protocols
    )
    {
        return With<FakeWebSocketManager>(
            o => o.SetProtocols(protocols)
        );
    }
}