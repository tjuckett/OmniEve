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
        public delegate void ModifyOrderActionFinished(long orderId, double price);
        public event ModifyOrderActionFinished OnModifyOrderActionFinished;

        public long OrderId { get; set; }
        public bool IsBid { get; set; }
        public double Price { get; set; }

        private bool _done = false;
        private DateTime _lastAction;
        private ModifyOrderState _state = ModifyOrderState.Idle;

        public void Initialize()
        {
            _state = ModifyOrderState.Begin;
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
                case ModifyOrderState.Idle:
                    break;
                case ModifyOrderState.Done:
                    if (OnModifyOrderActionFinished != null)
                        OnModifyOrderActionFinished(OrderId, Price);

                    _done = true;
                    break;

                case ModifyOrderState.Begin:
                    
                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("ModifyOrder:Process", "Market already open no need to open the market", Logging.White);
                    _state = ModifyOrderState.OpenMarket;
                    break;

                case ModifyOrderState.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("ModifyOrder:Process", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    _state = ModifyOrderState.Modify;
                    break;

                case ModifyOrderState.LoadOrders:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("ModifyOrder:Process", "Load orders", Logging.White);

                        if(marketWindow.LoadOrders() == true)
                            _state = ModifyOrderState.Modify;

                        break;
                    }
                    else
                    {
                        Logging.Log("ModifyOrder:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = ModifyOrderState.OpenMarket;
                    }

                    break;

                case ModifyOrderState.Modify:

                    // We keep getting the popup saying we can't modify many orders in a minute, so this needs to be at 6 or higher, probably higher
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 6)
                        break;

                    _lastAction = DateTime.UtcNow;

                    List<DirectOrder> orders = marketWindow.GetMyOrders(IsBid).ToList();
                    DirectOrder order = orders.First(o => o.OrderId == OrderId);

                    Logging.Log("ModifyOrder:Process", "Loaded order, OrderId - " + order.OrderId + " OrderPrice - " + order.Price + " NewPrice - " + Price, Logging.White);

                    bool success = order.ModifyOrder(Price);

                    if(success)
                        Logging.Log("ModifyOrder:Process", "Modifying order successful", Logging.White);
                    else
                        Logging.Log("ModifyOrder:Process", "Modifying order failure", Logging.White);

                    _state = ModifyOrderState.Done;
                    break;
            }
        }
    }
}
