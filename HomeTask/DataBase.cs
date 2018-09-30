using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace BitMEXAssistant
{
	/* Database handle class.
	 * Adds buy, sel orders as well as hedge buy and hedge close orders to the database.
	 * Calculates profit, commission, rebate and so on.
	 * One trade has 4 orders: buy, sel and hedge buy/sell (Executed at the second echange).
	 * Client order ID is used to identify (link together) all four orders. All of them will have the same Client order ID
	 */
	public class DataBase
	{
		private Form1 form;
		private MySqlConnection dbConn; // MySql connection variable
		private string connectionString;
		private string sqlQueryString;
		private bool flag;

		private string _sql;
		private double _sellPrice;
		private double _buyPrice;
		private double _profit;
		private double _rebate;
		private double _profitTotal;


		public DataBase() {
			connectionString = "server=" + Settings.dbHost + ";user id=slinger;password=659111;database=home_task";
			dbConn = new MySqlConnection(connectionString);
		}

		// Before trades will be add an empty record is created and Client order id is inserted. Later all 4 orders will use this is as a key
		public void InsertTradeRow(string clOrdID) {

			using (var conn = new MySqlConnection(connectionString))
			{
				
				if (conn.State == System.Data.ConnectionState.Closed)
				{
					conn.Open(); 
				}

				string sql = "SELECT * FROM `trades`";
				
				MySqlCommand cmd = new MySqlCommand(sql, conn);
				MySqlDataReader reader = cmd.ExecuteReader();
				
				while (reader.Read()) {
					if (reader["cl_order_id"].ToString() == clOrdID)
					{
						flag = true; // Id found
						break;
					}
				}

				conn.Close();


				if (flag)
					//MessageBox.Show("found!");
					Console.WriteLine("DataBase.cs line 68. Found");
				else
				{
					if (conn.State == System.Data.ConnectionState.Closed)
					{
						conn.Open();
					}

					sql = String.Format("INSERT INTO `trades` (exchange, rebate_prc, symbol, volume, cl_order_id) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", "bitmex/hitbtc", 0.025, "XBTUSD", 1, clOrdID);

					cmd = new MySqlCommand(sql, conn);
					cmd.ExecuteNonQuery();

				}
					
			}

		}

		public void UpdateRecord(string clOrdID, string orderID, TradeDirection direction, double price) {

			using (var conn = new MySqlConnection(connectionString))
			{
				if (conn.State == System.Data.ConnectionState.Closed)
				{
					conn.Open();
				}
				if (direction == TradeDirection.Buy)
				{
					_sql = string.Format("UPDATE `trades` SET buy_order_id = '{0}', buy_price = {1} where cl_order_id = {2}", orderID, price, clOrdID);
					MySqlCommand cmd = new MySqlCommand(_sql, conn);
					cmd.ExecuteNonQuery();
				}
				else
				{
					_sql = string.Format("UPDATE `trades` SET sell_order_id = '{0}', sell_price = {1} where cl_order_id = {2}", orderID, price, clOrdID);
					MySqlCommand cmd = new MySqlCommand(_sql, conn);
					cmd.ExecuteNonQuery();
				}
				conn.Close();
			}
			BitMexProfitCalculate(clOrdID);
		}

		public void BitMexProfitCalculate(string clOrdID) {

			// Profit
			// Read sell price
			// Read buy price
			// Result * 2 - write in profit 

			// Rebate 
			// rebate_total = buy_price * 0.025 / 100

			// profit_total = profit + rebate_total

			// Get sell price
			using (var conn = new MySqlConnection(connectionString))
			{

				if (conn.State == System.Data.ConnectionState.Closed)
				{
					conn.Open();
				}
				_sql = string.Format("SELECT sell_price FROM trades WHERE cl_order_id = {0}", clOrdID);
				MySqlCommand cmd = new MySqlCommand(_sql, conn);
				object scalarSellPrice = cmd.ExecuteScalar();
				MessageBox.Show("DataBase.cs line 140. : " + scalarSellPrice);

				if (scalarSellPrice.GetType() == typeof(DBNull))
				{
					_sellPrice = 0;
				}
				else {
					_sellPrice = Convert.ToDouble(scalarSellPrice);
				}

				conn.Close();


				if (conn.State == System.Data.ConnectionState.Closed)
				{
					conn.Open();
				}
				_sql = string.Format("SELECT buy_price FROM trades WHERE cl_order_id = {0}", clOrdID);
				MySqlCommand cmd2 = new MySqlCommand(_sql, conn);
				object scalarBuyPrice = cmd2.ExecuteScalar();
				MessageBox.Show("DataBase.cs line 161. : " + scalarBuyPrice);

				if (scalarBuyPrice.GetType() == typeof(DBNull))
				{
					_buyPrice = 0;
				}
				else
				{
					_buyPrice = (double)scalarBuyPrice;
				}
				

				if (_sellPrice != 0 && _buyPrice != 0)
				{
					_profit = _sellPrice - _buyPrice;
					_rebate = (_sellPrice * 0.025 / 100) + (_buyPrice * 0.025 / 100); // Volume is not calculated!
					_profitTotal = _profit + _rebate;

				}
				else {
					_profit = 0;
					_rebate = 0;
					_profitTotal = 0;
				}
				 
				//MessageBox.Show("DataBase.cs line 186. : " + _profit);

				_sql = string.Format("UPDATE `trades` SET profit = '{0}', rebate_total = '{1}', profit_total = '{2}' where cl_order_id = {3}", _profit, _rebate, _profitTotal, clOrdID);
				MySqlCommand cmd3 = new MySqlCommand(_sql, conn);
				cmd3.ExecuteNonQuery();





				conn.Close();

			}


		}




		public void AddBuyOrder() {

		}

		public void AddSellOrder()
		{

		}

		public void AddHedgeBuyOrder()
		{

		}

		public void AddHedgeSellOrder()
		{

		}
	}
}
