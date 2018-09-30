using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

using BitMEX; // Api class namespace

namespace BitMEXAssistant
{
	public partial class Form1 : Form, IMainView 
    {

		// DOM
		bool quote_received = true; 

		// Trade. Symbol leg 1. Bitmex exchange
		private Trade trade;

		public string symbol = "ETHUSD"; // ETHUSD
		//public string clientOrderId = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString() + "bbb"; // Client order id. This id comes back when an order is executed. Used for linking orders together

		// DB
		private DataBase _database;

        private decimal _balance;
        private OrderBookDataSet _orderBookDataSet;

       

        public Form1(DataBase dataBase) 
        {
			InitializeComponent();
			_database = dataBase;

			// DOM

			panel_big.AutoScroll = true;

            dataGridView1.ColumnAdded += (sender, args) => {
                args.Column.Width = 50;
            };
        }

		private void Form1_Load(object sender, EventArgs e)
		{
			// High and low price values for DOM render
		    orderBookControl.PriceStart = 6300; // ETHUSD 200-250. XBTCUSD
		    orderBookControl.PriceEnd = 6900;
		    orderBookControl.PriceStep = new decimal(0.5); // XBTCUSD: 0.5, ETHUSD: 0.05
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

        private bool _scrollNeeded = true;
        private ReadOnlyCollection<Order> _orders;

        private void OnOrderBookDataSetChanged() {
            orderBookControl.DataSet = _orderBookDataSet;
            if (_scrollNeeded && _orderBookDataSet != null) {
                ScrollToPrice(orderBookControl.DataSet.Ask[0].Price, panel_big);
                _scrollNeeded = false;
            }
        }

        private void OnOrdersChanged(IReadOnlyCollection<Order> orders) {
            dataGridView1.DataSource = orders;

            orderBookControl.ActiveOrders = orders.Where(o => o.Status == "New").ToList();
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

        public ReadOnlyCollection<Order> Orders {
            get { return _orders; }
            set {
                _orders = value;

                OnOrdersChanged(_orders);
            }
        }

       

        // TEST. DELETE
        private void button2_Click(object sender, EventArgs e)
		{
			// Testing the record. DELETE 
			_database.BitMexProfitCalculate("5604");
		}

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            orderBookControl.SoundEnabled = checkBox1.Checked;
            orderBookControl1.SoundEnabled = checkBox1.Checked;
        }
    }
}
