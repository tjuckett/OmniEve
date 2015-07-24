using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    using DirectEve;
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;
    using OmniEveModules.States;
    using OmniEveModules.Status;

    public class MyOrders : IScript
    {
        public delegate void MyOrdersActionFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders);
        public event MyOrdersActionFinished OnMyOrdersActionFinished;

        private DateTime _lastAction;
        private MyOrdersState _state = MyOrdersState.Idle;
        private bool _done = false;
        private List<DirectOrder> _myBuyOrders = null;
        private List<DirectOrder> _mySellOrders = null;

        public void Initialize()
        {
            _state = MyOrdersState.Begin;
        }

        public bool IsDone()
        {
            return _done == true;
        }

        public void Process()
        {
            if (!Status.Instance.InStation)
                return;

            if (Status.Instance.InSpace)
                return;

            DirectMarketWindow marketWindow = Cache.Instance.DirectEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();

            switch (_state)
            {
                case MyOrdersState.Idle:
                    break;
                case MyOrdersState.Done:
                    if (OnMyOrdersActionFinished != null)
                        OnMyOrdersActionFinished(_mySellOrders, _myBuyOrders);

                    _done = true;
                    break;

                case MyOrdersState.Begin:

                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("MyOrders:Process", "Market already open no need to open the market", Logging.White);

                    _state = MyOrdersState.OpenMarket;
                    break;

                case MyOrdersState.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("MyOrders:Process", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                    {
                        Logging.Log("MyOrders:Process", "Market window is not ready", Logging.White);
                        break;
                    }

                    _state = MyOrdersState.LoadOrders;
                    break;

                case MyOrdersState.LoadOrders:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("MyOrders:Process", "Load orders", Logging.White);

                        if(marketWindow.LoadOrders() == true)
                            _state = MyOrdersState.CacheOrders;

                        break;
                    }
                    else
                    {
                        Logging.Log("MyOrders:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = MyOrdersState.OpenMarket;
                    }

                    break;

                case MyOrdersState.CacheOrders:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (marketWindow != null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Logging.Log("MyOrders:Process", "Get list of my orders", Logging.White);

                        _myBuyOrders = marketWindow.GetMyOrders(true).ToList();
                        _mySellOrders = marketWindow.GetMyOrders(false).ToList();

                        if (_mySellOrders != null)
                        {
                            Logging.Log("MyOrders:Process", "Get list of my sell orders successful", Logging.White);
                            Cache.Instance.MySellOrders = _mySellOrders;
                        }

                        if (_myBuyOrders != null)
                        {
                            Logging.Log("MyOrders:Process", "Get list of my buy orders successful", Logging.White);
                            Cache.Instance.MyBuyOrders = _myBuyOrders;
                        }

                        _state = MyOrdersState.Done;
                    }
                    else
                    {
                        _state = MyOrdersState.OpenMarket;
                    }
                    break;
            }
        }
    }
}

