using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BitMEXAssistant {

    public class MainController {
        private readonly BitmexRealtimeDataService _realtimeDataService;
        private readonly IMainView _mainView;
        private TradeBitMex2[] _tradeBitMex;


        public MainController(BitmexRealtimeDataService realtimeDataService, IMainView mainView, TradeBitMex2[] tradeBitMex) {
            _tradeBitMex = tradeBitMex;
            _realtimeDataService = realtimeDataService;
            _mainView = mainView;

            _realtimeDataService.Initialize();

			// Model events subscription

            _realtimeDataService.TradeDataReceived += (sender, args) => _mainView.Invoke((Action) (() => _mainView.AddTrade(args.Data)));
            _realtimeDataService.BalanceReceived += (sender, args) => _mainView.Invoke((Action)(() => _mainView.Balance = args.Data));
            _realtimeDataService.OrderBookReceived += (sender, args) => _mainView.Invoke((Action)(() => _mainView.OrderBookDataSet = args.Data));

            foreach (var t in _tradeBitMex) {
                t.OrdersChanged += (sender, args) => {
                    _mainView.Invoke((Action) (() => {
                        _mainView.Orders = t.Orders.ToList().AsReadOnly();
                    }));
                };
            }
        }

	}

    public interface IMainView {
        void AddTrade(TradeData data);

        decimal Balance { get; set; }

        OrderBookDataSet OrderBookDataSet { get; set; }
        ReadOnlyCollection<Order> Orders { get; set; }

        object Invoke(Delegate @delegate);
    }
}