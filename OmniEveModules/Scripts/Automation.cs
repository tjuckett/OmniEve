using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    using DirectEve;
    using OmniEveModules.Actions;
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;
    using OmniEveModules.States;
    using OmniEveModules.Status;

    public class Automation : IScript
    {
        enum State
        {
            Done,
            Processing,
            MyOrders,
            MarketInfo,
            UpdateOrders,
            ItemHanger,
            CreateSellOrders,
            CreateBuyOrders,
        }

        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event MyOrders.MyOrdersFinished OnMyOrdersFinished;

        public event MarketInfo.MarketInfoFinished OnMarketInfoFinished;

        public event ItemHanger.ItemHangerFinished OnItemHangerFinished;

        public event SellItem.SellItemFinished OnSellItemFinished;
        public event BuyItem.BuyItemFinished OnBuyItemFinished;

        private object _stateLock = new object();

        private volatile State _state = State.MyOrders;
        private volatile int _newSellOrders = 0;
        private volatile int _newBuyOrders = 0;

        private List<DirectOrder> _mySellOrders = null;
        private List<DirectOrder> _myBuyOrders = null;
        private List<DirectItem> _itemsInHanger = null;
        private List<SellItem> _sellActions = new List<SellItem>();
        private List<BuyItem> _buyActions = new List<BuyItem>();
        private List<MarketInfo> _marketActions = new List<MarketInfo>();

        public override void DoWork(params object[] arguments)
        {
            try
            {
                while (_state != State.Done)
                {
                    switch (_state)
                    {
                        case State.Processing:
                            break;
                        case State.MyOrders:
                            RunMyOrders();
                            break;
                        case State.MarketInfo:
                            RunMarketInfo();
                            break;
                        case State.UpdateOrders:
                            RunUpdateOrders();
                            break;
                        case State.ItemHanger:
                            RunItemHanger();
                            break;
                        case State.CreateSellOrders:
                            RunCreateSellOrders();
                            break;
                        case State.CreateBuyOrders:
                            RunCreateBuyOrders();
                            break;
                    }
                }   
            }
            catch (Exception ex)
            {
                Logging.Log("Automation:DoWork", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void ChangeState(State state)
        {
            lock(_stateLock)
            {
                _state = state;
            }
        }

        private void RunMyOrders()
        {
            Logging.Log("Automation:RunMyOrders", "MyOrders State - Begin", Logging.Debug);

            MyOrders myOrders = new MyOrders();
            myOrders.OnMyOrdersFinished += MyOrdersFinished;
            myOrders.OnMyOrdersFinished += OnMyOrdersFinished;
            RunAction(myOrders);
            ChangeState(State.Processing);
        }

        private void RunMarketInfo()
        {
            Logging.Log("Automation:RunMarketInfo", "MarketInfo State - Begin", Logging.Debug);

            List<int> typeIds = new List<int>();

            foreach (DirectOrder order in _mySellOrders)
                typeIds.Add(order.TypeId);

            foreach (DirectOrder order in _myBuyOrders)
            {
                if (typeIds.FirstOrDefault(o => o == order.TypeId) == 0)
                    typeIds.Add(order.TypeId);
            }

            foreach (int typeId in typeIds)
            {
                MarketInfo marketInfo = new MarketInfo(typeId);
                marketInfo.OnMarketInfoFinished += OnMarketInfoFinished;
                marketInfo.OnMarketInfoFinished += MarketInfoFinished;
                _marketActions.Add(marketInfo);
            }

            if (RunNextMarketAction() == true)
                ChangeState(State.Processing);
            else
                ChangeState(State.UpdateOrders);
        }

        private bool RunNextMarketAction()
        {
            MarketInfo marketInfo = _marketActions.FirstOrDefault();

            if (marketInfo != null)
            {
                Logging.Log("Automation:RunNextMarketAction", "Popping next market info action to run", Logging.White);
                _marketActions.Remove(marketInfo);
                RunAction(marketInfo);
                return true;
            }
            else
            {
                Logging.Log("Automation:RunNextMarketAction", "No more market actions left, going to update orders state", Logging.White);
            }

            return false;
        }

        private void RunUpdateOrders()
        {
            Logging.Log("Automation:RunUpdateOrders", "UpdateOrders State - Begin", Logging.Debug);

            UpdateAllOrders updateAllOrders = new UpdateAllOrders();
            updateAllOrders.DoActions += RunActions;
            updateAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
            updateAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
            updateAllOrders.ScriptCompleted += UpdateAllOrdersComplete;
            updateAllOrders.RunScriptAsync();

            ChangeState(State.Processing);
        }

        private void RunItemHanger()
        {
            Logging.Log("Automation:RunItemHanger", "CheckItemHanger State - Begin", Logging.Debug);

            ItemHanger itemHanger = new ItemHanger();
            itemHanger.OnItemHangerFinished += OnItemHangerFinished;
            itemHanger.OnItemHangerFinished += ItemHangerFinished;
            RunAction(itemHanger);

            ChangeState(State.Processing);
        }

        private void RunCreateSellOrders()
        {
            if (_itemsInHanger == null)
                ChangeState(State.Done);

            _sellActions.Clear();

            _newSellOrders = 0;

            Logging.Log("Automation:RunCreateSellOrders", "CreateSellOrders State - Begin", Logging.Debug);

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
                    _sellActions.Add(sellItem);
                }
            }

            if (RunNextSellAction() == true)
                ChangeState(State.Processing);
            else
                ChangeState(State.CreateBuyOrders);
        }

        private bool RunNextSellAction()
        {
            int orderCap = Cache.Instance.OrderCap;
            int maxSellOrders = (int)((decimal)orderCap * 0.66m);

            if (_mySellOrders.Count + _newSellOrders <= maxSellOrders && _myBuyOrders.Count + _mySellOrders.Count + _newSellOrders < orderCap)
            {
                SellItem sellItem = _sellActions.FirstOrDefault();

                if (sellItem != null)
                {
                    Logging.Log("Automation:RunNextSellItem", "Popping next sell script to run", Logging.White);
                    _sellActions.Remove(sellItem);
                    RunAction(sellItem);
                    return true;
                }
                else
                {
                    Logging.Log("Automation:RunNextSellItem", "No more sell scripts left, going to create buy orders state", Logging.White);
                }
            }
            else
            {
                Logging.Log("Automation:RunNextSellItem", "Hit max number of sell orders allowed, going to create buy orders state", Logging.White);
            }

            _sellActions.Clear();

            return false;
        }

        private void RunCreateBuyOrders()
        {
            _buyActions.Clear();

            _newBuyOrders = 0;

            Logging.Log("Automation:RunCreateBuyOrders", "CreateBuyOrders State - Begin", Logging.Debug);

            string[] allLines = File.ReadAllLines("C:\\Users\\Tim And Desiree\\Documents\\GitHub\\OmniEve\\output\\BuyOrders.txt");

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
                        Logging.Log("Automation:RunCreateBuyOrders", "Adding type to create buy order for TypeId - " + typeId + " Volume - " + volume, Logging.Debug);
                        BuyItem buyItem = new BuyItem(typeId, volume, true);
                        buyItem.OnBuyItemFinished += OnBuyItemFinished;
                        buyItem.OnBuyItemFinished += BuyItemFinished;
                        _buyActions.Add(buyItem);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("Automation:RunCreateBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }

            if (RunNextBuyAction() == true)
                ChangeState(State.Processing);
            else
                ChangeState(State.Done);
        }

        private bool RunNextBuyAction()
        {
            int orderCap = Cache.Instance.OrderCap;
            int maxBuyOrders = (int)((decimal)orderCap * 0.66m);

            if (_myBuyOrders.Count + _newBuyOrders <= maxBuyOrders && _myBuyOrders.Count + _mySellOrders.Count + _newBuyOrders + _newSellOrders < (orderCap - 5))
            {
                BuyItem buyItem = _buyActions.FirstOrDefault();

                if (buyItem != null)
                {
                    Logging.Log("Automation:RunNextBuyItem", "Popping next buy script to run", Logging.White);
                    _buyActions.Remove(buyItem);
                    RunAction(buyItem);
                    return true;
                }
                else
                {
                    Logging.Log("Automation:RunNextBuyItem", "No more buy scripts left, going to done state", Logging.White);
                }
            }
            else
            {
                Logging.Log("Automation:Process", "Hit max number of buy orders allowed, going to done state", Logging.White);
            }

            _buyActions.Clear();

            return false;
        }

        private void MyOrdersFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders)
        {
            _mySellOrders = mySellOrders;
            _myBuyOrders = myBuyOrders;

            ChangeState(State.MarketInfo);

            Logging.Log("Automation:Process", "MyOrders State - End", Logging.Debug);
        }

        private void MarketInfoFinished(MarketItem marketItem)
        {
            if (RunNextMarketAction() == false)
            {
                ChangeState(State.UpdateOrders);
                Logging.Log("Automation:Process", "MarketInfo State - End", Logging.Debug);
            }
        }

        private void UpdateAllOrdersComplete()
        {
            ChangeState(State.ItemHanger);

            Logging.Log("Automation:Process", "UpdateOrders State - End", Logging.Debug);
        }

        private void ItemHangerFinished(List<DirectItem> hangerItems)
        {
            _itemsInHanger = hangerItems;

            ChangeState(State.CreateSellOrders);

            Logging.Log("Automation:Process", "CheckItem State - End", Logging.Debug);
        }

        private void SellItemFinished(DirectItem item, bool sold)
        {
            if(sold == true)
                _newSellOrders++;

            if (RunNextSellAction() == false)
            {
                Logging.Log("Automation:Process", "CreateSellOrders State - End", Logging.Debug);
                ChangeState(State.CreateBuyOrders);
            }
        }

        private void BuyItemFinished(int typeId, bool orderCreated)
        {
            if (orderCreated == true)
                _newBuyOrders++;

            if (RunNextBuyAction() == false)
            {
                Logging.Log("Automation:Process", "CreateBuyOrders State - End", Logging.Debug);
                ChangeState(State.Done);
            }
        }
    }
}
