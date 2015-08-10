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

    public class ModifyOrder : IAction
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadOrders,
            Modify
        }

        public delegate void ModifyOrderFinished(long orderId, double price);
        public event ModifyOrderFinished OnModifyOrderFinished;

        public long OrderId { get; set; }
        public bool IsBid { get; set; }
        public double Price { get; set; }

        private bool _done = false;
        private DateTime _lastAction;
        private State _state = State.Idle;

        public ModifyOrder(long orderId, bool isBid, double price)
        {
            OrderId = orderId;
            IsBid = isBid;
            Price = price;
        }

        public void Initialize()
        {
            _state = State.Begin;
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
                case State.Idle:
                    break;
                case State.Done:
                    _done = true;

                    if (OnModifyOrderFinished != null)
                        OnModifyOrderFinished(OrderId, Price);
                    break;

                case State.Begin:
                    
                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("ModifyOrder:Process", "Market already open no need to open the market", Logging.White);
                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("ModifyOrder:Process", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    _state = State.LoadOrders;
                    break;

                case State.LoadOrders:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("ModifyOrder:Process", "Load orders", Logging.White);

                        if(marketWindow.LoadOrders() == true)
                            _state = State.Modify;

                        break;
                    }
                    else
                    {
                        Logging.Log("ModifyOrder:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.Modify:

                    // We keep getting the popup saying we can't modify many orders in a minute, so this needs to be at 6 or higher, probably higher
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (marketWindow != null)
                    {
                        try
                        {
                            _lastAction = DateTime.UtcNow;

                            List<DirectOrder> orders = marketWindow.GetMyOrders(IsBid).ToList();
                            DirectOrder order = orders.FirstOrDefault(o => o.OrderId == OrderId);

                            if (order != null)
                            {
                                Logging.Log("ModifyOrder:Process", "Loaded order, OrderId - " + order.OrderId + " OrderPrice - " + order.Price + " NewPrice - " + Price, Logging.White);

                                bool success = order.ModifyOrder(Price);

                                if (success)
                                    Logging.Log("ModifyOrder:Process", "Modifying order successful", Logging.White);
                                else
                                    Logging.Log("ModifyOrder:Process", "Modifying order failure", Logging.White);
                            }
                            else
                            {
                                Logging.Log("ModifyOrder:Process", "Order no longer exists, exiting modify action", Logging.White);
                            }
                        }
                        catch(Exception ex)
                        {
                            Logging.Log("ModifyOrder:Process", "Exception [" + ex + "] - Ending modify order script", Logging.Debug);
                        }

                        _state = State.Done;
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
