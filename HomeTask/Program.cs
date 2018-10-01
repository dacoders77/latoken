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
		private static HitBtcDataService _hitBtcDataService; // Hedge exchange
		private static HitBtcRealtimeDataService _hitBtcRealtimeDataService; // Realtime HitBtc websicket subscription and events listening

		private static DataBase _dataBase;
	    private static TradeBitMex2 _tradeBitMex2;
		private static TradeBitMex2 _tradeBitMex3; // Works good

		

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			_dataBase = new DataBase();

			_bitmexDataService = new BitmexDataService(TradinServer.Real, _dataBase); // Real or demo server
			_bitmexRealtimeDataService = new BitmexRealtimeDataService(_bitmexDataService, "ETHUSD"); // + Trading symbol ETHUSD / XBTUSD

			_hitBtcDataService = new HitBtcDataService(TradinServer.Real); // DB instance is not passed as the parameter. The only DB is used in BitmexDataService 
			_hitBtcRealtimeDataService = new HitBtcRealtimeDataService(_hitBtcDataService, _dataBase, "ETHUSD"); // We send database class instance in order to update DB records when hedge market orders are filled in such events are triggered in HitBtc websocket event listener


			// Hit btc connection shall be performed like that:
			//_hitbtcDataService = new HitbtcDataService(TradinServer.Demo, _dataBase); 
			//_hitbtcRealtimeDataService = new HitbtcRealtimeDataService(_hitbtcDataService, "ETHUSD"); 

			// // Create the reading class + set up order book limit orders shift 
			// XBTUSD: 0, 0.5. 1 ..
			// ETHUSD: 0.05, 0,1, 0,15
			_tradeBitMex2 = new TradeBitMex2(_bitmexRealtimeDataService, _hitBtcRealtimeDataService, _bitmexDataService, 0.05); // ETHUSD 0.05 Dom price offset. 0 - at the best bid/ask
		    //_tradeBitMex3 = new TradeBitMex2(_bitmexRealtimeDataService, _hitBtcRealtimeDataService, _bitmexDataService, 0.1); // Works good

		    _mainController = new MainController(_bitmexRealtimeDataService, _hitBtcRealtimeDataService, _mainForm = new Form1(_dataBase), new[] { _tradeBitMex2 });

			// DELETE! WS messages listening must realized through a method in NEW Base class. In this class all events will be located
			// Then WS events will invoke public methods of Trade.cs class
			// BitmexRealtimeDataServices and HITBTC exchange class will inherite these methods from newly created BASE class with events in it.
			// Events located in BitmexRealtimeDataServices line 117

			//_trade = new TradeBitMex(_bitmexDataService, _bitmexDataService.Api);
			//_trade.placeLimitOrder();

			

			Application.Run(_mainForm);
		}
	}
}
