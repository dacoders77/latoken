using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace BitMEXAssistant
{
	/* Main trading class.
	 * Listens to websocket events and messages. 
	 * New order / Amend (move) order requests are sent via REST api requests handled in BitMexAPI.cs.
	 * When order statuses are received - trading information like trade price, volume, order id / order client id etc. is stored in DB.
	 * Client order id (ClOrdID) is attached to both buy and sell limit orders. This action is performed in order to link them together in DB.
	 * ClOrdID is firstly added to an empty row as a blueprint. Later both orders are gonna be inserted excatly to the same row.
	 * ClOrdID looks like this: 1538082422.52331.8495
	 * Only 4 last digits are are used for orders linking. This value is stored in DB in cl_order_id column.
	 * A number of instances of the Trade class can be made, each instance will write to an independant allocated row.
	 * When orders are executed and Filled websocket message is received - both orders are stored in the same row in the DB. Profit is calculated.
	 */

	class Trade
	{
		private Form1 form;
		bool activeSellOrder = false;
		bool activeBuyOrder = false;
		bool openNewPairOrderEnabled = true;

		string sellOrderId;
		double sellLimitPrice;

		string buyOrderId;
		double buyLimitPrice;

		double limitPriceShift = 0; // 0.5, 1, 2. 0 - Orders will be place right at bid/ask price. No gap
		int apiRequestDelay = 500; // Api delay request
		private DateTime rateLimitDate = new DateTime(2015, 1, 1);

		
		private Dictionary<string, Order> order; // The same as order statuses but contains Order object as the value

		// Constructor
		public Trade(Form1 Form) {

			form = Form;
			order = new Dictionary<string, Order>();

		}

		public void placeLimitOrder() {

			form.ws.Message += (sender, e) =>
			{
				try
				{
					JObject Message = JObject.Parse(e.Data); // Parse each WS message

					if (Message.ContainsKey("table"))
					{
						if ((string)Message["table"] == "orderBook10")
						{
							if (Message.ContainsKey("data"))
							{
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{
									Random rnd = new Random();
									string suffix = rnd.Next(1000, 10000).ToString(); // We use one Client order id to put in both sell and buy orders. The first part of this Id changes all the time. The last part - stays the same. This is because the exchange does not allow to have the same Client order ids

									// Place sell order
									if (!activeSellOrder && openNewPairOrderEnabled)
									{
										sellLimitPrice = (Double)TD[0]["asks"][0][0] + limitPriceShift;
										string response = form.bitmex.LimitOrder(form.symbol, "Sell", 1, sellLimitPrice, (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "." + suffix, false, false, false);
										//Console.WriteLine("------------ Place order response");
										//Console.WriteLine(response);
										sellOrderId = JObject.Parse(response)["orderID"].ToString();

										order.Add(sellOrderId, new Order(sellOrderId, JObject.Parse(response)["ordStatus"].ToString(), "Sell"));
										activeSellOrder = true; // Set flag to true when the order is opened 
									}

									// Place buy order
									if (!activeBuyOrder && openNewPairOrderEnabled)
									{
										buyLimitPrice = (Double)TD[0]["bids"][0][0] - limitPriceShift;
										string response = form.bitmex.LimitOrder(form.symbol, "Buy", 1, buyLimitPrice, (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "." + suffix, false, false, false);
										buyOrderId = JObject.Parse(response)["orderID"].ToString();
										//Console.WriteLine("------------ Place order response");
										//Console.WriteLine(response);
										
										order.Add(buyOrderId, new Order(buyOrderId, JObject.Parse(response)["ordStatus"].ToString(), "Buy"));
										activeBuyOrder = true; // Set flag to true when the order is opened 
									}

									openNewPairOrderEnabled = false; // When BUY and SELL orders are open wait untill the position is closed
									//Console.WriteLine("Order status: " + orderStatuses[sellOrderId]);

									// New orders flags
									if (order[sellOrderId].Status == "Filled")
									{
										System.Threading.Thread.Sleep(2000); // Sleep untill new order is placed. REMOVE THIS
										activeSellOrder = false;
									}

									if (order[buyOrderId].Status == "Filled")
									{
										System.Threading.Thread.Sleep(2000); // Sleep untill new order is placed. REMOVE THIS
										activeBuyOrder = false;
									}

									//foreach (KeyValuePair<string, string> entry in orderStatuses)
									//{
									//	//Console.WriteLine("****:" + entry.Key + " / " + entry.Value);
									//}

									// When BUY and SELL orders are filled (position closed) - allow to place new orders (open a new position)
									//Console.WriteLine(!activeSellOrder + " " + !activeBuyOrder);
									if (!activeSellOrder && !activeBuyOrder)
									{
										openNewPairOrderEnabled = true;
										//MessageBox.Show("Both filled! " + openNewPairOrderEnabled);
									}



									// Move sell order (Amend)
									if (order[sellOrderId].Status == "New")
									{
										if ((sellLimitPrice - limitPriceShift) == (Double)TD[0]["asks"][0][0])
										{
											//Console.WriteLine("SELL Order has not moved");
										}
										else
										{
											// Rate limit
											if (DateTime.Compare(DateTime.Now, rateLimitDate) > 0)
											{
												rateLimitDate = DateTime.Now.AddMilliseconds(apiRequestDelay); 
												sellLimitPrice = (Double)TD[0]["asks"][0][0] + limitPriceShift;

												string response = form.bitmex.AmendOrder(sellOrderId, sellLimitPrice);
												//Console.WriteLine("-------------------------- SELL order has moved! Amend it!: " + response);
											}
										}	
									}


									// Move buy order (Amend)
									if (order[sellOrderId].Status == "New")
									{
										if ((buyLimitPrice + limitPriceShift) == (Double)TD[0]["bids"][0][0])
										{
											//Console.WriteLine("BUY Order has not moved");
										}
										else
										{
											// Rate limit
											if (DateTime.Compare(DateTime.Now, rateLimitDate) > 0)
											{
												rateLimitDate = DateTime.Now.AddMilliseconds(apiRequestDelay);
												buyLimitPrice = (Double)TD[0]["bids"][0][0] - limitPriceShift;

												string response = form.bitmex.AmendOrder(buyOrderId, buyLimitPrice);
												//Console.WriteLine("-------------------------- SELL order has moved! Amend it!: " + response);
											}
										}
									}




								}
							}
						}

						// When order statuses are received. Filled, amended, cancel etc.
						else if ((string)Message["table"] == "order")
						{

							//MessageBox.Show("ddd");
							Console.WriteLine("############" + Message);

							if (Message.ContainsKey("data"))
							{
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{
				
									// If the key is found - update its value. If not - the order has just been placed, add this value to the dictionary
									if (order.ContainsKey((string)TD[0]["orderID"]))
									{
										// In some cases an orderStatus can be empty
										if (TD[0]["ordStatus"] != null)
										{
											// Update existing status
											order[(string)TD[0]["orderID"]].Status = (string)TD[0]["ordStatus"];
											Console.WriteLine("Line 184. Trade.cs. Order: " + order[(string)TD[0]["orderID"]].Id + " Direction: " + order[(string)TD[0]["orderID"]].Direction + " Status: " + order[(string)TD[0]["orderID"]].Status);

											// ADD BLUE PRINT to db only on Update order??
											// Now it seems like it is added on all events: update, filled, canceled. CHECK IT! 
											// When order is added:
											// "table": "order",
											// "action": "insert",
											// "data": [




											// Extract client order ID as a suffix. Get last 4 digits out of the string
											var clOrdID = TD[0]["clOrdID"].ToString().Substring(TD[0]["clOrdID"].ToString().Length - 4);
											// Add client order id to the DB as a blueprint
											form.database.InsertTradeRow(clOrdID);

											// Add a record to the DB
											// ..

											
											

										}
										else
										{
											// Delete?
											Console.WriteLine("Null status value while updating orderId in dictionary. Trade.cs line 192");
										}
									}
									else
									{
										// delete this closure
									}


								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Trade.cs line 204. WS Message parse error: " + ex.Message);
				}

			};
		}
	}
}

// 1. Place two orders at the sam time
// 2. Place new pair of orders only when both are filled
// 3. 