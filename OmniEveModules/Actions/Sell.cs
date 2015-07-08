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

    public class Sell : IAction
    {
        public int Item { get; set; }
        public int Unit { get; set; }
        public bool UseOrders { get; set; }

        private DateTime _lastAction;
        private SellState _state = SellState.Idle;

        public void Initialize()
        {
            _state = SellState.Begin;
        }

        public bool IsDone()
        {
            return _state == SellState.Done;
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
                case SellState.Idle:
                case SellState.Done:
                    break;

                case SellState.Begin:
                    if (UseOrders)
                    {
                        // Close the market window if there is one
                        if (marketWindow != null)
                            marketWindow.Close();

                        _state = SellState.OpenMarket;
                    }
                    else
                    {
                        _state = SellState.StartQuickSell;
                    }
                    break;

                case SellState.OpenMarket:

                    if (marketWindow == null)
                    {
                        _lastAction = DateTime.UtcNow;

                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    Logging.Log("Sell:Process", "Opening Market", Logging.White);
                    _state = SellState.LoadItem;

                    break;

                case SellState.LoadItem:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (marketWindow.DetailTypeId != Item)
                        {
                            marketWindow.LoadTypeId(Item);
                            _state = SellState.CreateOrder;
                            break;
                        }
                    }

                    break;

                case SellState.CreateOrder:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        List<DirectOrder> orders = marketWindow.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).ToList();

                        DirectOrder order = orders.OrderByDescending(o => o.Price).LastOrDefault();
                        if (order != null)
                        {
                            double price = order.Price - 0.01;
                            if (Cache.Instance.DirectEve.Session.StationId != null)
                            {
                                //Cache.Instance.DirectEve.Sell(Item, (int)Cache.Instance.DirectEve.Session.StationId, Unit, price, 0, false);
                            }
                        }
                        UseOrders = false;
                        _state = SellState.Done;
                    }

                    break;

                case SellState.StartQuickSell:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 1)
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
                    }

                    _state = SellState.WaitForSellWindow;
                    break;

                case SellState.WaitForSellWindow:

                    //if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != Item)
                    //    break;

                    // Mark as new execution
                    _lastAction = DateTime.UtcNow;

                    Logging.Log("Sell:Process", "Inspecting sell order for " + Item, Logging.White);
                    _state = SellState.InspectOrder;
                    break;

                case SellState.InspectOrder:
                    // Let the order window stay open for 2 seconds
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    if (sellWindow != null)
                    {
                        if ((!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue))
                        {
                            Logging.Log("Sell:Process", "No order available for " + Item, Logging.White);

                            sellWindow.Cancel();
                            _state = SellState.WaitingToFinishQuickSell;
                            break;
                        }

                        double price = sellWindow.Price.Value;

                        Logging.Log("Sell:Process", "Selling " + Unit + " of " + Item + " [Sell price: " + (price * Unit).ToString("#,##0.00") + "]", Logging.White);
                        sellWindow.Accept();
                        _state = SellState.WaitingToFinishQuickSell;
                    }
                    _lastAction = DateTime.UtcNow;
                    break;

                case SellState.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != Item)
                    {
                        DirectWindow modal = Cache.Instance.DirectEve.Windows.FirstOrDefault(w => w.IsModal);
                        if (modal != null)
                            modal.Close();

                        _state = SellState.Done;
                        break;
                    }
                    break;
            }
        }
    }
}
