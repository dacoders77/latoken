using System.Collections.ObjectModel;
using System.Linq;
using BitMEX;

namespace BitMEXAssistant
{
	/* Websocket AUT and connection open class
	 * Subscription and parsing is located in HitBtcRealtimeDataService.cs
	 */

	public class HitBtcDataService : IDataService
	{
		public ReadOnlyCollection<Instrument> Instruments { get; }
		public Instrument ActiveInstrument { get; set; } // Get the list of all available symbols
		public IWebSocket WebSocket { get; }
		public BitMEXApi Api { get; }


		public HitBtcDataService(TradinServer tradingServer)
		{

			if (tradingServer == TradinServer.Real)
			{
				WebSocket = new WebSocketWrapper("wss://api.hitbtc.com/api/2/ws");
			}
			else
			{
				// There is no DEMO at HitBtc
			}

			WebSocket.Connect();

			if (tradingServer == TradinServer.Real)
			{
				// Auth
				WebSocket.Send("{\"method\": \"login\",\"params\": {\"algo\": \"BASIC\",\"pKey\": \"" + Settings.pKey + "\",\"sKey\": \"" + Settings.sKey + "\"}}");
			}
			else
			{
				// There is no DEMO at HitBtc
			}

		}
	}
}
