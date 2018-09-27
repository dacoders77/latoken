using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WebSocketSharp;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Reflection; // установление флагов для панелей, что бы не моргало
using System.Media; // для проигрывания wav'ов

using BitMEX; // Api class namespace

namespace BitMEXAssistant
{
	public partial class Form1 : Form
	{

		string APIKey = Settings.bitmexApiKey;
		string APISecret = Settings.bitmexApiSecret;

		public BitMEXApi bitmex;
		public WebSocket ws;
		DateTime WebScocketLastMessage = new DateTime();

		Dictionary<string, decimal> Prices = new Dictionary<string, decimal>();
		List<OrderBook> OrderBookTopAsks = new List<OrderBook>();
		List<OrderBook> OrderBookTopBids = new List<OrderBook>();

		List<Instrument> ActiveInstruments = new List<Instrument>();
		Instrument ActiveInstrument = new Instrument();

		Position SymbolPosition = new Position();
		decimal Balance = 0;

		// DOM
		public Panel panel_small = new Panel(); 
		public Panel panel_market_delta = new Panel(); 
		bool quote_received = true; 

		public double market_delta_vol; 
		public double market_delta_stop; 
		double market_delta_stop_step = 100; 

		private JArray Asks; // Json array for asks values
		private JArray Bids;

		// Trade. Symbol leg 1. Bitmex exchange
		private Trade trade;

		// Hedge. Symbol leg 2. HitBtc exchange

		public string symbol = "ETHUSD"; // ETHUSD
		//public string clientOrderId = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "bbb"; // Client order id. This id comes back when an order is executed. Used for linking orders together

		// DB
		public DataBase database;

		public Form1()
		{
			InitializeComponent();

			// DOM
			// таймер для скрола панелей, когда график уезжает за экран
			System.Windows.Forms.Timer scroll_timer = new System.Windows.Forms.Timer();
			scroll_timer.Tick += new EventHandler(scroll_timer_Tick); // связали событие таймера
			scroll_timer.Interval = 200; // интервал таймера
			scroll_timer.Enabled = true;

			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(panel_small, true, null); // зададим свойства что бы не моргало для панели
			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(panel_market_delta, true, null); // зададим свойства что бы не моргало для панели market_delta
																																											   // DOM
			panel_big.Controls.Add(panel_small); 
			panel_small.Width = panel_big.Width; 
			panel_small.Height = panel_big.Height;
			panel_small.Location = new Point(0, 0); 
			panel_big.AutoScroll = true; 
			panel_big2.HorizontalScroll.Visible = true;

			
			panel_big2.Controls.Add(panel_market_delta);
			panel_market_delta.Width = panel_big.Width;

			//panel_market_delta.Height = panel_small.;
			panel_small.Location = new Point(0, 0);
			panel_big2.AutoScroll = true;

			panel_big2.Scroll += new ScrollEventHandler(panel_big2_Scroll); 

			// Trade
			trade = new Trade(this);

			// DB
			database = new DataBase(this);

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			bitmex = new BitMEXApi(APIKey, APISecret, false); // Real network - true, Test - false
			InitializeSymbolInformation(); // Prepare prices dictionary
			
			// Websocket class instance
			ws = new WebSocket("wss://testnet.bitmex.com/realtime"); // wss://www.bitmex.com/realtime wss://testnet.bitmex.com/realtime

			InitializeWebSocket(); // WebSocket warm-up
			InitializeDependentSymbolInformation();

			// DOM
			this.Text = "Latoken DOM";

			market_delta_vol = panel_market_delta.Height / 2;
			market_delta_stop = panel_market_delta.Height / 2;

			int high_limit = 7000;
			int low_limit = 6000;


			// Trade
			trade.placeLimitOrder();

		}

		private void InitializeWebSocket()
		{

			//ws = new WebSocket("wss://www.bitmex.com/realtime"); // wss://www.bitmex.com/realtime wss://testnet.bitmex.com/realtime
			
			ws.OnMessage += (sender, e) =>
			{
				// WS Stream goes here

				WebScocketLastMessage = DateTime.UtcNow;
				try
				{
					JObject Message = JObject.Parse(e.Data);

					// *********
					//Console.WriteLine(Message);
					//MessageBox.Show(Message.ToString());

					if (Message.ContainsKey("table"))
					{
						if ((string)Message["table"] == "trade")
						{
							if (Message.ContainsKey("data"))
							{ 
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{ 
									double Price = (double)TD.Children().LastOrDefault()["price"];
									string Symbol = (string)TD.Children().LastOrDefault()["symbol"];
									//Prices[Symbol] = Price;

									//Console.WriteLine("Form1.cs line 106. trade. Symbol: " + Symbol + " Price: " + Price + " Vol: " + TD.Children().LastOrDefault()["size"] + " Side: " + TD.Children().LastOrDefault()["side"]);

									// This throws: Exception thrown: 'System.NullReferenceException' in BitMEXAssistant.exe
									//zz.add_tick(Symbol, DateTime.Now, Price, (double)TD.Children().LastOrDefault()["size"], "555666", TD.Children().LastOrDefault()["side"].ToString().ToLower());

									SoundPlayer simpleSound = new SoundPlayer(@"C:\tick_2.wav");
									//simpleSound.Play();

									// DELETE Necessary for trailing stops
									//UpdateTrailingStopData(ActiveInstrument.Symbol, Prices[ActiveInstrument.Symbol]);
									//if (SymbolPosition.Symbol == Symbol && SymbolPosition.CurrentQty != 0 && chkTrailingStopEnabled.Checked)
									//{
									//	ProcessTrailingStop(Symbol, Price);
									//}
								}
							}
						}
						else if ((string)Message["table"] == "orderBook10")
						{
							if (Message.ContainsKey("data"))
							{
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{
									JArray TDBids = (JArray)TD[0]["bids"];
									/*
									if (TDBids.Any())
									{
										List<OrderBook> OB = new List<OrderBook>();
										foreach (JArray i in TDBids)
										{
											OrderBook OBI = new OrderBook();
											OBI.Price = (decimal)i[0];
											OBI.Size = (int)i[1];
											OB.Add(OBI);
										}

										OrderBookTopBids = OB;
									}

									JArray TDAsks = (JArray)TD[0]["asks"];
									if (TDAsks.Any())
									{
										List<OrderBook> OB = new List<OrderBook>();
										foreach (JArray i in TDAsks)
										{
											OrderBook OBI = new OrderBook();
											OBI.Price = (decimal)i[0];
											OBI.Size = (int)i[1];
											OB.Add(OBI);
										}

										OrderBookTopAsks = OB;
									}
									*/

									Asks = (JArray)TD[0]["asks"];
									Bids = (JArray)TD[0]["bids"];

									for (int i = 0; i < Asks.Count; i++)
									{
										//Console.WriteLine("smartcom_connect_potok.cs line 153 ccc: " + Asks[i][0] + " " + i);
										                    // dont need this: form1_root.stakan.update_stakan(row, bid, bidsize, ask, asksize); // заполнение матрицы стакана. без буфера.

										
										//zz.update_bid_ask("USDZZZ", i, 10, (double)Asks[i][0], (double)Asks[i][1], (double)Bids[i][0], (double)Bids[i][1]);
										
									}
								}
							}
						}
						else if ((string)Message["table"] == "position")
						{
							// PARSE
							if (Message.ContainsKey("data"))
							{
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{
									if (TD.Children().LastOrDefault()["symbol"] != null)
									{
										SymbolPosition.Symbol = (string)TD.Children().LastOrDefault()["symbol"];
									}
									if (TD.Children().LastOrDefault()["currentQty"] != null)
									{
										SymbolPosition.CurrentQty = (int?)TD.Children().LastOrDefault()["currentQty"];

									}
									if (TD.Children().LastOrDefault()["avgEntryPrice"] != null)
									{
										SymbolPosition.AvgEntryPrice = (decimal?)TD.Children().LastOrDefault()["avgEntryPrice"];

									}
									if (TD.Children().LastOrDefault()["markPrice"] != null)
									{
										SymbolPosition.MarkPrice = (decimal?)TD.Children().LastOrDefault()["markPrice"];

									}
									if (TD.Children().LastOrDefault()["liquidationPrice"] != null)
									{
										SymbolPosition.LiquidationPrice = (decimal?)TD.Children().LastOrDefault()["liquidationPrice"];
									}
									if (TD.Children().LastOrDefault()["leverage"] != null)
									{
										SymbolPosition.Leverage = (decimal?)TD.Children().LastOrDefault()["leverage"];

									}
									if (TD.Children().LastOrDefault()["unrealisedPnl"] != null)
									{
										SymbolPosition.UnrealisedPnl = (decimal?)TD.Children().LastOrDefault()["unrealisedPnl"];
									}
									if (TD.Children().LastOrDefault()["unrealisedPnlPcnt"] != null)
									{
										SymbolPosition.UnrealisedPnlPcnt = (decimal?)TD.Children().LastOrDefault()["unrealisedPnlPcnt"];

									}

								}
							}
						}
						else if ((string)Message["table"] == "margin")
						{
							if (Message.ContainsKey("data"))
							{
								JArray TD = (JArray)Message["data"];
								if (TD.Any())
								{
									try
									{
										Balance = ((decimal)TD.Children().LastOrDefault()["walletBalance"] / 100000000);
										UpdateBalanceAndTime();
									}
									catch (Exception ex)
									{

									}
								}
							}
						}
					}
					else if (Message.ContainsKey("info") && Message.ContainsKey("docs"))
					{
						string WebSocketInfo = "Websocket Info: " + Message["info"].ToString() + " " + Message["docs"].ToString();
						UpdateWebSocketInfo(WebSocketInfo);
					}
				}
				catch (Exception ex)
				{
					//MessageBox.Show(ex.Message);
				}
			};
			ws.OnError += (sender, e) =>
			{
			};

			ws.Connect();

			// Authenticate the API
			string APIExpires = bitmex.GetExpiresArg();
			string Signature = bitmex.GetWebSocketSignatureString(APISecret, APIExpires);
			ws.Send("{\"op\": \"authKeyExpires\", \"args\": [\"" + APIKey + "\", " + APIExpires + ", \"" + Signature + "\"]}");

			//// Chat Connect
			//ws.Send("{\"op\": \"subscribe\", \"args\": [\"chat\"]}");

		}

		private void UpdateWebSocketInfo(string WebSocketInfo)
		{
			//lblSettingsWebsocketInfo.Invoke(new Action(() => lblSettingsWebsocketInfo.Text = WebSocketInfo));
			//MessageBox.Show("private void UpdateWebSocketInfo(string WebSocketInfo) line 210 Form1.cs " + WebSocketInfo);
		}

		private void UpdateBalanceAndTime()
		{
			int HoursInFuture = 0;
			try
			{
				string USDValue = (Prices["XBTUSD"] * Balance).ToString("C", new CultureInfo("en-US"));
				//lblBalanceAndTime.Invoke(new Action(() => lblBalanceAndTime.Text = "Balance: " + Math.Round(Balance, 8).ToString() + " | " + USDValue + "     " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.AddHours(HoursInFuture).ToLongTimeString()));

				MessageBox.Show("private void UpdateBalanceAndTime(), line 225, Form1.cs Balance: " + Math.Round(Balance, 8).ToString() + " | " + USDValue + "     " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.AddHours(HoursInFuture).ToLongTimeString());

			}
			catch (Exception ex)
			{
				//lblBalanceAndTime.Invoke(new Action(() => lblBalanceAndTime.Text = "Balance: Error     " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.AddHours(HoursInFuture).ToLongTimeString()));
				MessageBox.Show("private void UpdateBalanceAndTime(), line 231, Form1.cs Balance: Error     " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.AddHours(HoursInFuture).ToLongTimeString());
			}
		}

		private void InitializeSymbolInformation()
		{
			ActiveInstruments = bitmex.GetActiveInstruments().OrderByDescending(a => a.Volume24H).ToList();
			// Assemble our price dictionary
			foreach (Instrument i in ActiveInstruments)
			{
				Prices.Add(i.Symbol, 0); // just setting up the item, 0 is fine here.
			}
		}

		private void InitializeDependentSymbolInformation()
		{
			// Form controls setup DELETE
			//ddlSymbol.DataSource = ActiveInstruments;
			//ddlSymbol.DisplayMember = "Symbol";
			//ddlSymbol.SelectedIndex = 0;
			ActiveInstrument = ActiveInstruments[0];

			InitializeSymbolSpecificData(true); // Subscribe to WebSocket with the given information
		}

		// WEB SOCKET SUBSCRIPTION
		private void InitializeSymbolSpecificData(bool FirstLoad = false)
		{
			if (!FirstLoad)
			{
				// Unsubscribe from old orderbook
				ws.Send("{\"op\": \"unsubscribe\", \"args\": [\"orderBook10:" + symbol + "\"]}");
				OrderBookTopAsks = new List<OrderBook>();
				OrderBookTopBids = new List<OrderBook>();

				// Unsubscribe from old instrument position
				ws.Send("{\"op\": \"unsubscribe\", \"args\": [\"position:" + symbol + "\"]}");

				// Unsubscribe from orders stauses
				ws.Send("{\"op\": \"unsubscribe\", \"args\": [\"order\"]}");

				// Replace dictionary symbol pull-up with fixed values. TEST
				//ActiveInstrument = bitmex.GetInstrument(((Instrument)ddlSymbol.SelectedItem).Symbol)[0];
				ActiveInstrument.Symbol = symbol;
				ActiveInstrument.TickSize = 0.5M;
				ActiveInstrument.Volume24H = 9000; // A random test value
				
			}

			// Subscribe to order statuses
			ws.Send("{\"op\": \"subscribe\", \"args\": [\"order\"]}");

			// Subscribe to new orderbook
			ws.Send("{\"op\": \"subscribe\", \"args\": [\"orderBook10:" + symbol + "\"]}");

			// Subscribe to position for new symbol
			//ws.Send("{\"op\": \"subscribe\", \"args\": [\"position:" + ActiveInstrument.Symbol + "\"]}");

			// Only subscribing to this symbol trade feed now, was too much at once before with them all.
			ws.Send("{\"op\": \"subscribe\", \"args\": [\"trade:" + symbol + "\"]}");

			// Margin Connect - do this last so we already have the price.
			//ws.Send("{\"op\": \"subscribe\", \"args\": [\"margin\"]}");

			

			//UpdateFormsForTickSize(ActiveInstrument.TickSize, ActiveInstrument.DecimalPlacesInTickSize);

		}

		void scroll_timer_Tick(object sender, EventArgs e) // событие таймера. используем его для подгонки графиков в центр панели, если они уехали за зону видимости
		{
			// работает
			//logging.log_add(this, "scroll:", " " + panel_big2.VerticalScroll.Maximum, 1);

			if (!quote_received) // можно двигать скрол только после прихода планок цены. иначе максимум скрола по умолчания = 100.
			{
				//panel_big2.VerticalScroll.Value = panel_big2.VerticalScroll.Value + 200;
				//logging.log_add(this, "scroll:", " max задан" + panel_big2.VerticalScroll.Maximum, 1);
			}

		} // scroll_timer_Tick

		void panel_big2_Scroll(object sender, EventArgs e) // событе изменения скрола
		{
			//logging.log_add(this, "scroll:", " " + panel_big2.VerticalScroll.Value, 1);
			//textBox1.Text = panel_big2.VerticalScroll.Value.ToString(); // координата скрола

		}// panel_big2_Scroll

	}
}
