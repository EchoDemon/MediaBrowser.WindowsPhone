﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.ApiInteraction.WebSocket;
using WebSocket4Net;

namespace MediaBrowser.Model
{
    public class WebSocketClient : IClientWebSocket
    {
        private WebSocket _socket;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public Net.WebSocketState State
        {
            get
            {

                switch (_socket.State)
                {
                    case WebSocketState.Closed:
                        return Net.WebSocketState.Closed;
                    case WebSocketState.Closing:
                        return Net.WebSocketState.Closed;
                    case WebSocketState.Connecting:
                        return Net.WebSocketState.Connecting;
                    case WebSocketState.None:
                        return Net.WebSocketState.None;
                    case WebSocketState.Open:
                        return Net.WebSocketState.Open;
                    default:
                        return Net.WebSocketState.None;
                }
            }
        }

        /// <summary>
        /// Connects the async.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public Task ConnectAsync(string url, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            try
            {
                _socket = new WebSocket(url);

                _socket.MessageReceived += websocket_MessageReceived;

                _socket.Open();

                _socket.Opened += (sender, args) => taskCompletionSource.TrySetResult(true);
                _socket.Closed += _socket_Closed;
            }
            catch (Exception ex)
            {
                _socket = null;

                taskCompletionSource.TrySetException(ex);
            }

            return taskCompletionSource.Task;
        }

        void _socket_Closed(object sender, EventArgs e)
        {
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the MessageReceived event of the websocket control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MessageReceivedEventArgs"/> instance containing the event data.</param>
        void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (OnReceiveDelegate != null)
            {
                OnReceiveDelegate(Encoding.UTF8.GetBytes(e.Message));
            }
        }

        /// <summary>
        /// Gets or sets the receive action.
        /// </summary>
        /// <value>The receive action.</value>
        public Action<byte[]> OnReceiveDelegate { get; set; }

        /// <summary>
        /// Sends the async.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="type">The type.</param>
        /// <param name="endOfMessage">if set to <c>true</c> [end of message].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public Task SendAsync(byte[] bytes, Net.WebSocketMessageType type, bool endOfMessage, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => _socket.Send(bytes, 0, bytes.Length), new CancellationToken());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
        }

        public event EventHandler Closed;


        public Action<string> OnReceive { get; set; }

        public Action<byte[]> OnReceiveBytes { get; set; }
    }
}
