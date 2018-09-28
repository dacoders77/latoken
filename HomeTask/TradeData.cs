namespace BitMEXAssistant {
    public class TradeData {
        public TradeData(decimal price, double volume, TradeDirection direction) {
            Price = price;
            Volume = volume;
            Direction = direction;
        }
        public decimal Price { get; }
        public double Volume { get; } //TODO double? проверить типы
        public TradeDirection Direction { get; }
    }
}