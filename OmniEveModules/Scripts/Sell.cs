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

    public class Sell : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadItem,
            CreateOrder,
            StartQuickSell,
            WaitForSellWindow,
            InspectOrder,
            WaitingToFinishQuickSell,
        }

        public delegate void SellFinished(DirectItem item);
        public event SellFinished OnSellFinished;

        private DirectItem _item = null;
        private bool _createOrder = false;
        private bool _done = false;

        private DateTime _lastAction;
        private State _state = State.Idle;

        public Sell(DirectItem item, bool createOrder)
        {
            _item = item;
            _createOrder = createOrder;
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
            DirectMarketActionWindow sellWindow = Cache.Instance.DirectEve.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);

            switch (_state)
            {
                case State.Idle:
                    break;
                case State.Done:
                    if (OnSellFinished != null)
                        OnSellFinished(_item);

                    _done = true;
                    break;

                case State.Begin:
                    if (_createOrder)
                    {
                        // Close the market window if there is one
                        if (marketWindow != null)
                            marketWindow.Close();

                        _state = State.OpenMarket;
                    }
                    else
                    {
                        _state = State.StartQuickSell;
                    }
                    break;

                case State.OpenMarket:

                    if (marketWindow == null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                    }

                    if (!marketWindow.IsReady)
                    {
                        Logging.Log("Sell:Process", "Market window is not ready", Logging.White);
                        break;
                    }

                    Logging.Log("Sell:Process", "Opening Market", Logging.White);
                    _state = State.LoadItem;

                    break;

                case State.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("Sell:Process", "Load orders for TypeId - " + _item.TypeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != _item.TypeId)
                        {
                            if(marketWindow.LoadTypeId(_item.TypeId))
                                _state = State.CreateOrder;
                        }

                        break;
                    }
                    else
                    {
                        Logging.Log("Sell:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.CreateOrder:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        DirectOrder order = marketWindow.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == Cache.Instance.DirectEve.Session.StationId);
                        
                        if (order != null)
                        {
                            double price = double.Parse((decimal.Parse(order.Price.ToString()) - 0.01m).ToString());

                            if (Cache.Instance.DirectEve.Session.StationId != null)
                            {
                                Logging.Log("Sell:Process", "Create order for Name - " + _item.Name + " Price - " + price.ToString() + " Quantity - " + _item.Quantity, Logging.White);
                                if(Cache.Instance.DirectEve.Sell(_item, (int)Cache.Instance.DirectEve.Session.StationId, _item.Quantity, price, 90, false) == true)
                                    Logging.Log("Sell:Process", "Successfully created order for Name - " + _item.Name, Logging.White);
                                else
                                    Logging.Log("Sell:Process", "Failed creating order for Name - " + _item.Name, Logging.White);
                            }
                        }
                        _state = State.Done;
                    }

                    break;

                case State.StartQuickSell:

                    /*if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 1)
                        break;
                    _lastAction = DateTime.UtcNow;

                    if (Cache.Instance.ItemHangar == null) break;

                    DirectItem directItem = Cache.Instance.ItemHangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                    if (directItem == null)
                    {
                        Logging.Log("Sell:Process", "Item " + Item + " no longer exists in the hanger", Logging.White);
                        break;
                    }

                    // Update Quantity
                    if (Unit == 00)
                        Unit = directItem.Quantity;

                    Logging.Log("Sell:Process", "Starting QuickSell for " + Item, Logging.White);
                    if (!directItem.QuickSell())
                    {
                        _lastAction = DateTime.UtcNow.AddSeconds(-5);

                        Logging.Log("Sell:Process", "QuickSell failed for " + Item + ", retrying in 5 seconds", Logging.White);
                        break;
                    }*/

                    _state = State.WaitForSellWindow;
                    break;

                case State.WaitForSellWindow:

                    //if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != Item)
                    //    break;

                    // Mark as new execution
                    _lastAction = DateTime.UtcNow;

                    Logging.Log("Sell:Process", "Inspecting sell order for " + _item.TypeId, Logging.White);
                    _state = State.InspectOrder;
                    break;

                case State.InspectOrder:
                    // Let the order window stay open for 2 seconds
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    if (sellWindow != null)
                    {
                        if ((!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue))
                        {
                            Logging.Log("Sell:Process", "No order available for " + _item.TypeId, Logging.White);

                            sellWindow.Cancel();
                            _state = State.WaitingToFinishQuickSell;
                            break;
                        }

                        double price = sellWindow.Price.Value;

                        Logging.Log("Sell:Process", "Selling " + _item.Volume + " of " + _item.TypeId + " [Sell price: " + (price * _item.Volume).ToString("#,##0.00") + "]", Logging.White);
                        sellWindow.Accept();
                        _state = State.WaitingToFinishQuickSell;
                    }
                    _lastAction = DateTime.UtcNow;
                    break;

                case State.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _item.ItemId)
                    {
                        DirectWindow modal = Cache.Instance.DirectEve.Windows.FirstOrDefault(w => w.IsModal);
                        if (modal != null)
                            modal.Close();

                        _state = State.Done;
                        break;
                    }
                    break;
            }
        }
    }
}
