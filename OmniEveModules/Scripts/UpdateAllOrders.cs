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
        enum State
        {
            Idle,
            Done,
            Begin,
            MyOrders,
            MarketInfo,
            ModifyOrders
        }

        public delegate void UpdateAllOrdersFinished();
        public event UpdateAllOrdersFinished OnUpdateAllOrdersFinished;

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event MyOrders.MyOrdersFinished OnMyOrdersFinished;

        public event MarketInfo.MarketInfoFinished OnMarketInfoFinished;

        private List<DirectOrder> _mySellOrders { get; set; }
        private List<DirectOrder> _myBuyOrders { get; set; }

        private bool _done = false;
        private State _state = State.Idle;

        private MyOrders _myOrders = null;
        private ModifyAllOrders _modifyAllOrders = null;
        private MarketInfoForList _marketInfoForList = null;

        public UpdateAllOrders()
        {
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
                    if (OnUpdateAllOrdersFinished != null)
                        OnUpdateAllOrdersFinished();

                    _done = true;
                    break;

                case State.Begin:
                    _state = State.MyOrders;
                    break;

                case State.MyOrders:
                    if (_myOrders == null)
                    {
                        _myOrders = new MyOrders();
                        _myOrders.OnMyOrdersFinished += MyOrdersFinished;

                        _myOrders.Initialize();
                    }

                    _myOrders.Process();
                    break;

                case State.MarketInfo:
                    if (_marketInfoForList == null)
                    {
                        List<int> typeIds = new List<int>();

                        foreach (DirectOrder order in _mySellOrders)
                            typeIds.Add(order.TypeId);

                        foreach (DirectOrder order in _myBuyOrders)
                        {
                            if (typeIds.FirstOrDefault(o => o == order.TypeId) == 0)
                                typeIds.Add(order.TypeId);
                        }

                        _marketInfoForList = new MarketInfoForList(typeIds);
                        _marketInfoForList.OnMarketInfoFinished += OnMarketInfoFinished;
                        _marketInfoForList.OnMarketInfoForListFinished += OnMarketInfoForListFinished;

                        _marketInfoForList.Initialize();
                    }

                    _marketInfoForList.Process();
                    break;

                case State.ModifyOrders:
                    if (_modifyAllOrders == null)
                    {
                        List<DirectOrder> sellOrders = new List<DirectOrder>();
                        List<DirectOrder> buyOrders = new List<DirectOrder>();

                        foreach (DirectOrder mySellOrder in _mySellOrders)
                        {
                            MarketItem marketItem = Cache.Instance.GetMarketItem(mySellOrder.TypeId);

                            if (marketItem == null)
                                continue;

                            DirectOrder lowestOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == mySellOrder.StationId);

                            if (lowestOrder.Price <= mySellOrder.Price && lowestOrder.OrderId != mySellOrder.OrderId)
                            {
                                double priceDifference = mySellOrder.Price - lowestOrder.Price;
                                double priceDifferencePct = priceDifference / mySellOrder.Price;

                                if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                {
                                    sellOrders.Add(mySellOrder);
                                }
                            }
                        }

                        foreach (DirectOrder myBuyOrder in _myBuyOrders)
                        {
                            MarketItem marketItem = Cache.Instance.GetMarketItem(myBuyOrder.TypeId);

                            if (marketItem == null)
                                continue;

                            DirectOrder highestOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == myBuyOrder.StationId);

                            if (highestOrder.Price >= myBuyOrder.Price && highestOrder.OrderId != myBuyOrder.OrderId)
                            {
                                double priceDifference = highestOrder.Price - myBuyOrder.Price;
                                double priceDifferencePct = priceDifference / myBuyOrder.Price;

                                if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                {
                                    buyOrders.Add(myBuyOrder);
                                }
                            }
                        }

                        _modifyAllOrders = new ModifyAllOrders(sellOrders, buyOrders);
                        _modifyAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
                        _modifyAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
                        _modifyAllOrders.OnModifyAllOrdersFinished += OnModifyAllOrdersFinished;

                        _modifyAllOrders.Initialize();
                    }

                    _modifyAllOrders.Process();
                    break;
            }
        }

        private void MyOrdersFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders)
        {
            _mySellOrders = mySellOrders;
            _myBuyOrders = myBuyOrders;

            _state = State.MarketInfo;

            OnMyOrdersFinished(mySellOrders, myBuyOrders);
        }

        private void OnMarketInfoForListFinished()
        {
            _state = State.ModifyOrders;
        }

        private void OnModifyAllOrdersFinished()
        {
            _state = State.Done;
        }

        /*private void ModifySellOrderFinished(long orderId, double price)
        {
            OnModifySellOrderFinished(orderId, price);
        }

        private void ModifyBuyOrderFinished(long orderId, double price)
        {
            OnModifyBuyOrderFinished(orderId, price);
        }*/
    }
}
