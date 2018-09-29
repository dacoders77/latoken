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

		public DataBase() {

			connectionString = "server=" + Settings.dbHost + ";user id=slinger;password=659111;database=home_task";
			dbConn = new MySqlConnection(connectionString);
			Console.WriteLine(connectionString);
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
					Console.WriteLine("Found");
				else
				{

					if (conn.State == System.Data.ConnectionState.Closed)
					{
						conn.Open();
					}

					//string sql = "INSERT INTO trades(date) VALUES ('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "' )";
					// SELECT * FROM `trades` WHERE exchange = "777"

					sql = "INSERT INTO trades (cl_order_id) VALUES ('" + clOrdID + "' )";

					cmd = new MySqlCommand(sql, conn);
					cmd.ExecuteNonQuery();

				}
					

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
