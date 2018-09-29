using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMEXAssistant
{
	/* Order information class. Contains necessary order data model fields like: 
	* - OrderId
	* - Order status (New, FIlled, Canceled etc.)
	* - Order direction etc.
	* - Used for execution response parsing. When an order is executed and response received there is no information about its direction and so on. 
	* Direction is needed for storing information in DB and profit calculation as well as hedge orders opening. 
	 */
	class Order
	{
		public string Id { get; set; }
		public string Status { get; set; }
		public TradeDirection Direction { get; set; }

		public Order(string id, string status, TradeDirection direction)
		{
			Id = id;
			Status = status;
			Direction = direction;
		}
	}



}
