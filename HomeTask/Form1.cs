using System;
using System.Windows.Forms;

using BitMEX; // Api class namespace

namespace BitMEXAssistant
{
	public partial class Form1 : Form, IMainView 
    {

        public BitMEXApi bitmex;
        public IWebSocket ws;

		// DOM
		bool quote_received = true; 

		// Trade. Symbol leg 1. Bitmex exchange
		private Trade trade;

		// Hedge. Symbol leg 2. HitBtc exchange

		public string symbol = "ETHUSD"; // ETHUSD
		//public string clientOrderId = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "bbb"; // Client order id. This id comes back when an order is executed. Used for linking orders together

		// DB
		public DataBase database;

        private decimal _balance;
        private OrderBookDataSet _orderBookDataSet;

        public Form1() 
        {
			InitializeComponent();

			// DOM
			// таймер для скрола панелей, когда график уезжает за экран
			var scroll_timer = new Timer();
			scroll_timer.Tick += scroll_timer_Tick; // связали событие таймера
			scroll_timer.Interval = 200; // интервал таймера
			scroll_timer.Enabled = true;

			panel_big.AutoScroll = true; 
			
			// Trade
			trade = new Trade(this);

			// DB
			database = new DataBase(this);

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Trade
			//trade.placeLimitOrder();

		    orderBookControl.PriceStart = 200;
		    orderBookControl.PriceEnd = 300;
		    orderBookControl.PriceStep = new decimal(0.05);
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

        public void AddTrade(TradeData tradeData) => orderBookControl.AddTrade(tradeData);

        private void OnBalanceChanged() {
            // nop
        }

        private void OnOrderBookDataSetChanged() {
            orderBookControl.DataSet = _orderBookDataSet;
        }

        public decimal Balance {
            get { return _balance; }
            set {
                if (_balance == value)
                    return;

                _balance = value;

                OnBalanceChanged();
            }
        }

        public OrderBookDataSet OrderBookDataSet {
            get { return _orderBookDataSet; }
            set {
                if (_orderBookDataSet == value)
                    return;

                _orderBookDataSet = value;

                OnOrderBookDataSetChanged();
            }
        }
    }
}
