using System.Collections.ObjectModel;
using BitMEX;

namespace BitMEXAssistant {
    public interface IDataService {
        Instrument ActiveInstrument { get; set; }
        BitMEXApi Api { get; }
        ReadOnlyCollection<Instrument> Instruments { get; }
        IWebSocket WebSocket { get; }
    }
}