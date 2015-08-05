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

    public class CancelOrder : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadOrders,
            Cancel
        }

        public delegate void CancelOrderFinished(long orderId);
        public event CancelOrderFinished OnCancelOrderFinished;

        public long OrderId { get; set; }
        public bool IsBid { get; set; }

        private bool _done = false;
        private DateTime _lastAction;
        private State _state = State.Idle;

        public CancelOrder(long orderId, bool isBid)
        {
            OrderId = orderId;
            IsBid = isBid;
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

                    if (OnCancelOrderFinished != null)
                        OnCancelOrderFinished(OrderId);
                    break;

                case State.Begin:

                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("CancelOrder:Process", "Market already open no need to open the market", Logging.White);
                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("CancelOrder:Process", "Opening Market", Logging.White);
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
                        Logging.Log("CancelOrder:Process", "Load orders", Logging.White);

                        if (marketWindow.LoadOrders() == true)
                            _state = State.Cancel;

                        break;
                    }
                    else
                    {
                        Logging.Log("CancelOrder:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.Cancel:

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
                                Logging.Log("CancelOrder:Process", "Loaded order, OrderId - " + order.OrderId + " OrderPrice - " + order.Price + " NewPrice - " + Price, Logging.White);

                                bool success = order.CancelOrder();

                                if (success)
                                    Logging.Log("CancelOrder:Process", "Canceling order successful", Logging.White);
                                else
                                    Logging.Log("CancelOrder:Process", "Canceling order failure", Logging.White);
                            }
                            else
                            {
                                Logging.Log("CancelOrder:Process", "Order no longer exists, exiting modify action", Logging.White);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Log("CancelOrder:Process", "Exception [" + ex + "] - Ending modify order script", Logging.Debug);
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
