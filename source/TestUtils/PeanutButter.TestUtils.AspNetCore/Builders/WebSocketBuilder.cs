using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Fakes;
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds a fake web socket
    /// </summary>
    public class WebSocketBuilder : Builder<WebSocketBuilder, WebSocket>
    {
        /// <summary>
        /// Does nothing - only here so that GetRandom works
        /// </summary>
        /// <returns></returns>
        public override WebSocketBuilder Randomize()
        {
            // nothing worth randomizing here
            return this;
        }

        /// <summary>
        /// Constructs the fake web socket
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the receive handler for the fake socket
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public WebSocketBuilder WithReceiveHandler(
            Func<ArraySegment<byte>, CancellationToken, Task<WebSocketReceiveResult>> handler
        )
        {
            return With<FakeWebSocket>(
                o => o.SetReceiveHandler(handler)
            );
        }

        /// <summary>
        /// Sets the send handler for the fake socket
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public WebSocketBuilder WithSendHandler(
            Func<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken, Task> handler
        )
        {
            return With<FakeWebSocket>(
                o => o.SetSendHandler(handler)
            );
        }

        /// <summary>
        /// Sets the sub-protocol for the fake socket
        /// </summary>
        /// <param name="subProtocol"></param>
        /// <returns></returns>
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