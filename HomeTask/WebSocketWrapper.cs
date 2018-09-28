using System;
using WebSocketSharp;

namespace BitMEXAssistant {
    public class WebSocketWrapper : IWebSocket {
        private readonly WebSocket _webSocket;

        public WebSocketWrapper(string url) {
            _webSocket = new WebSocket(url);
            _webSocket.OnMessage += (sender, e) => OnMessage(new EventArgs<string>(e.Data));
            _webSocket.OnError += (sender, e) => OnError(new EventArgs<Exception>(e.Exception));
        }

        public void Connect() => _webSocket.Connect();

        public void Send(string data) => _webSocket.Send(data);

        public event EventHandler<EventArgs<string>> Message;

        protected virtual void OnMessage(EventArgs<string> e) => Message?.Invoke(this, e);

        public event EventHandler<EventArgs<Exception>> Error;

        protected virtual void OnError(EventArgs<Exception> e) => Error?.Invoke(this, e);
    }
}