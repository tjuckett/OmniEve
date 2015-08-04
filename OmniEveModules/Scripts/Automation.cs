using System;
using System.Collections.Generic;
using System.IO;
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

    public class Automation : IScript
    {
        enum State
        {
            Idle,
            Done,
            Begin,
            MyOrders,
            MarketInfo,
            ModifyOrders,
            CheckItemHanger,
            CreateSellOrders,
            LoadNewOrders,
            CreateBuyOrders,
        }

        public delegate void AutomationFinished();
        public event AutomationFinished OnAutomationFinished;

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event MyOrders.MyOrdersFinished OnMyOrdersFinished;

        public event MarketInfo.MarketInfoFinished OnMarketInfoFinished;

        public event ItemHanger.ItemHangerFinished OnItemHangerFinished;

        public event SellItems.SellItemsFinished OnSellItemsFinished;
        public event SellItem.SellItemFinished OnSellItemFinished;

        public event BuyItems.BuyItemsFinished OnBuyItemsFinished;
        public event BuyItem.BuyItemFinished OnBuyItemFinished;

        private List<DirectOrder> _mySellOrders { get; set; }
        private List<DirectOrder> _myBuyOrders { get; set; }
        private List<DirectItem> _itemsInHanger { get; set; }

        private bool _done = false;
        private State _state = State.Idle;
        private int _newSellOrders = 0;
        private int _newBuyOrders = 0;

        private MyOrders _myOrders = null;
        private ModifyAllOrders _modifyAllOrders = null;
        private MarketInfoForList _marketInfoForList = null;
        private ItemHanger _itemHanger = null;
        private SellItems _sellItems = null;
        private BuyItems _buyItems = null;

        public Automation()
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
                    if (OnAutomationFinished != null)
                        OnAutomationFinished();

                    _done = true;
                    break;

                case State.Begin:
                    _state = State.MyOrders;
                    break;

                case State.MyOrders:
                    try
                    {
                        if (_myOrders == null)
                        {
                            Logging.Log("Automation:Process", "MyOrders State - Begin", Logging.Debug);

                            _myOrders = new MyOrders();
                            _myOrders.OnMyOrdersFinished += MyOrdersFinished;
                            _myOrders.OnMyOrdersFinished += OnMyOrdersFinished;

                            _myOrders.Initialize();
                        }

                        _myOrders.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.MarketInfo:
                    try
                    {
                        if (_marketInfoForList == null)
                        {
                            Logging.Log("Automation:Process", "MarketInfo State - Begin", Logging.Debug);

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
                            _marketInfoForList.OnMarketInfoForListFinished += MarketInfoForListFinished;

                            _marketInfoForList.Initialize();
                        }

                        _marketInfoForList.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.ModifyOrders:
                    try
                    {
                        if (_modifyAllOrders == null)
                        {
                            Logging.Log("Automation:Process", "ModifyOrders State - Begin", Logging.Debug);

                            List<DirectOrder> sellOrders = new List<DirectOrder>();
                            List<DirectOrder> buyOrders = new List<DirectOrder>();

                            foreach (DirectOrder mySellOrder in _mySellOrders)
                            {
                                MarketItem marketItem = Cache.Instance.GetMarketItem(mySellOrder.TypeId);

                                if (marketItem == null)
                                    continue;

                                DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == mySellOrder.StationId);
                                DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == mySellOrder.StationId);

                                if (lowestSellOrder != null && lowestSellOrder.Price <= mySellOrder.Price && lowestSellOrder.OrderId != mySellOrder.OrderId)
                                {
                                    double priceDifference = mySellOrder.Price - lowestSellOrder.Price;
                                    double priceDifferencePct = priceDifference / mySellOrder.Price;

                                    if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                    {
                                        sellOrders.Add(mySellOrder);
                                    }
                                    else if (highestBuyOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
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

                                DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == myBuyOrder.StationId);
                                DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == myBuyOrder.StationId);

                                if (highestBuyOrder != null && highestBuyOrder.Price >= myBuyOrder.Price && highestBuyOrder.OrderId != myBuyOrder.OrderId)
                                {
                                    double priceDifference = highestBuyOrder.Price - myBuyOrder.Price;
                                    double priceDifferencePct = priceDifference / myBuyOrder.Price;

                                    if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                                    {
                                        buyOrders.Add(myBuyOrder);
                                    }
                                    else if (lowestSellOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                                    {
                                        buyOrders.Add(myBuyOrder);
                                    }
                                }
                            }

                            _modifyAllOrders = new ModifyAllOrders(sellOrders, buyOrders);
                            _modifyAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
                            _modifyAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
                            _modifyAllOrders.OnModifyAllOrdersFinished += ModifyAllOrdersFinished;

                            _modifyAllOrders.Initialize();
                        }

                        _modifyAllOrders.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.CheckItemHanger:
                    try
                    {
                        if (_itemHanger == null)
                        {
                            Logging.Log("Automation:CheckItemHanger", "ModifyOrders State - Begin", Logging.Debug);

                            _itemHanger = new ItemHanger();
                            _itemHanger.OnItemHangerFinished += OnItemHangerFinished;
                            _itemHanger.OnItemHangerFinished += ItemHangerFinished;
                            _itemHanger.Initialize();
                        }

                        _itemHanger.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.CreateSellOrders:
                    try
                    {
                        if (_sellItems == null)
                        {
                            if (_itemsInHanger == null)
                                _state = State.Done;

                            Logging.Log("Automation:CreateSellOrders", "ModifyOrders State - Begin", Logging.Debug);

                            int orderCap = Cache.Instance.DirectEve.GetOrderCap();
                            int maxSellOrders = (int)((decimal)orderCap * 0.66m);

                            List<DirectItem> sellItemList = new List<DirectItem>();

                            foreach (DirectItem item in _itemsInHanger)
                            {
                                int typeId = item.TypeId;

                                DirectOrder order = _mySellOrders.FirstOrDefault(o => o.TypeId == typeId);

                                if (order == null)
                                {
                                    _newSellOrders++;

                                    if (_mySellOrders.Count + _newSellOrders <= maxSellOrders && _myBuyOrders.Count + _mySellOrders.Count + _newSellOrders <= orderCap)
                                    {
                                        if (item != null)
                                        {
                                            sellItemList.Add(item);
                                        }
                                    }
                                }
                            }

                            _sellItems = new SellItems(sellItemList);
                            _sellItems.OnSellItemFinished += OnSellItemFinished;
                            _sellItems.OnSellItemFinished += SellItemFinished;
                            _sellItems.OnSellItemsFinished += OnSellItemsFinished;
                            _sellItems.OnSellItemsFinished += SellItemsFinished;
                            _sellItems.Initialize();
                        }

                        _sellItems.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.CreateBuyOrders:
                    try
                    {
                        if (_buyItems == null)
                        {
                            Logging.Log("Automation:CreateBuyOrders", "ModifyOrders State - Begin", Logging.Debug);

                            int orderCap = Cache.Instance.DirectEve.GetOrderCap();

                            int maxBuyOrders = (int)((decimal)orderCap * 0.66m);

                            Dictionary<int, int> ordersToCreate = new Dictionary<int, int>();

                            string[] allLines = File.ReadAllLines("C:\\Users\\Tim\\Documents\\GitHub\\OmniEve\\output\\BuyOrders.txt");

                            foreach (string line in allLines)
                            {
                                try
                                {
                                    string[] parameters = line.Split(',');

                                    int typeId = int.Parse(parameters[0]);
                                    int volume = int.Parse(parameters[2]);
                                    double buyPrice = double.Parse(parameters[3]);

                                    volume = volume / 2;
                                    
                                    if (volume * buyPrice > 100000000)
                                        volume = (int)(10000000.0 / buyPrice);

                                    if (volume <= 0)
                                        volume = 1;

                                    DirectOrder order = _myBuyOrders.FirstOrDefault(o => o.TypeId == typeId);
                                    DirectItem item = _itemsInHanger.FirstOrDefault(i => i.TypeId == typeId);

                                    if (order == null && item == null)
                                    {
                                        _newBuyOrders++;

                                        if (_myBuyOrders.Count + _newBuyOrders <= maxBuyOrders && _myBuyOrders.Count + _mySellOrders.Count + _newBuyOrders + _newSellOrders <= (orderCap - 5))
                                        {
                                            Logging.Log("Automation:Process", "Adding type to create buy order for TypeId - " + typeId + " Volume - " + volume, Logging.Debug);
                                            ordersToCreate.Add(typeId, volume);
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                                }
                            }

                            _buyItems = new BuyItems(ordersToCreate, true);
                            _buyItems.OnBuyItemFinished += OnBuyItemFinished;
                            _buyItems.OnBuyItemFinished += BuyItemFinished;
                            _buyItems.OnBuyItemsFinished += OnBuyItemsFinished;
                            _buyItems.OnBuyItemsFinished += BuyItemsFinished;
                            _buyItems.Initialize();
                        }

                        _buyItems.Process();
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;
            }
        }

        private void MyOrdersFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders)
        {
            _mySellOrders = mySellOrders;
            _myBuyOrders = myBuyOrders;

            _state = State.MarketInfo;

            Logging.Log("Automation:Process", "MyOrders State - End", Logging.Debug);
        }

        private void MarketInfoForListFinished()
        {
            _state = State.ModifyOrders;

            Logging.Log("Automation:Process", "MarketInfo State - End", Logging.Debug);
        }

        private void ModifyAllOrdersFinished()
        {
            _state = State.CheckItemHanger;

            Logging.Log("Automation:Process", "ModifyOrders State - End", Logging.Debug);
        }

        private void ItemHangerFinished(List<DirectItem> hangerItems)
        {
            _itemsInHanger = hangerItems;

            _state = State.CreateSellOrders;

            Logging.Log("Automation:Process", "CheckItemHanger State - End", Logging.Debug);
        }

        private void SellItemFinished(DirectItem item, bool sold)
        {
        }

        private void SellItemsFinished(List<DirectItem> itemsSold)
        {
            _state = State.CreateBuyOrders;

            Logging.Log("Automation:Process", "CreateSellOrders State - End", Logging.Debug);
        }

        private void BuyItemFinished(int typeId, bool orderCreated)
        {
        }

        private void BuyItemsFinished(List<int> typeIdsBought, bool ordersCreated)
        {
            _state = State.Done;

            Logging.Log("Automation:Process", "CreateBuyOrders State - End", Logging.Debug);
        }
    }
}
