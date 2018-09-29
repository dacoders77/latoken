using System.Collections.ObjectModel;
using System.Linq;
using BitMEX;

namespace BitMEXAssistant {
	public class BitmexDataService {

		public ReadOnlyCollection<Instrument> Instruments { get; }
		public Instrument ActiveInstrument { get; set; } // Get the list of all available symbols
		public IWebSocket WebSocket { get; }
		public BitMEXApi Api { get; }

		public BitmexDataService(TradinServer tradingServer) {

			if (tradingServer == TradinServer.Real)
			{
				WebSocket = new WebSocketWrapper("wss://www.bitmex.com/realtime");
				Api = new BitMEXApi(Settings.bitmexApiKey, Settings.bitmexApiSecret, true); // false - demo, true - real account
			}
			else {
				WebSocket = new WebSocketWrapper("wss://testnet.bitmex.com/realtime"); 
				Api = new BitMEXApi(Settings.bitmexDemoApiKey, Settings.bitmexDemoApiSecret, false);
				System.Windows.Forms.MessageBox.Show(Settings.bitmexDemoApiKey + " / " + Settings.bitmexDemoApiSecret);
			}


			
			Instruments = Api.GetActiveInstruments().OrderByDescending(a => a.Volume24H).ToList().AsReadOnly();
			ActiveInstrument = Instruments[0];

			WebSocket.Connect();

			// Authenticate the API
			var apiExpires = Api.GetExpiresArg();

			if (tradingServer == TradinServer.Real)
			{
				var signature = Api.GetWebSocketSignatureString(Settings.bitmexApiSecret, apiExpires);
				WebSocket.Send($@"{{""op"": ""authKeyExpires"", ""args"": [""{Settings.bitmexApiKey}"", {apiExpires}, ""{signature}""]}}");
			}
			else
			{
				var signature = Api.GetWebSocketSignatureString(Settings.bitmexDemoApiSecret, apiExpires);
				WebSocket.Send($@"{{""op"": ""authKeyExpires"", ""args"": [""{Settings.bitmexDemoApiKey}"", {apiExpires}, ""{signature}""]}}");
			}
				

			
		}



    }
}