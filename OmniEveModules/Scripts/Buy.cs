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

    public class Buy : IScript
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

        public int Item { get; set; }
        public int Unit { get; set; }
        public bool UseOrders { get; set; }

        private DateTime _lastAction;
        private bool _returnBuy;
        private State _state = State.Idle;

        public void Initialize()
        {
            _state = State.Begin;
        }

        public bool IsDone()
        {
            return _state == State.Done;
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
                case State.Done:
                    break;

                case State.Begin:

                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();
                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:
                    
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (marketWindow == null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    Logging.Log("Buy:Process", "Opening Market", Logging.White);
                    _state = State.LoadItem;

                    break;

                case State.LoadItem:

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null && marketWindow.DetailTypeId != Item)
                    {
                        marketWindow.LoadTypeId(Item);
                        if (UseOrders)
                        {
                            _state = State.CreateOrder;
                        }
                        else
                        {
                            _state = State.BuyItem;
                        }

                        break;
                    }

                    break;

                case State.CreateOrder:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        List<DirectOrder> orders = marketWindow.BuyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();

                        DirectOrder order = orders.OrderByDescending(o => o.Price).FirstOrDefault();
                        if (order != null)
                        {
                            double price = order.Price + 0.01;
                            if (Cache.Instance.DirectEve.Session.StationId != null)
                            {
                                Cache.Instance.DirectEve.Buy((int)Cache.Instance.DirectEve.Session.StationId, Item, price, Unit, DirectOrderRange.Station, 1, 30);
                            }
                        }
                        UseOrders = false;
                        _state = State.Done;
                    }

                    break;

                case State.BuyItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (marketWindow != null)
                    {
                        List<DirectOrder> orders = marketWindow.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();

                        DirectOrder order = orders.OrderBy(o => o.Price).FirstOrDefault();
                        if (order != null)
                        {
                            // Calculate how much we still need
                            if (order.VolumeEntered >= Unit)
                            {
                                order.Buy(Unit, DirectOrderRange.Station);
                                _state = State.WaitForItems;
                            }
                            else
                            {
                                order.Buy(Unit, DirectOrderRange.Station);
                                Unit = Unit - order.VolumeEntered;
                                Logging.Log("Buy:Process", "Missing " + Convert.ToString(Unit) + " units", Logging.White);
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
                        Logging.Log("Buy:Process", "Return Buy", Logging.White);
                        _returnBuy = false;
                        _state = State.OpenMarket;
                        break;
                    }

                    Logging.Log("Buy:Process", "Done", Logging.White);
                    _state = State.Done;

                    break;
            }
        }
    }
}
