using System.Collections.ObjectModel;
using System.Linq;
using BitMEX;

namespace BitMEXAssistant {
    public class BitmexDataService {
        public ReadOnlyCollection<Instrument> Instruments { get; }
        public Instrument ActiveInstrument { get; set; } // Get the list of all available symbols
        public IWebSocket WebSocket { get; }
        public BitMEXApi Api { get; }

        public BitmexDataService() {
			//WebSocket = new WebSocketWrapper("wss://testnet.bitmex.com/realtime"); // wss://www.bitmex.com/realtime
			WebSocket = new WebSocketWrapper("wss://www.bitmex.com/realtime");

			Api = new BitMEXApi(Settings.bitmexApiKey, Settings.bitmexApiSecret, false);

			Instruments = Api.GetActiveInstruments().OrderByDescending(a => a.Volume24H).ToList().AsReadOnly();
            ActiveInstrument = Instruments[0];

            WebSocket.Connect();

            // Authenticate the API
            var apiExpires = Api.GetExpiresArg();
            var signature = Api.GetWebSocketSignatureString(Settings.bitmexApiSecret, apiExpires);

            WebSocket.Send($@"{{""op"": ""authKeyExpires"", ""args"": [""{Settings.bitmexApiKey}"", {apiExpires}, ""{signature}""]}}");
        }
    }
}