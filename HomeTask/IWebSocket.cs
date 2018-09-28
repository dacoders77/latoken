using System;

namespace BitMEXAssistant {
    public interface IWebSocket {
        event EventHandler<EventArgs<string>> Message;
        event EventHandler<EventArgs<Exception>> Error;
        void Connect();
        void Send(string data);
    }
}