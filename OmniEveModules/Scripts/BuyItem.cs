using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectEve;

namespace OmniEveModules.Scripts
{
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;
    using OmniEveModules.States;
    using OmniEveModules.Status;

    public class BuyItem : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadItem,
            BuyItem,
            WaitForItems,
            CreateOrder
        }

        public delegate void BuyItemFinished(int typeId, bool orderCreated);
        public event BuyItemFinished OnBuyItemFinished;

        private int _typeId = 0;
        private int _volume = 0;
        private bool _createOrder = false;
        private DateTime _lastAction;
        private bool _returnBuy = false;
        private bool _done = false;
        private State _state = State.Idle;

        public BuyItem(int typeId, int volume, bool createOrder)
        {
            _typeId = typeId;
            _volume = volume;
            _createOrder = createOrder;
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
                    if (OnBuyItemFinished != null)
                        OnBuyItemFinished(_typeId, _createOrder);

                    _done = true;
                    break;

                case State.Begin:

                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();
                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:
                    
                    if (marketWindow == null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                    }

                    if (!marketWindow.IsReady)
                    {
                        Logging.Log("BuyItem:Process", "Market window is not ready", Logging.White);
                        break;
                    }

                    Logging.Log("BuyItem:Process", "Opening Market", Logging.White);
                    _state = State.LoadItem;

                    break;

                case State.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("BuyItem:Process", "Load orders for TypeId - " + _typeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != _typeId)
                        {
                            if (marketWindow.LoadTypeId(_typeId))
                            {
                                if(_createOrder == true)
                                    _state = State.CreateOrder;
                                else
                                    _state = State.BuyItem;
                            }
                        }

                        break;
                    }
                    else
                    {
                        Logging.Log("BuyItem:Process", "Market Window is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.CreateOrder:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                        {
                            Logging.Log("BuyItem:Process", "Market window is not ready", Logging.White);
                            break;
                        }

                        if (marketWindow.DetailTypeId != _typeId)
                        {
                            _state = State.LoadItem;
                            break;
                        }

                        List<DirectOrder> orders = marketWindow.BuyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();

                        DirectOrder order = orders.OrderByDescending(o => o.Price).FirstOrDefault();
                        if (order != null)
                        {
                            double price = double.Parse((decimal.Parse(order.Price.ToString()) + 0.01m).ToString());
                            if (Cache.Instance.DirectEve.Session.StationId != null)
                            {
                                Cache.Instance.DirectEve.Buy((int)Cache.Instance.DirectEve.Session.StationId, _typeId, price, _volume, DirectOrderRange.Station, 1, 90);
                            }
                        }
                        _state = State.Done;
                    }

                    break;

                case State.BuyItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                        {
                            Logging.Log("BuyItem:Process", "Market window is not ready", Logging.White);
                            break;
                        }

                        if (marketWindow.DetailTypeId != _typeId)
                        {
                            _state = State.LoadItem;
                            break;
                        }

                        List<DirectOrder> orders = marketWindow.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();

                        DirectOrder order = orders.OrderBy(o => o.Price).FirstOrDefault();
                        if (order != null)
                        {
                            // Calculate how much we still need
                            if (order.VolumeEntered >= _volume)
                            {
                                order.Buy(_volume, DirectOrderRange.Station);
                                _state = State.WaitForItems;
                            }
                            else
                            {
                                order.Buy(_volume, DirectOrderRange.Station);
                                _volume = _volume - order.VolumeEntered;
                                Logging.Log("BuyItem:Process", "Missing " + Convert.ToString(_volume) + " units", Logging.White);
                                _returnBuy = true;
                                _state = State.WaitForItems;
                            }
                        }
                    }

                    break;

                case State.WaitForItems:
                    // Wait 5 seconds after moving
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();

                    if (_returnBuy)
                    {
                        Logging.Log("BuyItem:Process", "Return Buy", Logging.White);
                        _returnBuy = false;
                        _state = State.OpenMarket;
                        break;
                    }

                    Logging.Log("BuyItem:Process", "Done", Logging.White);
                    _state = State.Done;

                    break;
            }
        }
    }
}
