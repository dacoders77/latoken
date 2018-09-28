namespace BitMEXAssistant {

	/* Trade type class.
	 * A trade has thre params: price, volume and direction.
	 * Used in severeal cases like: trade event, DOM event etc. 
	 */
	 
    public class TradeData {
        public TradeData(decimal price, double volume, TradeDirection direction) {
            Price = price;
            Volume = volume;
            Direction = direction;
        }
        public decimal Price { get; }
        public double Volume { get; } //TODO double? Is type correct?
        public TradeDirection Direction { get; }
    }
}