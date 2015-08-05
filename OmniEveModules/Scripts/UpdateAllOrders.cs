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

    public class UpdateAllOrders : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            PopNextModify,
            ProcessModify,
            PopNextCancel,
            ProcessCancel
        }

        public delegate void UpdateAllOrdersFinished();
        public event UpdateAllOrdersFinished OnUpdateAllOrdersFinished;

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event CancelOrder.CancelOrderFinished OnCancelOrderFinished;

        public List<DirectOrder> SellOrders { get; set; }
        public List<DirectOrder> BuyOrders { get; set; }

        private bool _done = false;
        private State _state = State.Idle;
        private List<ModifyOrder> _modifyOrders = new List<ModifyOrder>();
        private ModifyOrder _currentModify = null;

        private List<CancelOrder> _cancelOrders = new List<CancelOrder>();
        private CancelOrder _currentCancel = null;

        public UpdateAllOrders(List<DirectOrder> sellOrders, List<DirectOrder> buyOrders)
        {
            SellOrders = sellOrders;
            BuyOrders = buyOrders;
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

            switch (_state)
            {
                case State.Idle:
                    break;
                case State.Done:
                    _done = true;

                    if (OnUpdateAllOrdersFinished != null)
                        OnUpdateAllOrdersFinished();
                    break;

                case State.Begin:
                    // Create all the modify orders
                    foreach(DirectOrder order in SellOrders)
                    {
                        MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                        if(marketItem == null)
                            continue;

                        DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
                        DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                        if (lowestSellOrder != null && lowestSellOrder.Price <= order.Price && lowestSellOrder.OrderId != order.OrderId)
                        {
                            double priceDifference = order.Price - lowestSellOrder.Price;
                            double priceDifferencePct = priceDifference / order.Price;
                            double price = double.Parse((decimal.Parse(lowestSellOrder.Price.ToString()) - 0.01m).ToString());

                            bool createModifyOrder = false;

                            if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                createModifyOrder = true;
                            else if (highestBuyOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                                createModifyOrder = true;
                            
                            if(createModifyOrder == true)
                            { 
                                Logging.Log("UpdateAllOrders:Process", "Creating sell modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                                ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, false, price);
                                modifyOrder.OnModifyOrderFinished += OnModifySellOrderFinished;
                                _modifyOrders.Add(modifyOrder);
                            }
                        }
                    }

                    // Create all the modify orders and cancel orders
                    foreach (DirectOrder order in BuyOrders)
                    {
                        MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                        if (marketItem == null)
                            continue;

                        DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
                        DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                        double profit = lowestSellOrder.Price - highestBuyOrder.Price;
                        double tax = lowestSellOrder.Price * .01 + highestBuyOrder.Price * 0.015;
                        double profitPct = lowestSellOrder.Price / highestBuyOrder.Price;

                        if (highestBuyOrder != null && lowestSellOrder != null && ((profit < 10000000 && profitPct < 1.25) || (profit >= 10000000 && tax > profit * 0.5)))
                        {
                            CancelOrder cancelOrder = new CancelOrder(order.OrderId, true);
                            cancelOrder.OnCancelOrderFinished += OnCancelOrderFinished;
                            _cancelOrders.Add(cancelOrder);
                        }
                        // Don't modify if there isn't a lowest order and don't modify if the lowest order is our own
                        else if (highestBuyOrder != null && highestBuyOrder.OrderId != order.OrderId && highestBuyOrder.Price >= order.Price)
                        {
                            double priceDifference = highestBuyOrder.Price - order.Price;
                            double priceDifferencePct = priceDifference / order.Price;
                            double price = double.Parse((decimal.Parse(highestBuyOrder.Price.ToString()) + 0.01m).ToString());

                            bool createModifyOrder = false;

                            if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                createModifyOrder = true;
                            else if (lowestSellOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                                createModifyOrder = true;

                            if (createModifyOrder == true)
                            { 
                                Logging.Log("UpdateAllOrders:Process", "Creating buy modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                                ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, true, price);
                                modifyOrder.OnModifyOrderFinished += OnModifyBuyOrderFinished;
                                _modifyOrders.Add(modifyOrder);
                            }
                        }
                    }

                    _state = State.PopNextModify;
                    break;

                case State.PopNextModify:

                    _currentModify = _modifyOrders.FirstOrDefault();
                    
                    if (_currentModify != null)
                    {
                        _modifyOrders.Remove(_currentModify);

                        Logging.Log("UpdateAllOrders:Process", "Popping next order script to run", Logging.White);

                        _currentModify.Initialize();
                        _state = State.ProcessModify;
                    }
                    else
                    {
                        Logging.Log("UpdateAllOrders:Process", "No more modify order scripts left, going to cancel orders", Logging.White);
                        _state = State.PopNextCancel;
                    }
                    break;

                case State.ProcessModify:

                    if (_currentModify != null)
                    {
                        _currentModify.Process();

                        // If the current script is done then pop the next one
                        if (_currentModify.IsDone() == true)
                        {
                            Logging.Log("UpdateAllOrders:Process", "Modify script is done, executing callback and popping next", Logging.White);
                            _state = State.PopNextModify;
                        }
                    }

                    break;

                case State.PopNextCancel:

                    _currentCancel = _cancelOrders.FirstOrDefault();

                    if (_currentCancel != null)
                    {
                        _cancelOrders.Remove(_currentCancel);

                        Logging.Log("UpdateAllOrders:Process", "Popping next order script to run", Logging.White);

                        _currentCancel.Initialize();
                        _state = State.ProcessModify;
                    }
                    else
                    {
                        Logging.Log("UpdateAllOrders:Process", "No more cancel order scripts left, going to done state", Logging.White);
                        _state = State.Done;
                    }
                    break;

                case State.ProcessCancel:

                    if (_currentCancel != null)
                    {
                        _currentCancel.Process();

                        // If the current script is done then pop the next one
                        if (_currentCancel.IsDone() == true)
                        {
                            Logging.Log("UpdateAllOrders:Process", "Cancel script is done, executing callback and popping next", Logging.White);
                            _state = State.PopNextCancel;
                        }
                    }

                    break;
            }
        }
    }
}
