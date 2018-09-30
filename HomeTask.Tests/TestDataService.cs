using System.Collections.ObjectModel;
using BitMEX;
using BitMEXAssistant;

namespace HomeTask.Tests {
    public class TestDataService : IDataService {
        public Instrument ActiveInstrument { get; set; }
        public BitMEXApi Api { get; }
        public ReadOnlyCollection<Instrument> Instruments { get; }
        public IWebSocket WebSocket { get; } = new TestWebSocket();
    }
}
