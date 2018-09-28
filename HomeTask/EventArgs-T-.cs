using System;

namespace BitMEXAssistant {
    public class EventArgs<T> : EventArgs {
        public EventArgs(T data) {
            Data = data;
        }

        public T Data { get; }
    }
}