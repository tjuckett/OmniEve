using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Actions
{
    using DirectEve;
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;
    using OmniEveModules.States;
    using OmniEveModules.Status;

    public class MyOrders : IAction
    {
        private DateTime _lastAction;
        private MyOrdersState _state = MyOrdersState.Idle;

        public void Initialize()
        {
            _state = MyOrdersState.Begin;
        }

        public bool IsDone()
        {
            return _state == MyOrdersState.Done;
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
                case MyOrdersState.Done:
                    break;

                case MyOrdersState.Begin:

                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("MyOrders", "Market already open no need to open the market", Logging.White);

                    _state = MyOrdersState.OpenMarket;
                    break;

                case MyOrdersState.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("MyOrders", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                    {
                        Logging.Log("MyOrders", "Market window is not ready", Logging.White);
                        break;
                    }

                    _state = MyOrdersState.LoadOrders;
                    break;

                case MyOrdersState.LoadOrders:

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("MyOrders", "Load orders", Logging.White);

                        marketWindow.LoadOrders();

                        _state = MyOrdersState.CacheOrders;
                        break;
                    }
                    else
                    {
                        Logging.Log("BuyOrders", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = MyOrdersState.OpenMarket;
                    }

                    break;

                case MyOrdersState.CacheOrders:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (marketWindow != null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Logging.Log("MyOrders", "Get list of my orders", Logging.White);

                        List<DirectOrder> buyOrders = marketWindow.GetMyOrders(true).ToList();
                        List<DirectOrder> sellOrders = marketWindow.GetMyOrders(false).ToList();

                        if (sellOrders != null)
                        {
                            Logging.Log("MyOrders", "Get list of my sell orders successful", Logging.White);
                            Cache.Instance.MySellOrders = sellOrders;
                        }

                        if (buyOrders != null)
                        {
                            Logging.Log("MyOrders", "Get list of my buy orders successful", Logging.White);
                            Cache.Instance.MyBuyOrders = buyOrders;                            
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

