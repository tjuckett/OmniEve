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

    public class SellItem : IAction
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            OpenMarket,
            LoadItem,
            GetPrice,
            OpenSellWindow,
            SetPrice,
            SellItem
        }

        public delegate void SellItemFinished(DirectItem item, bool sold);
        public event SellItemFinished OnSellItemFinished;

        private DirectItem _item = null;
        private bool _createOrder = false;
        private bool _done = false;
        private bool _sold = false;
        private int _sellWindowNotReadyCount = 0;
        private double _price = 0.0f;

        private DateTime _lastAction;
        private State _state = State.Idle;

        public SellItem(DirectItem item, bool createOrder)
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
            DirectSellMultiWindow sellWindow = Cache.Instance.DirectEve.Windows.OfType<DirectSellMultiWindow>().FirstOrDefault();

            switch (_state)
            {
                case State.Idle:
                    break;
                case State.Done:
                    _done = true;

                    if (OnSellItemFinished != null)
                        OnSellItemFinished(_item, _sold);
                    break;

                case State.Begin:
                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();

                    _state = State.OpenMarket;
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
                        Logging.Log("SellItem:Process", "Market window is not ready", Logging.White);
                        break;
                    }

                    Logging.Log("SellItem:Process", "Opening Market", Logging.White);
                    _state = State.LoadItem;

                    break;

                case State.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        Logging.Log("SellItem:Process", "Load orders for TypeId - " + _item.TypeId.ToString(), Logging.White);

                        if (marketWindow.DetailTypeId != _item.TypeId)
                        {
                            if(marketWindow.LoadTypeId(_item.TypeId))
                                _state = State.GetPrice;
                        }

                        break;
                    }
                    else
                    {
                        Logging.Log("SellItem:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.GetPrice:
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                        {
                            Logging.Log("SellItem:Process", "Market window is not ready", Logging.White);
                            break;
                        }

                        if (marketWindow.DetailTypeId != _item.TypeId)
                        {
                            _state = State.LoadItem;
                            break;
                        }

                        List<DirectOrder> sellOrders = marketWindow.SellOrders.OrderBy(o => o.Price).Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();
                        List<DirectOrder> buyOrders = marketWindow.BuyOrders.OrderByDescending(o => o.Price).Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();
                        DirectOrder firstSellOrder = sellOrders.FirstOrDefault();
                        DirectOrder firstBuyOrder = buyOrders.FirstOrDefault();

                        if (firstSellOrder != null)
                        {
                            sellOrders.Remove(firstSellOrder);
                            DirectOrder secondSellOrder = sellOrders.FirstOrDefault();

                            if (secondSellOrder != null)
                            {
                                decimal priceDifference = decimal.Parse(secondSellOrder.Price.ToString()) - decimal.Parse(firstSellOrder.Price.ToString());
                                decimal priceDifferencePct = priceDifference / decimal.Parse(firstSellOrder.Price.ToString());
                                if (priceDifferencePct > 0.05m || priceDifference > 5000000)
                                {
                                    // Check if the first buy order is close enough to the sell order that we no longer want to sell, otherwise the jump between the two sell orders doesn't matter.
                                    // If there is no first buy order then create the order anyway
                                    if (firstBuyOrder != null && firstSellOrder.Price / firstBuyOrder.Price < 1.5)
                                    {
                                        Logging.Log("SellItem:Process", "No sale, price difference between the first two orders is too high Pct - " + priceDifferencePct + " Diff - " + priceDifference, Logging.White);
                                        _state = State.Done;
                                        break;
                                    }
                                }
                            }

                            _price = double.Parse((decimal.Parse(firstSellOrder.Price.ToString()) - 0.01m).ToString());
                            _state = State.OpenSellWindow;

                            Logging.Log("SellItem:Process", "Lowest sell price, Name - " + _item.Name + " Price - " + _price, Logging.White);
                        }
                        else
                        {
                            Logging.Log("SellItem:Process", "No current sell orders, can't create lowest order for Name - " + _item.Name, Logging.White);
                            _state = State.Done;
                        }
                    }
                    else
                    {
                        Logging.Log("SellItem:Process", "MarketWindow is not open, going back to open market state", Logging.White);

                        _state = State.OpenMarket;
                    }

                    break;

                case State.OpenSellWindow:

                    _sellWindowNotReadyCount = 0;

                    if (Cache.Instance.DirectEve.OpenSellItems(_item) == true)
                    {
                        Logging.Log("SellItem:Process", "Opening sell window for Name - " + _item.Name, Logging.White);
                        if (_createOrder)
                            _state = State.SetPrice;
                        else
                            _state = State.SellItem;
                    }
                    else
                    {
                        Logging.Log("SellItem:Process", "Failed to open sell window for Name - " + _item.Name, Logging.White);
                        _state = State.Done;
                    }

                    break;

                case State.SetPrice:
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 1)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (sellWindow != null)
                    {
                        if(!sellWindow.IsReady)
                        {
                            _sellWindowNotReadyCount++;
                            Logging.Log("SellItem:Process", "Sell window is not ready", Logging.White);

                            if (_sellWindowNotReadyCount >= 5)
                            {
                                Logging.Log("SellItem:Process", "Something is wrong trying to sell this item, quitting the sell", Logging.White);
                                _state = State.Done;
                            }

                            break;
                        }

                        sellWindow.SetPrice(_item.ItemId, _price);

                        Logging.Log("SellItem:Process", "Sell window setting price to " + _price, Logging.White);
                        _state = State.SellItem;
                    }
                    else
                    {
                        Logging.Log("SellItem:Process", "Sell Window is not open going back to open sell window state", Logging.White);
                        _state = State.OpenSellWindow;
                    }

                    break;

                case State.SellItem:
                    if (sellWindow != null)
                    {
                        if (!sellWindow.IsReady)
                        {
                            _sellWindowNotReadyCount++;
                            Logging.Log("SellItem:Process", "Sell window is not ready", Logging.White);

                            if (_sellWindowNotReadyCount >= 5)
                                _state = State.OpenSellWindow;

                            break;
                        }

                        sellWindow.SellItems();

                        Logging.Log("SellItem:Process", "Selling item Name - " + _item.Name, Logging.White);

                        _sold = true;

                        _state = State.Done;
                    }
                    else
                    {
                        Logging.Log("SellItem:Process", "Sell Window is not open going back to open sell window state", Logging.White);
                        _state = State.OpenSellWindow;
                    }

                    break;
            }
        }
    }
}
