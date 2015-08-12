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

    public class UpdateOrder : IAction
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadOrders,
            MarketInfo,
            Update
        }

        public delegate void UpdateOrderFinished(long orderId);
        public event UpdateOrderFinished OnUpdateOrderFinished;

        public long OrderId { get; set; }
        public bool IsBid { get; set; }

        private bool _done = false;
        private DateTime _lastAction;
        private State _state = State.Idle;

        public UpdateOrder(long orderId, bool isBid)
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

                    if (OnUpdateOrderFinished != null)
                        OnUpdateOrderFinished(OrderId);
                    break;

                case State.Begin:
                    
                    // Don't close the market window if its already up
                    if (marketWindow != null)
                        Logging.Log("UpdateOrder:Process", "Market already open no need to open the market", Logging.White);
                    _state = State.OpenMarket;
                    break;

                case State.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        Logging.Log("UpdateOrder:Process", "Opening Market", Logging.White);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    _state = State.LoadOrders;
                    break;

                case State.LoadOrders:

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                            break;

                        Logging.Log("UpdateOrder:Process", "Load orders", Logging.White);

                        if(marketWindow.LoadOrders() == true)
                            _state = State.MarketInfo;

                        break;
                    }
                    else
                    {
                        Logging.Log("UpdateOrder:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.MarketInfo:
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 1)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                            break;

                        List<DirectOrder> orders = marketWindow.GetMyOrders(IsBid).ToList();

                        if(orders == null)
                        {
                            Logging.Log("UpdateOrder:Process", "Something is wrong, order list is empty, ending update order action", Logging.White);
                            _state = State.Done;
                        }

                        DirectOrder order = orders.FirstOrDefault(o => o.OrderId == OrderId);

                        if(order == null)
                        {
                            Logging.Log("UpdateOrder:Process", "Order doesn't exist, ending update order action OrderId - " + OrderId, Logging.White);
                            _state = State.Done;
                            break;
                        }

                        Logging.Log("UpdateOrder:Process", "Load orders for TypeId - " + order.TypeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != order.TypeId)
                        {
                            if (marketWindow.LoadTypeId(order.TypeId) == true)
                                _state = State.Update;
                        }
                        else
                            _state = State.Update;

                        break;
                    }
                    else
                    {
                        Logging.Log("MarketItemInfo:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.Update:

                    // We keep getting the popup saying we can't modify many orders in a minute, so this needs to be at 6 or higher, probably higher
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 1)
                        break;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                            break;

                        try
                        {
                            _lastAction = DateTime.UtcNow;

                            List<DirectOrder> orders = marketWindow.GetMyOrders(IsBid).ToList();
                            DirectOrder order = orders.FirstOrDefault(o => o.OrderId == OrderId);

                            if (order != null)
                            {
                                Logging.Log("UpdateOrder:Process", "Loaded order, OrderId - " + order.OrderId + " OrderPrice - " + order.Price, Logging.White);

                                if(IsBid)
                                    UpdateBuyOrder(order, marketWindow.SellOrders, marketWindow.BuyOrders);
                                else
                                    UpdateSellOrder(order, marketWindow.SellOrders, marketWindow.BuyOrders);
                            }
                            else
                            {
                                Logging.Log("UpdateOrder:Process", "Order no longer exists, exiting modify action", Logging.White);
                            }
                        }
                        catch(Exception ex)
                        {
                            Logging.Log("UpdateOrder:Process", "Exception [" + ex + "] - Ending modify order script", Logging.Debug);
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

        public void UpdateSellOrder(DirectOrder order, List<DirectOrder> sellOrders, List<DirectOrder> buyOrders)
        {
            if (order == null)
                return;

            DirectOrder highestBuyOrder = buyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
            DirectOrder lowestSellOrder = sellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

            if (lowestSellOrder != null && lowestSellOrder.Price <= order.Price && lowestSellOrder.OrderId != order.OrderId)
            {
                double priceDifference = order.Price - lowestSellOrder.Price;
                double priceDifferencePct = priceDifference / order.Price;
                double price = double.Parse((decimal.Parse(lowestSellOrder.Price.ToString()) - 0.01m).ToString());

                bool UpdateOrder = false;

                if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                    UpdateOrder = true;
                else if (highestBuyOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                    UpdateOrder = true;

                if (UpdateOrder == true)
                {
                    Logging.Log("UpdateOrder:UpdateSellOrder", "Modifying order  for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                    bool success = order.ModifyOrder(price);
                }
            }
        }

        public void UpdateBuyOrder(DirectOrder order, List<DirectOrder> sellOrders, List<DirectOrder> buyOrders)
        {
            if (order == null)
                return;

            DirectOrder highestBuyOrder = buyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
            DirectOrder lowestSellOrder = sellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

            double profit = lowestSellOrder.Price - highestBuyOrder.Price;
            double tax = lowestSellOrder.Price * .01 + highestBuyOrder.Price * 0.015;
            double profitPct = lowestSellOrder.Price / highestBuyOrder.Price;

            if (highestBuyOrder != null && lowestSellOrder != null && ((profit < 10000000 && profitPct < 1.25) || (profit >= 10000000 && tax > profit * 0.5)))
            {
                Logging.Log("UpdateAllOrders:Process", "Canceling order for Order Id - " + order.OrderId, Logging.White);
                order.CancelOrder();
            }
            // Don't modify if there isn't a lowest order and don't modify if the lowest order is our own
            else if (highestBuyOrder != null && highestBuyOrder.OrderId != order.OrderId && highestBuyOrder.Price >= order.Price)
            {
                double priceDifference = highestBuyOrder.Price - order.Price;
                double priceDifferencePct = priceDifference / order.Price;
                double price = double.Parse((decimal.Parse(highestBuyOrder.Price.ToString()) + 0.01m).ToString());

                bool createUpdateOrder = false;

                if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                    createUpdateOrder = true;
                else if (lowestSellOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                    createUpdateOrder = true;

                if (createUpdateOrder == true)
                {
                    Logging.Log("UpdateAllOrders:Process", "Modifying order for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                    bool success = order.ModifyOrder(price);
                }
            }
        }
    }
}
