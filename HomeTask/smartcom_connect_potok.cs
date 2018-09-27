using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

// DELETE THIS CLASS!!!
// OLD ONE. ONLY FOR TESTING THE OLD CODE

namespace BitMEXAssistant
{
	public class smartcom_connect_potok
	{

		// определим переменные
		public Thread connect_potok; // переменная потока, в которй смартком будет подключаться к серверу котировок
		private Form1 form1_root; // объявим переменную в которой будем хранить ссылку form на основной класс.

		double que_length = 500; // размер массива-буфера под очередь тиков
		public bool smartcom_potok_running = true; // переменная для цикла while в потоке. если тру - то цикл выполняется.
		public int tick_count = 0; // для хранения кол-ва сделок 
		public int tick_count_second = 1; // для хранения кол-ва сделок за секунду
		DateTime time = System.DateTime.Now; // переменная для времени. нужна для подсчета кол-ва тиков в секунду

		public List<double> tick_que; // коллекция для очереди тиков. данный массив будем читаться graph_class
		public List<double> volume_que; // коллекция для объемов.
		public List<string> direction_que; // коллеция для направлений сделок
		public List<double> ask_line_que; // коллекция для хранения линии бида
		public List<double> bid_line_que; // и аска

		// для стакана и маркет дельты
		public bool quote_received = true; // флаг того, что котировка пришла и нужно запустить поток. сделать это только один раз при пришедшей котировке. это нужно для получения планок цены иначе поток запустится, а планок - нет

		private JArray Asks; // Json array for asks values
		private JArray Bids;

		// CLass constructor
		public smartcom_connect_potok(Form1 form) {

			form1_root = form; // положили в form1_root ссылку this, на основную форму

			// поток
			connect_potok = new Thread(new ThreadStart(smartcom_thread_start)); // создание экземпляра объектра поток с указанием метода, который будет выполняться при запуске потока
			connect_potok.IsBackground = true; // поток бакграундный
			connect_potok.Name = "smartcom_connect_potok";
			connect_potok.Start(); // запуск потока

		}

		public void smartcom_thread_start() // метод, который выполняется при запуске потока
		{

			try
			{
				// переменные и события смарткома
				/*
				smartcom.AddTick += new _IStClient_AddTickEventHandler(smartcom_AddTick); // назначили обработчик события прошла сделка по инструменту
				smartcom.UpdateQuote += new _IStClient_UpdateQuoteEventHandler(smartcom_UpdateQuote);
				smartcom.UpdateBidAsk += new _IStClient_UpdateBidAskEventHandler(smartcom_UpdateBidAsk);
				*/

				// массив-буфер для тиков. далее будем читать его в graph_class
				tick_que = new List<double>(); // создали экземпляр коллекции очереди 
				volume_que = new List<double>(); // коллекция объемом сделок
				direction_que = new List<string>(); // направления сделок
				ask_line_que = new List<double>(); // коллекция линии аска
				bid_line_que = new List<double>(); // коллекция линии бидп

				form1_root.ws.OnMessage += (sender, e) =>
				{

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
										decimal Price = (decimal)TD.Children().LastOrDefault()["price"];
										string Symbol = (string)TD.Children().LastOrDefault()["symbol"];

										// Call smartcom_AddTick here
										smartcom_AddTick("BTCXXX", DateTime.Now, (double)Price, 100, "445566", "BUY"); 

										//Prices[Symbol] = Price;

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
										*/

										Asks = (JArray)TD[0]["asks"];
										Bids = (JArray)TD[0]["bids"];


										/*
										if (TDAsks.Any())
										{
											List<OrderBook> OB = new List<OrderBook>();
											foreach (JArray i in TDAsks)
											{
												OrderBook OBI = new OrderBook();
					 +6. `							OBI.Price = (decimal)i[0];
												OBI.Size = (int)i[1];
												OB.Add(OBI);
											}

											OrderBookTopAsks = OB;
										}
										*/
									}
								}
								//Console.WriteLine("line 145 asks: " + Asks);
								//Console.WriteLine("line 145 asks: " + Bids);


								
								for (int i = 0; i < Asks.Count; i++) {
									Console.WriteLine("smartcom_connect_potok.cs line 153 ccc: " + Asks[i][0] + " " + i);
									//form1_root.stakan.update_stakan(row, bid, bidsize, ask, asksize); // заполнение матрицы стакана. без буфера.
									this.form1_root.stakan.update_stakan(i, (double)Asks[i][0], (double)Asks[i][1], (double)Bids[i][0], (double)Bids[i][1]);

								}

								//MessageBox.Show("d" + Asks.Count);
							}
						}
						else if (Message.ContainsKey("info") && Message.ContainsKey("docs"))
						{
							string WebSocketInfo = "Websocket Info: " + Message["info"].ToString() + " " + Message["docs"].ToString();
							//UpdateWebSocketInfo(WebSocketInfo);
							MessageBox.Show(WebSocketInfo);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show("Error im WS message. smartcom_connect_potok.cs line 76. " + ex);
					}

				};




					// массив-буфер для тиков. далее будем читать его в graph_class
					tick_que = new List<double>(); // создали экземпляр коллекции очереди 
				volume_que = new List<double>(); // коллекция объемом сделок
				direction_que = new List<string>(); // направления сделок
				ask_line_que = new List<double>(); // коллекция линии аска
				bid_line_que = new List<double>(); // коллекция линии бидп

			}
			catch (Exception Error)
			{
				//form1_root.logging.log_add(form1_root, "system", "smartcom_thread_start()", "не удалось создать smartcom. установлен ли он?!," + Error.Message, 4);
				MessageBox.Show("не удалось создать smartcom. установлен ли он?!," + Error.Message);
			}
		}


		//void smartcom_AddTick(string symbol, System.DateTime datetime, double price, double volume, string tradeno, SmartCOM4Lib.StOrder_Action action)
		void smartcom_AddTick(string symbol, System.DateTime datetime, double price, double volume, string tradeno, string action)
		{
			/*
            // проверка тиков. выводим в лог. добавил, когда отлавливал неправильное отображение кружочков
            
            if (action == StOrder_Action.StOrder_Action_Buy) // если лонг
            {
                logging.log_add(form1_root, "filter1", "que_read", "buy: " + price + " " + volume, 2);
            }
            else
            {
                logging.log_add(form1_root, "filter1", "que_read", "sell: " + price + " " + volume, 4); // если шорт
            }
            
            */


			// заполняем очередь тиков + параллельно коллекцию объемов и направлений сделок. эти три коллекции - идут параллельно
			if (tick_que.Count <= que_length) // кол-во элементов в очереди. добавляем элементы в массивы, до тех пор пока длина массива не станет равна значению длины очереди. потом очередь очищаем и нчинаем за ново
			{
				tick_que.Add(price); // добавли элемент в колекцию тиков
				volume_que.Add(volume);
				direction_que.Add(action);
			}
			else
			{
				tick_que.Clear(); volume_que.Clear(); direction_que.Clear();// удалить все элементы из коллекции

				//form1_root.stakan.que_elements_procceded = 1; // обнулим счетчик обработанных элементов очереди
				form1_root.stakan.que_elements_procceded = 0;
				tick_que.Add(price); volume_que.Add(volume); direction_que.Add(action); // и опять добавим элементы
			}






			// считаем кол-во тиков за 1сек
			if (DateTime.Compare(System.DateTime.Now, time.AddSeconds(1)) < 0) // time раньше time + 1 секунда
			{
				tick_count_second++;
			}
			else // прошла секунда
			{
				tick_count_second = 1;
				time = System.DateTime.Now;
			}

			//MessageBox.Show(DateTime.Now.ToShortDateString().Equals("30.11.2012").ToString());

		} //smartcom_AddTick



		void smartcom_UpdateBidAsk(string symbol, int row, int nrows, double bid, double bidsize, double ask, double asksize)
		{
			form1_root.stakan.update_stakan(row, bid, bidsize, ask, asksize); // заполнение матрицы стакана. без буфера.

			// заполнения массивов для линии бида и аска. с буфером
			if (row == 0) // возьмем первую строчку бида и аска стакана
			{
				// заполняем очередь линии бида и аска
				if (ask_line_que.Count <= que_length) // кол-во элементов в очереди
				{
					ask_line_que.Add(ask); bid_line_que.Add(bid);
				}
				else
				{
					ask_line_que.Clear(); bid_line_que.Clear();// удалить все элементы из коллекциq

					form1_root.stakan.que_bidask_elements_procceded = 1; // обнулим счетчик обработанных элементов очереди
					ask_line_que.Add(ask); bid_line_que.Add(bid);
				}

			}// if (row == 0)


		} // smartcom_UpdateBidAsk


		public void smartcom_kill()
		{
			
		} 


		// Enum description taken from Smartcom lib. DELETE it later! Only for testing
		public enum StOrder_Action
		{
			StOrder_Action_Buy = 1,
			StOrder_Action_Sell = 2,
			StOrder_Action_Short = 3,
			StOrder_Action_Cover = 4
		}

	}
}
