using System.Collections.ObjectModel;

namespace BitMEXAssistant {

    public class OrderBookDataSet {
        public OrderBookDataSet(ReadOnlyCollection<OrderBookRecord> ask, ReadOnlyCollection<OrderBookRecord> bid) {
            Ask = ask;
            Bid = bid;
        }
        public ReadOnlyCollection<OrderBookRecord> Ask { get; }
        public ReadOnlyCollection<OrderBookRecord> Bid { get; }
    }
}