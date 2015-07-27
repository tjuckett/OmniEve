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

    public class ModifyAllOrders : IScript
    {
        public delegate void ModifyAllOrdersFinished();
        public event ModifyAllOrdersFinished OnModifyAllOrdersFinished;

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public List<DirectOrder> SellOrders { get; set; }
        public List<DirectOrder> BuyOrders { get; set; }

        private bool _done = false;
        private ModifyAllOrdersState _state = ModifyAllOrdersState.Idle;
        private List<ModifyOrder> _modifyOrders = new List<ModifyOrder>();
        private ModifyOrder _currentModify = null;

        public ModifyAllOrders(List<DirectOrder> sellOrders, List<DirectOrder> buyOrders)
        {
            SellOrders = sellOrders;
            BuyOrders = buyOrders;
        }

        public void Initialize()
        {
            _state = ModifyAllOrdersState.Begin;
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
                case ModifyAllOrdersState.Idle:
                    break;
                case ModifyAllOrdersState.Done:
                    if (OnModifyAllOrdersFinished != null)
                        OnModifyAllOrdersFinished();

                    _done = true;
                    break;

                case ModifyAllOrdersState.Begin:
                    // Create all the modify orders
                    foreach(DirectOrder order in SellOrders)
                    {
                        MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                        if(marketItem == null)
                            continue;

                        DirectOrder lowestOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                        // Don't modify if there isn't a lowest order and don't modify if the lowest order is our own
                        if(lowestOrder != null && lowestOrder.OrderId != order.OrderId && lowestOrder.Price <= order.Price)
                        {
                            double price = double.Parse((decimal.Parse(lowestOrder.Price.ToString()) - 0.01m).ToString());

                            Logging.Log("ModifyAllOrders:Process", "Creating sell modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                            ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, false, price);
                            modifyOrder.OnModifyOrderFinished += OnModifySellOrderFinished;
                            _modifyOrders.Add(modifyOrder);
                        }
                    }

                    // Create all the modify orders
                    foreach (DirectOrder order in BuyOrders)
                    {
                        MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                        if (marketItem == null)
                            continue;

                        DirectOrder highestOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                        // Don't modify if there isn't a lowest order and don't modify if the lowest order is our own
                        if (highestOrder != null && highestOrder.OrderId != order.OrderId && highestOrder.Price >= order.Price)
                        {
                            double price = double.Parse((decimal.Parse(highestOrder.Price.ToString()) + 0.01m).ToString());

                            Logging.Log("ModifyAllOrders:Process", "Creating buy modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                            ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, true, price);
                            modifyOrder.OnModifyOrderFinished += OnModifyBuyOrderFinished;
                            _modifyOrders.Add(modifyOrder);
                        }
                    }

                    _state = ModifyAllOrdersState.PopNext;
                    break;

                case ModifyAllOrdersState.PopNext:

                    _currentModify = _modifyOrders.FirstOrDefault();
                    
                    if (_currentModify != null)
                    {
                        _modifyOrders.Remove(_currentModify);

                        Logging.Log("ModifyAllOrders:Process", "Popping next order script to run", Logging.White);

                        _currentModify.Initialize();
                        _state = ModifyAllOrdersState.Process;
                    }
                    else
                    {
                        Logging.Log("ModifyAllOrders:Process", "No more order scripts left, going to done state", Logging.White);
                        _state = ModifyAllOrdersState.Done;
                    }
                    break;

                case ModifyAllOrdersState.Process:

                    if (_currentModify != null)
                    {
                        _currentModify.Process();

                        // If the current script is done then pop the next one
                        if (_currentModify.IsDone() == true)
                        {
                            Logging.Log("ModifyAllOrders:Process", "Modify script is done, executing callback and popping next", Logging.White);
                            _state = ModifyAllOrdersState.PopNext;
                        }
                    }

                    break;
            }
        }
    }
}
