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

    public class MarketInfo : IAction
    {
        public delegate void MarketInfoActionFinished(MarketItemInfo marketInfo);
        public event MarketInfoActionFinished OnMarketInfoActionFinished;

        public int TypeId { get; set; }

        private DateTime _lastAction;
        private MarketInfoState _state = MarketInfoState.Idle;
        private bool _done = false;

        private MarketItemInfo _marketInfoItem;

        public void Initialize()
        {
            _state = MarketInfoState.Begin;
        }

        public bool IsDone()
        {
            return _done;
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
                case MarketInfoState.Idle:
                    break;
                case MarketInfoState.Done:
                    if (OnMarketInfoActionFinished != null)
                        OnMarketInfoActionFinished(_marketInfoItem);

                    _done = true;
                    break;

                case MarketInfoState.Begin:

                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("MarketItemInfo:Process", "Market already open no need to open the market", Logging.White);

                    _state = MarketInfoState.OpenMarket;
                    break;

                case MarketInfoState.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("MarketItemInfo:Process", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                    {
                        Logging.Log("MarketItemInfo:Process", "Market window is not ready", Logging.White);
                        break;
                    }

                    _state = MarketInfoState.LoadItem;
                    break;

                case MarketInfoState.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("MarketItemInfo:Process", "Load orders for TypeId - " + TypeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != TypeId)
                        {
                            if(marketWindow.LoadTypeId(TypeId) == true)
                                _state = MarketInfoState.CacheInfo;
                        }

                        break;
                    }
                    else
                    {
                        Logging.Log("MarketItemInfo:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = MarketInfoState.OpenMarket;
                    }

                    break;

                case MarketInfoState.CacheInfo:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                        {
                            Logging.Log("MarketItemInfo:Process", "Market window is not ready", Logging.White);
                            break;
                        }

                        if (marketWindow.DetailTypeId != TypeId)
                            _state = MarketInfoState.LoadItem;

                        Logging.Log("MarketItemInfo:Process", "Get list of orders for Item - " + TypeId.ToString(), Logging.White);

                        _marketInfoItem = new MarketItemInfo();

                        if (_marketInfoItem != null)
                        {
                            Logging.Log("MarketItemInfo:Process", "Get list of orders successful", Logging.White);

                            _marketInfoItem.SellOrders = marketWindow.SellOrders;
                            _marketInfoItem.BuyOrders = marketWindow.BuyOrders;
                            _marketInfoItem.TypeId = TypeId;

                            _state = MarketInfoState.Done;
                        }
                    }
                    else
                    {
                        _state = MarketInfoState.OpenMarket;
                    }
                    break;
            }
        }
    }
}
