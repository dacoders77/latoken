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

				//string sql = "INSERT INTO trades(date) VALUES ('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' )";
				// SELECT * FROM `trades` WHERE exchange = "777"

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

					//string sql = "INSERT INTO trades(date) VALUES ('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' )";
					sql = String.Format("INSERT INTO `trades` (exchange, rebate_prc, symbol, volume, cl_order_id) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}')", "bitmex/hitbtc", 0.025, "ETHUSD", 1, clOrdID);

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
			//BitMexProfitCalculate(clOrdID);
		}

		private void BitMexProfitCalculate(string clOrdID) {

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

				if (scalarSellPrice == null)
				{
					_sellPrice = 0;
				}
				else {
					//_sellPrice = Convert.ToDouble(scalarSellPrice);
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
				if (scalarBuyPrice == null)
				{
					_buyPrice = 0;
				}
				else
				{
					//_buyPrice = Convert.ToDouble(scalarBuyPrice);
				}
				conn.Close();

				//double profit = _sellPrice - _buyPrice;
				//MessageBox.Show("DataBase.cs line 156. : " + profit);
			}

			/*
			// Get buy price
			using (var conn = new MySqlConnection(connectionString))
			{
				if (conn.State == System.Data.ConnectionState.Closed)
				{
					conn.Open();
				}
				_sql = string.Format("SELECT symbol FROM trades WHERE buy_price = {0}", clOrdID);
				MySqlCommand cmd = new MySqlCommand(_sql, conn);
				object scalarValue = cmd.ExecuteScalar();
				//MessageBox.Show("DataBase.cs line 136. : " + scalarValue);
				_buyPrice = (double)scalarValue;
				conn.Close();
			}
			*/

			

			// Get basket allocated funds. Works good
			//sqlQueryString = string.Format("SELECT allocated_funds FROM baskets WHERE id = {0}", basketId);
			//MySqlCommand cmd4 = new MySqlCommand(sqlQueryString, mySqlConnection);
			//object basketAllocatedFunds = cmd4.ExecuteScalar();
			//form.basket.UpdateInfoJson(string.Format("basketAllocatedFunds: {0}", basketAllocatedFunds), "volumeCalculate", "calc", requestId); // Update json info feild in DB


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
