using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BitMEXAssistant {

    public class MainController {
        private readonly BitmexRealtimeDataService _bitmexRealtimeDataService;
		private HitBtcRealtimeDataService _hitBtcRealtimeDataService;

		private readonly IMainView _mainView;
        private TradeBitMex2[] _tradeBitMex;



        public MainController(BitmexRealtimeDataService bitmexRealtimeDataService, HitBtcRealtimeDataService hitBtcRealtimeDataService, IMainView mainView, TradeBitMex2[] tradeBitMex) {
            _tradeBitMex = tradeBitMex;
			_bitmexRealtimeDataService = bitmexRealtimeDataService;
			_hitBtcRealtimeDataService = hitBtcRealtimeDataService;
			_mainView = mainView;

			_bitmexRealtimeDataService.Initialize(); // BitMex start subscription and websocket events listening
			_hitBtcRealtimeDataService.Initialize(); 


			// Model events subscription

			_bitmexRealtimeDataService.TradeDataReceived += (sender, args) => _mainView.Invoke((Action) (() => _mainView.AddTrade(args.Data)));
			_bitmexRealtimeDataService.BalanceReceived += (sender, args) => _mainView.Invoke((Action)(() => _mainView.Balance = args.Data));
			_bitmexRealtimeDataService.OrderBookReceived += (sender, args) => _mainView.Invoke((Action)(() => _mainView.OrderBookDataSet = args.Data));

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