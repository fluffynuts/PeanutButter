using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    /// <summary>
    /// Provides a fake websocket
    /// </summary>
    public class FakeWebSocket : WebSocket, IFake
    {
        /// <inheritdoc />
        public override void Abort()
        {
            _state = WebSocketState.Aborted;
        }

        /// <inheritdoc />
        public override Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string statusDescription,
            CancellationToken cancellationToken
        )
        {
            _state = WebSocketState.Closed;
            _closeStatus = closeStatus;
            _closeStatusDescription = statusDescription;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task CloseOutputAsync(
            WebSocketCloseStatus closeStatus,
            string statusDescription,
            CancellationToken cancellationToken)
        {
            return CloseAsync(closeStatus, statusDescription, cancellationToken);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (State == WebSocketState.Closed)
            {
                return;
            }

            CloseAsync(WebSocketCloseStatus.NormalClosure, "disposed", CancellationToken.None);
        }

        /// <inheritdoc />
        public override Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken
        )
        {
            return _receiver(buffer, cancellationToken);
        }

        private Func<ArraySegment<byte>, CancellationToken, Task<WebSocketReceiveResult>> _receiver
            = DefaultReceiver;

        private static Task<WebSocketReceiveResult> DefaultReceiver(
            ArraySegment<byte> arg1,
            CancellationToken arg2
        )
        {
            return Task.FromResult(new WebSocketReceiveResult(arg1.Count, WebSocketMessageType.Text, true));
        }

        /// <inheritdoc />
        public override Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            return _sender(buffer, messageType, endOfMessage, cancellationToken);
        }

        private Func<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken, Task>
            _sender = DefaultSender;

        private static Task DefaultSender(
            ArraySegment<byte> arg1,
            WebSocketMessageType arg2,
            bool arg3,
            CancellationToken arg4
        )
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override WebSocketCloseStatus? CloseStatus => _closeStatus;
        private WebSocketCloseStatus? _closeStatus;

        /// <inheritdoc />
        public override string CloseStatusDescription => _closeStatusDescription;
        private string _closeStatusDescription;

        /// <inheritdoc />
        public override WebSocketState State => _state;
        private WebSocketState _state = WebSocketState.None;

        /// <inheritdoc />
        public override string SubProtocol => _subProtocol;
        private string _subProtocol;

        /// <summary>
        /// Set the receive handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetReceiveHandler(
            Func<ArraySegment<byte>, CancellationToken, Task<WebSocketReceiveResult>> handler
        )
        {
            _receiver = handler ?? DefaultReceiver;
        }

        /// <summary>
        /// Set the send handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetSendHandler(
            Func<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken, Task> handler
        )
        {
            _sender = handler ?? DefaultSender;
        }

        /// <summary>
        /// Set the sub-protocol
        /// </summary>
        /// <param name="subProtocol"></param>
        public void SetSubProtocol(string subProtocol)
        {
            _subProtocol = subProtocol;
        }
    }
}