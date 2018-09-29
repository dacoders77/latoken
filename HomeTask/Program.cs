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
		private static Trade _trade;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			_bitmexDataService = new BitmexDataService(TradinServer.Demo); // Real or demo server

			
            _mainController = new MainController(new BitmexRealtimeDataService(_bitmexDataService), _mainForm = new Form1());

			// DELETE! WS messages listening must realized through a method in NEW Base class. I this class all events will be located
			// Then WS events will invoke public methods of Trade.cs class
			// BitmexRealtimeDataServices and HITBTC exchange class will inherite these methods from newly created BASE class with events in it.
			// Events located in BitmexRealtimeDataServices line 117
			_trade = new Trade(_bitmexDataService, _bitmexDataService.Api);
			_trade.placeLimitOrder();

			Application.Run(_mainForm);
		}
	}
}
