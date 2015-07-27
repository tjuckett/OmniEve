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

    public class MarketInfo : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadItem,
            CacheInfo
        }

        public delegate void MarketInfoFinished(MarketItem marketItem);
        public event MarketInfoFinished OnMarketInfoFinished;

        public int TypeId { get; set; }

        private DateTime _lastAction;
        private State _state = State.Idle;
        private bool _done = false;
        private MarketItem _marketItem;

        public MarketInfo(int typeId)
        {
            TypeId = typeId;
        }

        public void Initialize()
        {
            _state = State.Begin;
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
                case State.Idle:
                    break;
                case State.Done:
                    if (OnMarketInfoFinished != null)
                        OnMarketInfoFinished(_marketItem);

                    _done = true;
                    break;

                case State.Begin:

                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("MarketItemInfo:Process", "Market already open no need to open the market", Logging.White);

                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:

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

                    _state = State.LoadItem;
                    break;

                case State.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("MarketItemInfo:Process", "Load orders for TypeId - " + TypeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != TypeId)
                        {
                            if(marketWindow.LoadTypeId(TypeId) == true)
                                _state = State.CacheInfo;
                        }

                        break;
                    }
                    else
                    {
                        Logging.Log("MarketItemInfo:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.CacheInfo:

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
                            _state = State.LoadItem;

                        Logging.Log("MarketItemInfo:Process", "Get list of orders for Item - " + TypeId.ToString(), Logging.White);

                        _marketItem = new MarketItem();

                        if (_marketItem != null)
                        {
                            Logging.Log("MarketItemInfo:Process", "Get list of orders successful", Logging.White);

                            _marketItem.SellOrders = marketWindow.SellOrders;
                            _marketItem.BuyOrders = marketWindow.BuyOrders;
                            _marketItem.TypeId = TypeId;

                            Cache.Instance.SetMarketItem(TypeId, _marketItem);

                            _state = State.Done;
                        }
                    }
                    else
                    {
                        _state = State.OpenMarket;
                    }
                    break;
            }
        }
    }
}
