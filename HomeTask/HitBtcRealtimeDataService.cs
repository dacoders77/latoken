using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace BitMEXAssistant
{
	/*
	* Realtime listener and auth websocket class. WS subscriptions and parsing
	* Rest API is not used at HitBtc exchange. Orders are sent through websocket stream via websocket.send
	* Websocket events like onTrade, onOrderBookChanged etc. are inherited from WSEvents inerface
	* The same inheritance is used in other realtime class - BitMexRealtimeDataService.cs
	* Database instance is used for updating records in DB when hedge market orders are filled
	*/

	public class HitBtcRealtimeDataService : WSEvents
	{

		private readonly IDataService _dataService;
		private readonly DataBase _dataBase;
		private string _symbol;

		public HitBtcRealtimeDataService(IDataService dataService, DataBase dataBase, string symbol)
		{
			_dataService = dataService;
			_symbol = symbol;
			_dataBase = dataBase;
		}

		public void Initialize()
		{

			_dataService.WebSocket.Message += WebSocketOnMessage;
			//_dataService.WebSocket.Error += WebSocketOnError; // There is no onError message at HitBtc

			InitializeSymbolSpecificData(true);
		}

		private void InitializeSymbolSpecificData(bool firstLoad = false)
		{
			// Subscribe to reports (Order statuses)
			_dataService.WebSocket.Send("{\"method\": \"subscribeReports\", \"params\": {} } "); // Works good

			// Subscribe to ticker
			//_dataService.WebSocket.Send("{\"method\": \"subscribeTicker\", \"params\": {\"symbol\": \"ETHBTC\"},\"id\": 123 } "); // Works good

			// New order
			//Int32 b = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
			//_dataService.WebSocket.Send("{\"method\": \"newOrder\",\"params\": {\"clientOrderId\": \"" + b + "\",\"symbol\": \"EOSUSD\",\"side\": \"buy\",\"type\": \"market\",\"price\": \"0.059837\",\"quantity\": \"0.01\"},\"id\": 123}");

		}

		private void WebSocketOnMessage(object sender, EventArgs<string> e)
		{

			var message = JObject.Parse(e.Data);
			Console.WriteLine("HitBtcRealtimeDataService.cs: " + message);

			/*
			try
			{
				var message = JObject.Parse(e.Data);
				if (message.ContainsKey("table"))
				{

					if (!message.ContainsKey("data"))
						return;

					var data = (JArray)message["data"];
					if (!data.Any())
						return;

					switch ((string)message["table"])
					{
						case "trade":

							//Console.WriteLine("****" + data.Children().Count()); // Trades are grouped in the message. There may be more than 1 element in data

							var price = (double)data.Children().Last()["price"]; // TD["price"].Value<double>() - correct?
							var symbol = (string)data.Children().Last()["symbol"];
							var volume = (double)data.Children().Last()["size"];
							var side = data.Children().Last()["side"].ToString().ToUpperInvariant() == "BUY" ? TradeDirection.Buy : TradeDirection.Sell;

							RaiseTradeDataReceived(new TradeData((decimal)price, volume, side)); // Call the method and rise an event
							break;
						case "orderBook10":
							var asks = (JArray)data[0]["asks"];
							var bids = (JArray)data[0]["bids"];

							RaiseOrderBookReceived(
								new OrderBookDataSet(
									asks.Select(t => new OrderBookRecord((decimal)t[0], (int)t[1])).ToList().AsReadOnly(),
									bids.Select(t => new OrderBookRecord((decimal)t[0], (int)t[1])).ToList().AsReadOnly()
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
			}
			catch (Exception ex)
			{
				// TODO make logging
				throw;
			}

			*/
		}

		private void WebSocketOnError(object sender, EventArgs<Exception> e)
		{
			throw new AggregateException(e.Data);
		}

		public void HitBtcHedgePositionOpen(string clOrdID, TradeDirection direction)
		{

			// New order
			//Int32 b = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
			clOrdID = "123456" + clOrdID;
			_dataService.WebSocket.Send("{\"method\": \"newOrder\",\"params\": {\"clientOrderId\": \"" + clOrdID + "\",\"symbol\": \"ETHUSD\",\"side\": \""+ direction.ToString().ToLower() +"\",\"type\": \"market\",\"price\": \"0.05983\",\"quantity\": \"0.001\"},\"id\": 123}");

		}


		// Events declaration
		// All events are movet to WSEvents.cs class
	}
}