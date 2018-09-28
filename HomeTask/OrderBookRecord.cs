namespace BitMEXAssistant {
    public struct OrderBookRecord {
        public OrderBookRecord(decimal price, int volume) {
            Price = price;
            Volume = volume;
        }
        public decimal Price { get; }
        public int Volume { get; }
    }
}