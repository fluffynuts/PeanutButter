using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    public class WebSocketBuilder : Builder<WebSocketBuilder, WebSocket>
    {
        public override WebSocketBuilder Randomize()
        {
            // nothing worth randomizing here
            return this;
        }

        protected override WebSocket ConstructEntity()
        {
            return GenericBuilderBase.TryCreateSubstituteFor<FakeWebSocket>(
                throwOnError: false,
                callThrough: true,
                out var result
            )
                ? result
                : new FakeWebSocket();
        }

        public WebSocketBuilder WithReceiveHandler(
            Func<ArraySegment<byte>, CancellationToken, Task<WebSocketReceiveResult>> handler
        )
        {
            return With<FakeWebSocket>(
                o => o.SetReceiveHandler(handler)
            );
        }

        public WebSocketBuilder WithSendHandler(
            Func<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken, Task> handler
        )
        {
            return With<FakeWebSocket>(
                o => o.SetSendHandler(handler)
            );
        }

        public WebSocketBuilder WithSubProtocol(
            string subProtocol
        )
        {
            return With<FakeWebSocket>(
                o => o.SetSubProtocol(subProtocol)
            );
        }
    }
}