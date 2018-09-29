using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMEXAssistant
{
	// Exchange event like onTrade, onOrderBook, onPositionChanged etc. are moved to this class
	// This class is gonna be inherited in TradeBitMex.cs and TradeHitBtc.cs in order to provide the same functionality
	// Don't make any changes to these events!

	public class WSEvents
	{
		// Events declaration
		// Trade data received event
		public event EventHandler<EventArgs<TradeData>> TradeDataReceived;
		public void RaiseTradeDataReceived(TradeData data) => OnTradeDataReceived(new EventArgs<TradeData>(data));
		protected virtual void OnTradeDataReceived(EventArgs<TradeData> e) => TradeDataReceived?.Invoke(this, e);

		// Order book event received
		public event EventHandler<EventArgs<OrderBookDataSet>> OrderBookReceived;
		public void RaiseOrderBookReceived(OrderBookDataSet data) => OnOrderBookReceived(new EventArgs<OrderBookDataSet>(data));
		protected virtual void OnOrderBookReceived(EventArgs<OrderBookDataSet> e) => OrderBookReceived?.Invoke(this, e);

		// Balance received event. // We don't need balance events for now
		public event EventHandler<EventArgs<decimal>> BalanceReceived;
		public void RaiseBalanceReceived(decimal data) => OnBalanceReceived(new EventArgs<decimal>(data));
		protected virtual void OnBalanceReceived(EventArgs<decimal> e) => BalanceReceived?.Invoke(this, e);
	}
}
