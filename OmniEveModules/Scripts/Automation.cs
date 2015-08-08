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
            UpdateOrders,
            CheckItemHanger,
            CreateSellOrders,
            ProcessSellOrders,
            CreateBuyOrders,
            ProcessBuyOrders
        }

        public delegate void AutomationFinished();
        public event AutomationFinished OnAutomationFinished;

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event MyOrders.MyOrdersFinished OnMyOrdersFinished;

        public event MarketInfo.MarketInfoFinished OnMarketInfoFinished;

        public event ItemHanger.ItemHangerFinished OnItemHangerFinished;

        public event SellItem.SellItemFinished OnSellItemFinished;
        public event BuyItem.BuyItemFinished OnBuyItemFinished;

        private List<DirectOrder> _mySellOrders { get; set; }
        private List<DirectOrder> _myBuyOrders { get; set; }
        private List<DirectItem> _itemsInHanger { get; set; }

        private bool _done = false;
        private State _state = State.Idle;
        private int _newSellOrders = 0;
        private int _newBuyOrders = 0;

        private MyOrders _myOrders = null;
        private UpdateAllOrders _updateAllOrders = null;
        private MarketInfoForList _marketInfoForList = null;
        private ItemHanger _itemHanger = null;
        private BuyItem _buyItem = null;
        private SellItem _sellItem = null;

        private List<SellItem> _sellItems = new List<SellItem>();
        private List<BuyItem> _buyItems = new List<BuyItem>();

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
                    _done = true;

                    if (OnAutomationFinished != null)
                        OnAutomationFinished();
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

                case State.UpdateOrders:
                    try
                    {
                        if (_updateAllOrders == null)
                        {
                            Logging.Log("Automation:Process", "UpdateOrders State - Begin", Logging.Debug);

                            _updateAllOrders = new UpdateAllOrders(_mySellOrders, _myBuyOrders);
                            _updateAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
                            _updateAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
                            _updateAllOrders.OnUpdateAllOrdersFinished += UpdateAllOrdersFinished;

                            _updateAllOrders.Initialize();
                        }

                        _updateAllOrders.Process();
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
                            Logging.Log("Automation:Process", "CheckItemHanger State - Begin", Logging.Debug);

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
                        if (_itemsInHanger == null)
                            _state = State.Done;

                        Logging.Log("Automation:Process", "CreateSellOrders State - Begin", Logging.Debug);

                        List<DirectItem> sellItemList = new List<DirectItem>();

                        foreach (DirectItem item in _itemsInHanger)
                        {
                            if (item == null)
                                continue;

                            int typeId = item.TypeId;

                            DirectOrder order = _mySellOrders.FirstOrDefault(o => o.TypeId == typeId);

                            if (order == null)
                            {
                                SellItem sellItem = new SellItem(item, true);
                                sellItem.OnSellItemFinished += OnSellItemFinished;
                                sellItem.OnSellItemFinished += SellItemFinished;

                                _sellItems.Add(sellItem);
                            }
                        }
                        
                        _state = State.ProcessSellOrders;

                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.ProcessSellOrders:
                    try
                    {
                        Logging.Log("Automation:Process", "ProcessSellOrders State - Begin", Logging.Debug);

                        int orderCap = Cache.Instance.DirectEve.GetOrderCap();
                        int maxSellOrders = (int)((decimal)orderCap * 0.66m);

                        if(_sellItem == null)
                        {
                            if (_mySellOrders.Count + _newSellOrders <= maxSellOrders && _myBuyOrders.Count + _mySellOrders.Count + _newSellOrders <= orderCap)
                            {
                                _sellItem = _sellItems.FirstOrDefault();

                                if (_sellItem != null)
                                {
                                    _sellItems.Remove(_sellItem);

                                    Logging.Log("Automation:Process", "Popping next sell script to run", Logging.White);

                                    _sellItem.Initialize();
                                }
                                else
                                {
                                    Logging.Log("Automation:Process", "No more sell scripts left, going to create buy orders state", Logging.White);
                                    _state = State.CreateBuyOrders;
                                }
                            }
                            else
                            {
                                Logging.Log("Automation:Process", "Hit max number of sell orders allowed, going to create buy orders state", Logging.White);
                                _sellItems.Clear();
                                _state = State.CreateBuyOrders;
                            }
                        }

                        if(_sellItem != null)
                            _sellItem.Process();
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
                        _buyItems.Clear();

                        Logging.Log("Automation:Process", "CreateBuyOrders State - Begin", Logging.Debug);

                        string[] allLines = File.ReadAllLines("C:\\Users\\tjuckett\\Documents\\GitHub\\OmniEve\\output\\BuyOrders.txt");

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
                                    Logging.Log("Automation:Process", "Adding type to create buy order for TypeId - " + typeId + " Volume - " + volume, Logging.Debug);
                                    BuyItem buyItem = new BuyItem(typeId, volume, true);
                                    buyItem.OnBuyItemFinished += OnBuyItemFinished;
                                    buyItem.OnBuyItemFinished += BuyItemFinished;
                                    _buyItems.Add(buyItem);
                                }
                            }
                            catch(Exception ex)
                            {
                                Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                            }
                        }

                        _state = State.ProcessBuyOrders;
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("Automation:Process", "Exception [" + ex + "]", Logging.Debug);
                        _state = State.Done;
                    }
                    break;

                case State.ProcessBuyOrders:
                    try
                    {
                        Logging.Log("Automation:Process", "ProcessBuyOrders State - Begin", Logging.Debug);

                        int orderCap = Cache.Instance.DirectEve.GetOrderCap();
                        int maxBuyOrders = (int)((decimal)orderCap * 0.66m);

                        if(_buyItem == null)
                        {
                            if (_myBuyOrders.Count + _newBuyOrders <= maxBuyOrders && _myBuyOrders.Count + _mySellOrders.Count + _newBuyOrders + _newSellOrders <= (orderCap - 5) && _buyItems.Count > 0)
                            {
                                _buyItem = _buyItems.FirstOrDefault();

                                if (_buyItem != null)
                                {
                                    _buyItems.Remove(_buyItem);

                                    Logging.Log("Automation:Process", "Popping next buy script to run", Logging.White);

                                    _buyItem.Initialize();
                                }
                                else
                                {
                                    Logging.Log("Automation:Process", "No more buy scripts left, going to done state", Logging.White);
                                    _state = State.Done;
                                }
                            }
                            else
                            {
                                Logging.Log("Automation:Process", "Hit max number of buy orders allowed, going to done state", Logging.White);
                                _buyItems.Clear();
                                _state = State.Done;
                            }
                        }

                        if (_buyItem != null)
                            _buyItem.Process();
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
            _state = State.UpdateOrders;

            Logging.Log("Automation:Process", "MarketInfo State - End", Logging.Debug);
        }

        private void UpdateAllOrdersFinished()
        {
            _state = State.CheckItemHanger;

            Logging.Log("Automation:Process", "UpdateOrders State - End", Logging.Debug);
        }

        private void ItemHangerFinished(List<DirectItem> hangerItems)
        {
            _itemsInHanger = hangerItems;

            _state = State.CreateSellOrders;

            Logging.Log("Automation:Process", "CheckItemHanger State - End", Logging.Debug);
        }

        private void SellItemFinished(DirectItem item, bool sold)
        {
            if(sold == true)
                _newSellOrders++;

            _sellItem = null;
        }

        private void BuyItemFinished(int typeId, bool orderCreated)
        {
            if (orderCreated == true)
                _newBuyOrders++;

            _buyItem = null;
        }
    }
}
