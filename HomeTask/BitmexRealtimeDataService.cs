using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BitMEXAssistant {
    public class BitmexRealtimeDataService {
        private string symbol = "ETHUSD"; // ETHUSD. Subscription symbol
        private readonly BitmexDataService _dataService;

        public BitmexRealtimeDataService(BitmexDataService dataService) {
            _dataService = dataService;
        }

        public void Initialize() {
            //_api = new BitMEXApi(Settings.bitmexApiKey, Settings.bitmexApiSecret, false);
            //_webSocket = _createWebSocket();

            _dataService.WebSocket.Message += WebSocketOnMessage;
            _dataService.WebSocket.Error += WebSocketOnError;

            InitializeSymbolSpecificData(true);
        }

        private void InitializeSymbolSpecificData(bool firstLoad = false) {
            if (!firstLoad) {
                // Unsubscribe from old orderbook
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"orderBook10:" + symbol + "\"]}");

                // Unsubscribe from old instrument position
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"position:" + symbol + "\"]}");

                // Unsubscribe from orders stauses
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"order\"]}");

                // Replace dictionary symbol pull-up with fixed values. TEST
                //ActiveInstrument = bitmex.GetInstrument(((Instrument)ddlSymbol.SelectedItem).Symbol)[0];
                _dataService.ActiveInstrument.Symbol = symbol;
                _dataService.ActiveInstrument.TickSize = 0.5M;
                _dataService.ActiveInstrument.Volume24H = 9000; // A random test value
            }

            // Subscribe to orders statuses
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"order\"]}");

            // Subscribe to orderbook
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"orderBook10:" + symbol + "\"]}");

            // Subscribe to position for new symbol
            //ws.Send("{\"op\": \"subscribe\", \"args\": [\"position:" + ActiveInstrument.Symbol + "\"]}");

            // Only subscribing to this symbol trade feed now, was too much at once before with them all.
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"trade:" + symbol + "\"]}");

            // Margin Connect - do this last so we already have the price.
            //ws.Send("{\"op\": \"subscribe\", \"args\": [\"margin\"]}");

            //UpdateFormsForTickSize(ActiveInstrument.TickSize, ActiveInstrument.DecimalPlacesInTickSize);

        }

        private void WebSocketOnMessage(object sender, EventArgs<string> e) {
            try {
                var message = JObject.Parse(e.Data);
                if (message.ContainsKey("table")) {

                    if (!message.ContainsKey("data"))
                        return;

                    var data = (JArray)message["data"];
                    if (!data.Any())
                        return;

                    switch ((string)message["table"]) {
                        case "trade":
                            var price = (double)data.Children().Last()["price"]; // TD["price"].Value<double>() - correct?
                            var symbol = (string)data.Children().Last()["symbol"];
                            var volume = (double)data.Children().Last()["size"];
                            var side = data.Children().Last()["side"].ToString().ToUpperInvariant() == "BUY" ? TradeDirection.Buy : TradeDirection.Sell;

                            RaiseTradeDataReceived(new TradeData((decimal) price, volume, side)); // Call a method and rise an event
                            break;
                        case "orderBook10":
                            var asks = (JArray)data[0]["asks"];
                            var bids = (JArray)data[0]["bids"];

                            RaiseOrderBookReceived(
                                new OrderBookDataSet(
                                    asks.Select(t => new OrderBookRecord((decimal) t[0], (int) t[1])).ToList().AsReadOnly(),
                                    bids.Select(t => new OrderBookRecord((decimal) t[0], (int) t[1])).ToList().AsReadOnly()
                                )
                            );
                            break;
                        case "margin":
                            var balance = (decimal)data.Children().Last()["walletBalance"] / 100000000;
                            RaiseBalanceReceived(balance);
                            break;
                    }

                } 
//                else if (message.ContainsKey("info") && message.ContainsKey("docs")) {
//                    string WebSocketInfo = "Websocket Info: " + message["info"].ToString() + " " + message["docs"].ToString();
//                    UpdateWebSocketInfo(WebSocketInfo);
//                }
            } catch (Exception ex) {
                // TODO make logging
                throw;
            }
        }

        private void WebSocketOnError(object sender, EventArgs<Exception> e) {
            throw new AggregateException(e.Data);
        }

		// Events declaration

		// Trade data received event
        public event EventHandler<EventArgs<TradeData>> TradeDataReceived;
        private void RaiseTradeDataReceived(TradeData data) => OnTradeDataReceived(new EventArgs<TradeData>(data));
        protected virtual void OnTradeDataReceived(EventArgs<TradeData> e) => TradeDataReceived?.Invoke(this, e);

		// Order book event received
        public event EventHandler<EventArgs<OrderBookDataSet>> OrderBookReceived;
        private void RaiseOrderBookReceived(OrderBookDataSet data) => OnOrderBookReceived(new EventArgs<OrderBookDataSet>(data));
        protected virtual void OnOrderBookReceived(EventArgs<OrderBookDataSet> e) => OrderBookReceived?.Invoke(this, e);

		// Balance received event. // We don't need balance events for now
		public event EventHandler<EventArgs<decimal>> BalanceReceived; 
        private void RaiseBalanceReceived(decimal data) => OnBalanceReceived(new EventArgs<decimal>(data));
        protected virtual void OnBalanceReceived(EventArgs<decimal> e) => BalanceReceived?.Invoke(this, e);
    }
}