using System;
using System.Windows.Forms;

using BitMEX; // Api class namespace

namespace BitMEXAssistant
{
	public partial class Form1 : Form, IMainView 
    {

        public BitMEXApi bitmex;
        public IWebSocket ws;

		// Trade. Symbol leg 1. Bitmex exchange
		private Trade trade;

		// Hedge. Symbol leg 2. HitBtc exchange

		public string symbol = "ETHUSD"; // ETHUSD
		//public string clientOrderId = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "bbb"; // Client order id. This id comes back when an order is executed. Used for linking orders together

		// DB
		public DataBase database;

        private decimal _balance;
        private OrderBookDataSet _orderBookDataSet;
        private readonly Timer _scrollTimer = new Timer();

        public Form1() 
        {
			InitializeComponent();

			// DOM
			// таймер для скрола панелей, когда график уезжает за экран
            _scrollTimer.Tick += ScrollTimerOnTick; // связали событие таймера
			_scrollTimer.Interval = 2000; // интервал таймера
            _scrollTimer.Start();

			// Trade
			trade = new Trade(this);

			// DB
			database = new DataBase(this);

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Trade
			//trade.placeLimitOrder();

			// High and low price values for DOM render
		    orderBookControl.PriceStart = 200;
		    orderBookControl.PriceEnd = 250;
		    orderBookControl.PriceStep = new decimal(0.05);
		}

        private void ScrollTimerOnTick(object sender, EventArgs e) // событие таймера. используем его для подгонки графиков в центр панели, если они уехали за зону видимости
		{
		    if (orderBookControl.DataSet != null && orderBookControl.DataSet.Ask.Count > 0)
		        ScrollToPrice(orderBookControl.DataSet.Ask[0].Price, panel_big);
		}

        private void ScrollToPrice(decimal currentPrice, ScrollableControl panel) {
            var relativePrice = (double) (currentPrice - orderBookControl.PriceStart) /
                                (double) (orderBookControl.PriceEnd - orderBookControl.PriceStart);

            var scrollSize = panel.VerticalScroll.Maximum - panel.VerticalScroll.Minimum;

            var offset = panel.VerticalScroll.Minimum - panel_big.Height / 2;

            var verticalScrollValue = (int) (scrollSize * (1 - relativePrice) + offset);

            verticalScrollValue = Math.Max(panel.VerticalScroll.Minimum, Math.Min(panel.VerticalScroll.Maximum, verticalScrollValue));

            panel.VerticalScroll.Value = verticalScrollValue;
        }

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
