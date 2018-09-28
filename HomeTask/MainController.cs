namespace BitMEXAssistant {

    public class MainController {
        private readonly BitmexRealtimeDataService _realtimeDataService;
        private readonly IMainView _mainView;

        public MainController(BitmexRealtimeDataService realtimeDataService, IMainView mainView) {
            _realtimeDataService = realtimeDataService;
            _mainView = mainView;

            _realtimeDataService.Initialize();

			// Model events subscription
            _realtimeDataService.TradeDataReceived += (sender, args) => _mainView.AddTrade(args.Data);
            _realtimeDataService.BalanceReceived += (sender, args) => _mainView.Balance = args.Data;
            _realtimeDataService.OrderBookReceived += (sender, args) => _mainView.OrderBookDataSet = args.Data;

        }

		private void _realtimeDataService_OrderBookReceived(object sender, EventArgs<OrderBookDataSet> e)
		{
			throw new System.NotImplementedException();
		}
	}

    public interface IMainView {
        void AddTrade(TradeData data);

        decimal Balance { get; set; }

        OrderBookDataSet OrderBookDataSet { get; set; }
    }
}