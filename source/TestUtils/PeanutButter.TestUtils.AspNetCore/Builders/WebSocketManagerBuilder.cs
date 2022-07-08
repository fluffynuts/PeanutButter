using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class WebSocketManagerBuilder : Builder<WebSocketManagerBuilder, WebSocketManager>
    {
        public override WebSocketManagerBuilder Randomize()
        {
            return this; // nothing really to randomize on this
        }

        protected override WebSocketManager ConstructEntity()
        {
            return new FakeWebSocketManager();
        }

        public WebSocketManagerBuilder WithWebSocketAcceptHandler(
            Func<string, Task<WebSocket>> handler
        )
        {
            return With<FakeWebSocketManager>(
                o => o.SetWebSocketAcceptHandler(handler)
            );
        }

        public WebSocketManagerBuilder WithIsWebSocketRequest(bool flag)
        {
            return With<FakeWebSocketManager>(
                o => o.SetIsWebSocketRequest(true)
            );
        }

        public WebSocketManagerBuilder WithWebSocketRequestedProtocols(
            IEnumerable<string> protocols
        )
        {
            return With<FakeWebSocketManager>(
                o => o.SetProtocols(protocols)
            );
        }
    }
}