using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace BitMEXAssistant
{
	// Fresh copy of TradeBitMex.cs class which later will be deprecated

	class TradeBitMex2
	{
		private BitmexRealtimeDataService _bitmexRealtimeDataService;

		private BitmexDataService _bitmexDataService;

		bool activeSellOrder = false;
		bool activeBuyOrder = false;
		bool openNewPairOrderEnabled = true;

		string sellOrderId;
		double sellLimitPrice;

		string buyOrderId;
		double buyLimitPrice;

		private double _limitPriceShift; // = 1; // 0.5, 1, 2. 0 - Orders will be place right at bid/ask price. No gap
		int apiRequestDelay = 500; // Api delay request
		private DateTime rateLimitDate = new DateTime(2015, 1, 1);

		private Dictionary<string, Order> order; // The same as order statuses but contains Order object as the value


		public TradeBitMex2(BitmexRealtimeDataService bitmexRealtimeDataService, BitmexDataService bitmexDataService, double limitPriceShift) {

			_limitPriceShift = limitPriceShift;
			_bitmexRealtimeDataService = bitmexRealtimeDataService;
			_bitmexDataService = bitmexDataService;
			order = new Dictionary<string, Order>();
		}

		public void orderBookRecevied(EventArgs<string> e) {

			var message = JObject.Parse(e.Data);
			
			if (message.ContainsKey("table"))
			{
				if ((string)message["table"] == "orderBook10")
				{
					if (message.ContainsKey("data"))
					{
						JArray TD = (JArray)message["data"];
						if (TD.Any())
						{
							Random rnd = new Random();
							string suffix = rnd.Next(1000, 10000).ToString(); // We use one Client order id to put in both sell and buy orders. The first part of this Id changes all the time. The last part - stays the same. This is because the exchange does not allow to have the same Client order ids

							//Console.WriteLine("TradeBitMex2.cs. openNewPairOrderEnabled: " + openNewPairOrderEnabled);

							// Place sell order
							if (!activeSellOrder && openNewPairOrderEnabled)
							{
								
								sellLimitPrice = (Double)TD[0]["asks"][0][0] + _limitPriceShift;
								string response = _bitmexDataService.Api.LimitOrder("XBTUSD", "Sell", 10, sellLimitPrice, (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "." + suffix, false, false, false);
								//MessageBox.Show(response);

								sellOrderId = JObject.Parse(response)["orderID"].ToString();
								//Console.WriteLine("------------ Place order response. TradeBitMex2.cs");
								//Console.WriteLine(response);


								order.Add(sellOrderId, new Order(sellOrderId, JObject.Parse(response)["ordStatus"].ToString(), TradeDirection.Sell));
								activeSellOrder = true; // Set flag to true when the order is opened 
							
							}

							// Place buy order
							if (!activeBuyOrder && openNewPairOrderEnabled)
							{
								
								buyLimitPrice = (Double)TD[0]["bids"][0][0] - _limitPriceShift;
								string response = _bitmexDataService.Api.LimitOrder("XBTUSD", "Buy", 10, buyLimitPrice, (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "." + suffix, false, false, false);
								//MessageBox.Show(response);

								buyOrderId = JObject.Parse(response)["orderID"].ToString();
								//Console.WriteLine("------------ Place order response. TradeBitMex2.cs");
								//Console.WriteLine(response);

								order.Add(buyOrderId, new Order(buyOrderId, JObject.Parse(response)["ordStatus"].ToString(), TradeDirection.Buy));
								activeBuyOrder = true; // Set flag to true when the order is opened 
								
							}

							// When BUY and SELL orders are filled wait untill the whole position is closed. Both orders are filled
							openNewPairOrderEnabled = false; 


							// When BUY and SELL orders are filled (both orders in a position are osed) - allow to place new pair of orders (open a new position)
							if (!activeSellOrder && !activeBuyOrder)
							{
								openNewPairOrderEnabled = true;
								//.Show("Both filled! " + openNewPairOrderEnabled);
							}



							// Move sell order (Amend)
							if (order[sellOrderId].Status == "New")
							{
								if ((sellLimitPrice - _limitPriceShift) == (Double)TD[0]["asks"][0][0])
								{
									//Console.WriteLine("SELL Order has not moved");
								}
								else
								{
									// Rate limit
									if (DateTime.Compare(DateTime.Now, rateLimitDate) > 0)
									{
										rateLimitDate = DateTime.Now.AddMilliseconds(apiRequestDelay);
										sellLimitPrice = (Double)TD[0]["asks"][0][0] + _limitPriceShift;

										string response = _bitmexDataService.Api.AmendOrder(sellOrderId, sellLimitPrice);
										Console.WriteLine("-------------------------- SELL order has moved! Amend it!: " + response);
									}
								}
							}


							// Move buy order (Amend)
							if (order[sellOrderId].Status == "New")
							{
								if ((buyLimitPrice + _limitPriceShift) == (Double)TD[0]["bids"][0][0])
								{
									//Console.WriteLine("BUY Order has not moved");
								}
								else
								{
									// Rate limit
									if (DateTime.Compare(DateTime.Now, rateLimitDate) > 0)
									{
										rateLimitDate = DateTime.Now.AddMilliseconds(apiRequestDelay);
										buyLimitPrice = (Double)TD[0]["bids"][0][0] - _limitPriceShift;

										string response = _bitmexDataService.Api.AmendOrder(buyOrderId, buyLimitPrice);
										//Console.WriteLine("-------------------------- SELL order has moved! Amend it!: " + response);
									}
								}
							}
						}
					}
				}

				// When order statuses are received. Filled, amended, cancel etc.
				else if ((string)message["table"] == "order")
				{
					//Console.WriteLine("############" + message);

					if (message.ContainsKey("data"))
					{
						JArray TD = (JArray)message["data"];
						if (TD.Any())
						{

							// If the key is found in order dictionary - update its value
							if (order.ContainsKey((string)TD[0]["orderID"]))
							{
								
								// In some cases an orderStatus can be empty
								if (TD[0]["ordStatus"] != null) 
								{


									// Update existing status of the order Filled. Canceled, New etc. statuses are not taken into the account
									order[(string)TD[0]["orderID"]].Status = (string)TD[0]["ordStatus"];

									// STATUSES FOR NEW ORDER PAIR OPEN
									// New orders flags

									if (order[sellOrderId].Status == "Filled")
									{
										activeSellOrder = false;
										System.Threading.Thread.Sleep(2000); // Sleep untill new order is placed. REMOVE THIS
									}

									if (order[buyOrderId].Status == "Filled")
									{
										activeBuyOrder = false;
										System.Threading.Thread.Sleep(2000); // Sleep untill new order is placed. REMOVE THIS
									}

							


									// Only for statuses except Filled
									if (TD[0]["ordStatus"].ToString() != "Filled") {

									


										// Extract client order ID as a suffix. Get last 4 digits out of the string
										var clOrdID = TD[0]["clOrdID"].ToString().Substring(TD[0]["clOrdID"].ToString().Length - 4);

										// Add client order id to the DB as a BLUEPRINT
										// WORKS GOOD

										try
										{
											_bitmexDataService.dataBase.InsertTradeRow(clOrdID);
										}
										catch (Exception ex)
										{
											Console.WriteLine("TradeBitMex2.cs line 205. Exception thrown: " + ex);
										}
										
									}
								}
							}
						}
					}
				}

				if ((string)message["action"] == "update")
				{
					//MessageBox.Show("update");
					
					if (message.ContainsKey("data"))
					{
						JArray TD = (JArray)message["data"];
						if (TD.Any())
						{

							if ((string)TD[0]["ordStatus"] == "Filled")
							{
								//order[TD[0]["orderID"].ToString()].Status = "Filled";


								// Extract client order ID as a suffix. Get last 4 digits out of the string
								//Console.WriteLine("TradeBitMext2.cs line 226. TD0 " + TD[0]["clOrdID"].ToString().Substring(TD[0]["clOrdID"].ToString().Length - 4));

								var clOrdID = TD[0]["clOrdID"].ToString().Substring(TD[0]["clOrdID"].ToString().Length - 4);

								try
								{
									_bitmexDataService.dataBase.UpdateRecord(clOrdID, (string)TD[0]["orderID"], order[TD[0]["orderID"].ToString()].Direction, (double)TD[0]["avgPx"]);
								}
								catch (Exception ex)
								{
									Console.WriteLine("TradeBitMex2.cs line 205. Exception thrown: " + ex);
								}
								

								//foreach (KeyValuePair<string, Order> entry in order)
								//{
								//	Console.WriteLine("****:" + entry.Key + " id: " + entry.Value.Id + " direction:" + entry.Value.Direction + " status:" + entry.Value.Status);
								//}

							}
						}
					}
				}
			}
		}
	}
}
