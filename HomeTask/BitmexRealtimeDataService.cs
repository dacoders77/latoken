using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BitMEXAssistant {

	// Realtime listener and auth websocket class
	// Rest API auth is performed separately in BitMexAPI.cs

    public class BitmexRealtimeDataService : WSEvents {

        private readonly BitmexDataService _dataService;
		private TradeBitMex2 _tradeBitMex2;
		private TradeBitMex2 _tradeBitMex3; // Works good

		private string _symbol; 

        public BitmexRealtimeDataService(BitmexDataService dataService, string symbol) {
            _dataService = dataService;
			_symbol = symbol;
        }

        public void Initialize() {

            _dataService.WebSocket.Message += WebSocketOnMessage;
            _dataService.WebSocket.Error += WebSocketOnError;

			_tradeBitMex2 = new TradeBitMex2(this, _dataService, 0); // Create the reading class + set up order book limit orders shift (0, 0.5. 1 ..)
			//_tradeBitMex3 = new TradeBitMex2(this, _dataService, 1); // Works good

			InitializeSymbolSpecificData(true);
        }

        private void InitializeSymbolSpecificData(bool firstLoad = false) {
            if (!firstLoad) {
                // Unsubscribe from old orderbook
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"orderBook10:" + _symbol + "\"]}");

                // Unsubscribe from old instrument position
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"position:" + _symbol + "\"]}");

                // Unsubscribe from orders stauses
                _dataService.WebSocket.Send("{\"op\": \"unsubscribe\", \"args\": [\"order\"]}");

                // Replace dictionary symbol pull-up with fixed values. TEST
                //ActiveInstrument = bitmex.GetInstrument(((Instrument)ddlSymbol.SelectedItem).Symbol)[0];
                _dataService.ActiveInstrument.Symbol = _symbol;
                _dataService.ActiveInstrument.TickSize = 0.5M;
                _dataService.ActiveInstrument.Volume24H = 9000; // A random test value
            }

            // Subscribe to orders statuses
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"order\"]}");

            // Subscribe to orderbook
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"orderBook10:" + _symbol + "\"]}");

            // Subscribe to position for new symbol
            //ws.Send("{\"op\": \"subscribe\", \"args\": [\"position:" + ActiveInstrument.Symbol + "\"]}");

            // Only subscribing to this symbol trade feed now, was too much at once before with them all.
            _dataService.WebSocket.Send("{\"op\": \"subscribe\", \"args\": [\"trade:" + _symbol + "\"]}");

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

					_tradeBitMex2.orderBookRecevied(e); // Call trade class methid for orders placement
					//_tradeBitMex3.orderBookRecevied(e);

					switch ((string)message["table"]) {
                        case "trade":

							Console.WriteLine("******************************************** " + data.Children().Count());

                            var price = (double)data.Children().Last()["price"]; // TD["price"].Value<double>() - correct?
                            var symbol = (string)data.Children().Last()["symbol"];
                            var volume = (double)data.Children().Last()["size"];
                            var side = data.Children().Last()["side"].ToString().ToUpperInvariant() == "BUY" ? TradeDirection.Buy : TradeDirection.Sell;

                            RaiseTradeDataReceived(new TradeData((decimal) price, volume, side)); // Call the method and rise an event
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
		// All events are movet to WSEvents.cs class
    }
}