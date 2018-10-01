using System;
using MySql.Data.MySqlClient;


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
		private readonly string _connectionString;

		public DataBase()
        {
			_connectionString = $@"server={Settings.dbHost};user id=slinger;password=659111;database=home_task";
		}

		// Before trades will be add an empty record is created and Client order id is inserted. Later all 4 orders will use this is as a key
		public void InsertTradeRow(string clOrdId) 
        {
			using (var conn = new MySqlConnection(_connectionString))
			{
			    if (conn.State == System.Data.ConnectionState.Closed)
			        conn.Open();

			    var idFound = false;
			    var sqlSelect = @"SELECT * FROM `trades`";
			    using (var cmd = new MySqlCommand(sqlSelect, conn))
			    using (var reader = cmd.ExecuteReader()) 
                {
			        while (reader.Read()) 
                    {
                        if (reader[@"cl_order_id"].ToString() != clOrdId)
                            continue;

                        idFound = true;
                        break;
                    }
			    }

			    if (idFound)
			        Console.WriteLine(@"DataBase.cs line 68. Found");
			    else 
                {
			        var sqlInsert = $@"INSERT INTO `trades` (exchange, rebate_prc, symbol, volume, cl_order_id) VALUES ('{"bitmex/hitbtc"}', '{0.025}', '{"XBTUSD"}', '{1}', '{clOrdId}')";
                    using (var cmd = new MySqlCommand(sqlInsert, conn))
                        cmd.ExecuteNonQuery();
                }

			    conn.Close();
			}
		}

		public void UpdateRecord(string clOrderId, string orderId, TradeDirection direction, double price) 
        {
			using (var conn = new MySqlConnection(_connectionString))
			{
			    if (conn.State == System.Data.ConnectionState.Closed)
			        conn.Open();

			    var orderIdColumn = direction == TradeDirection.Buy ? @"buy_order_id" : @"sell_order_id";
                var sql = $@"UPDATE `trades` SET {orderIdColumn} = '{orderId}', buy_price = {price} where cl_order_id = {clOrderId}";
			    using (var cmd = new MySqlCommand(sql, conn))
			        cmd.ExecuteNonQuery();

			    conn.Close();
			}
		}

		public void BitMexProfitCalculate(string clOrderId) 
        {
			// Profit
			// Read sell price
			// Read buy price
			// Result * 2 - write in profit 

			// Rebate 
			// rebate_total = buy_price * 0.025 / 100

			// profit_total = profit + rebate_total

			using (var conn = new MySqlConnection(_connectionString))
			{
			    if (conn.State == System.Data.ConnectionState.Closed)
			        conn.Open();

			    decimal sellPrice;
			    var sqlSelectSell = $@"SELECT sell_price FROM trades WHERE cl_order_id = {clOrderId}";
			    using (var cmd = new MySqlCommand(sqlSelectSell, conn)) 
                {
				    var scalarSellPrice = cmd.ExecuteScalar();
			        sellPrice = scalarSellPrice == null || scalarSellPrice is DBNull ? 0 : Convert.ToDecimal(scalarSellPrice);
				}

			    decimal buyPrice;
			    var sqlSelectBuy = $@"SELECT buy_price FROM trades WHERE cl_order_id = {clOrderId}";
			    using (var cmd = new MySqlCommand(sqlSelectBuy, conn))
				{
					var scalarBuyPrice = cmd.ExecuteScalar();
				    buyPrice = scalarBuyPrice == null || scalarBuyPrice is DBNull ? 0 : Convert.ToDecimal(scalarBuyPrice);
				}

			    decimal profit = 0;
			    decimal rebate = 0;
			    decimal profitTotal = 0;
                if (sellPrice != 0 && buyPrice != 0) {
			        profit = sellPrice - buyPrice;
			        rebate = sellPrice * 0.025M / 100 + buyPrice * 0.025M / 100; // Volume is not calculated!
			        profitTotal = profit + rebate;
			    } 

                var sqlUpdate = $@"UPDATE `trades` SET profit = '{profit}', rebate_total = '{rebate}', profit_total = '{profitTotal}' where cl_order_id = {clOrderId}";
			    using (var cmd = new MySqlCommand(sqlUpdate, conn))
			        cmd.ExecuteNonQuery();

			    conn.Close();
			}
		}
	}
}
