using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMEX;

namespace BitMEXAssistant
{
	static class Program
	{
	    private static MainController _mainController;
	    private static Form1 _mainForm;

		private static BitmexDataService _bitmexDataService;
		private static BitmexRealtimeDataService _bitmexRealtimeDataService;
		private static DataBase _dataBase;

		//private static TradeBitMex _trade;
		//private static TradeBitMex2 _trade2; // New trading class. Everything will be moved there from TradeBitMex

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			_dataBase = new DataBase();
			_bitmexDataService = new BitmexDataService(TradinServer.Demo, _dataBase); // Real or demo server
			_bitmexRealtimeDataService = new BitmexRealtimeDataService(_bitmexDataService, "XBTUSD"); // + Trading symbol ETHUSD

			// Hit btc connection shall be performed like that:
			//_hitbtcDataService = new HitbtcDataService(TradinServer.Demo, _dataBase); 
			//_hitbtcRealtimeDataService = new HitbtcRealtimeDataService(_hitbtcDataService, "ETHUSD"); 


			//_trade2 = new TradeBitMex2(_bitmexRealtimeDataService); // BitMex

			_mainController = new MainController(_bitmexRealtimeDataService, _mainForm = new Form1(_dataBase));

			// DELETE! WS messages listening must realized through a method in NEW Base class. I this class all events will be located
			// Then WS events will invoke public methods of Trade.cs class
			// BitmexRealtimeDataServices and HITBTC exchange class will inherite these methods from newly created BASE class with events in it.
			// Events located in BitmexRealtimeDataServices line 117

			//_trade = new TradeBitMex(_bitmexDataService, _bitmexDataService.Api);
			//_trade.placeLimitOrder();

			

			Application.Run(_mainForm);
		}
	}
}
