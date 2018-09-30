using System;
using System.Collections.Generic;
using BitMEXAssistant;

namespace HomeTask.Tests {
    public class TestWebSocket : IWebSocket {
        public event EventHandler<EventArgs<string>> Message;
        public event EventHandler<EventArgs<Exception>> Error;

        public List<string> LastData { get; } = new List<string>();

        public void Connect() { }

        public void Send(string data) {
            LastData.Add(data);
        }

        public void RaiseOnMessage(string message) => OnMessage(new EventArgs<string>(message));
        public void RaiseOnError(Exception exception) => OnError(new EventArgs<Exception>(exception));

        protected virtual void OnMessage(EventArgs<string> e) {
            Message?.Invoke(this, e);
        }

        protected virtual void OnError(EventArgs<Exception> e) {
            Error?.Invoke(this, e);
        }
    }
}