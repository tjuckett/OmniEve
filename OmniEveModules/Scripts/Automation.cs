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

        //public event UpdateOrder.ModifyOrderFinished OnModifySellOrderFinished;
        //public event UpdateOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event MyOrders.MyOrdersFinished OnMyOrdersFinished;

        public event UpdateOrder.UpdateOrderFinished OnUpdateOrderFinished;

        public event ItemHanger.ItemHangerFinished OnItemHangerFinished;

        public event SellItem.SellItemFinished OnSellItemFinished;
        public event BuyItem.BuyItemFinished OnBuyItemFinished;

        private object _stateLock = new object();

        private volatile State _state = State.MyOrders;
        private volatile int _newSellOrders = 0;
        private volatile int _newBuyOrders = 0;
        private bool _isDone = false;

        private List<DirectOrder> _mySellOrders = null;
        private List<DirectOrder> _myBuyOrders = null;
        private List<DirectItem> _itemsInHanger = null;
        private Queue<SellItem> _sellActions = new Queue<SellItem>();
        private Queue<BuyItem> _buyActions = new Queue<BuyItem>();
        private Queue<UpdateOrder> _updateOrderActions = new Queue<UpdateOrder>();

        public override bool IsDone()
        {
            return _isDone;
        }

        public override void OnFrame()
        {
            try
            {
                switch (_state)
                {
                    case State.Processing:
                        break;
                    case State.MyOrders:
                        RunMyOrders();
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
                    case State.Done:
                        _isDone = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logging.Log("Automation:DoWork", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void ChangeState(State state)
        {
            _state = state;
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

        private void RunUpdateOrders()
        {
            Logging.Log("Automation:RunUpdateOrders", "UpdateOrders State - Begin", Logging.Debug);

            List<int> typeIds = new List<int>();

            _updateOrderActions.Clear();

            foreach (DirectOrder order in _mySellOrders)
            {
                UpdateOrder updateOrder = new UpdateOrder(order.OrderId, false);
                updateOrder.OnUpdateOrderFinished += OnUpdateOrderFinished;
                updateOrder.OnUpdateOrderFinished += UpdateOrderFinished;
                _updateOrderActions.Enqueue(updateOrder);
            }

            foreach (DirectOrder order in _myBuyOrders)
            {
                UpdateOrder updateOrder = new UpdateOrder(order.OrderId, true);
                updateOrder.OnUpdateOrderFinished += OnUpdateOrderFinished;
                updateOrder.OnUpdateOrderFinished += UpdateOrderFinished;
                _updateOrderActions.Enqueue(updateOrder);
            }

            if (RunNextUpdateOrderAction() == true)
                ChangeState(State.Processing);
            else
                ChangeState(State.UpdateOrders);
        }

        private bool RunNextUpdateOrderAction()
        {
            UpdateOrder updateOrder = null;

            if (_updateOrderActions.Count > 0)
                updateOrder = _updateOrderActions.Dequeue();

            if (updateOrder != null)
            {
                Logging.Log("Automation:RunNextUpdateOrderAction", "Popping next update order action to run", Logging.White);
                RunAction(updateOrder);
                return true;
            }
            else
            {
                Logging.Log("Automation:RunNextMarketAction", "No more update order actions left, going ItemHanger to state", Logging.White);
            }

            return false;
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
                ChangeState(State.CreateBuyOrders);

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
                    _sellActions.Enqueue(sellItem);
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
                SellItem sellItem = null;

                if(_sellActions.Count > 0)
                    sellItem = _sellActions.Dequeue();

                if (sellItem != null)
                {
                    Logging.Log("Automation:RunNextSellItem", "Popping next sell script to run", Logging.White);
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
                        Logging.Log("Automation:RunCreateBuyOrders", "Adding type to create buy order for TypeId - " + typeId + " Volume - " + volume, Logging.Debug);
                        BuyItem buyItem = new BuyItem(typeId, volume, true);
                        buyItem.OnBuyItemFinished += OnBuyItemFinished;
                        buyItem.OnBuyItemFinished += BuyItemFinished;
                        _buyActions.Enqueue(buyItem);
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
                ChangeState(State.MyOrders);
        }

        private bool RunNextBuyAction()
        {
            int orderCap = Cache.Instance.OrderCap;
            int maxBuyOrders = (int)((decimal)orderCap * 0.66m);

            if (_myBuyOrders.Count + _newBuyOrders <= maxBuyOrders && _myBuyOrders.Count + _mySellOrders.Count + _newBuyOrders + _newSellOrders < (orderCap - 5))
            {
                BuyItem buyItem = null;

                if(_buyActions.Count > 0)
                    buyItem = _buyActions.Dequeue();

                if (buyItem != null)
                {
                    Logging.Log("Automation:RunNextBuyItem", "Popping next buy script to run", Logging.White);
                    RunAction(buyItem);
                    return true;
                }
                else
                {
                    Logging.Log("Automation:RunNextBuyItem", "No more buy scripts left, going to my orders state", Logging.White);
                }
            }
            else
            {
                Logging.Log("Automation:RunNextBuyAction", "Hit max number of buy orders allowed, going to my orders state", Logging.White);
            }

            _buyActions.Clear();

            return false;
        }

        private void MyOrdersFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders)
        {
            _mySellOrders = mySellOrders;
            _myBuyOrders = myBuyOrders;

            ChangeState(State.UpdateOrders);

            Logging.Log("Automation:MyOrdersFinished", "MyOrders State - End", Logging.Debug);
        }

        private void UpdateOrderFinished(long orderId)
        {
            if (RunNextUpdateOrderAction() == false)
            {
                ChangeState(State.ItemHanger);
                Logging.Log("Automation:UpdateOrdersFinished", "UpdateOrders State - End", Logging.Debug);
            }
        }

        private void ItemHangerFinished(List<DirectItem> hangerItems)
        {
            _itemsInHanger = hangerItems;

            ChangeState(State.CreateSellOrders);

            Logging.Log("Automation:ItemHangerFinished", "CheckItem State - End", Logging.Debug);
        }

        private void SellItemFinished(DirectItem item, bool sold)
        {
            if(sold == true)
                _newSellOrders++;

            if (RunNextSellAction() == false)
            {
                Logging.Log("Automation:SellItemFinished", "CreateSellOrders State - End", Logging.Debug);
                ChangeState(State.CreateBuyOrders);
            }
        }

        private void BuyItemFinished(int typeId, bool orderCreated)
        {
            if (orderCreated == true)
                _newBuyOrders++;

            if (RunNextBuyAction() == false)
            {
                Logging.Log("Automation:BuyItemFinished", "CreateBuyOrders State - End", Logging.Debug);
                ChangeState(State.MyOrders);
            }
        }
    }
}
