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

    public class Inventory : IAction
    {
        public delegate void InventoryActionFinished();
        public event InventoryActionFinished OnInventoryActionFinished;

        private DateTime _lastAction;
        private InventoryState _state = InventoryState.Idle;
        private bool _done = false;

        public void Initialize()
        {
            _state = InventoryState.Begin;
        }

        public bool IsDone()
        {
            return _done;
        }

        public void Process()
        {
            if (!Status.Instance.InStation)
                return;

            if (Status.Instance.InSpace)
                return;

            DirectContainerWindow inventoryWindow = Cache.Instance.DirectEve.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w=>w.Name.Contains("Inventory"));

            switch (_state)
            {
                case InventoryState.Idle:
                    break;
                case InventoryState.Done:
                    if (OnInventoryActionFinished != null)
                        OnInventoryActionFinished();

                    _done = true;
                    break;

                case InventoryState.Begin:

                    // Don't close the market window if its already up
                    _state = InventoryState.OpenInventory;
                    break;

                case InventoryState.OpenInventory:

                    _state = InventoryState.LoadItems;
                    break;

                case InventoryState.LoadItems:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                        //Logging.Log("Inventory:Process", "Load orders for TypeId - " + TypeId.ToString(), Logging.White);

                    DirectContainer itemHanger = Cache.Instance.DirectEve.GetItemHangar();
                    foreach(DirectItem item in itemHanger.Items)
                    {
                        Logging.Log("Inventory:Process", "Item loaded Name - " + item.Name, Logging.White);
                    }

                    _state = InventoryState.Done;

                    break;
                
                /*case MarketInfoState.CacheInfo:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;

                    if (marketWindow != null)
                    {
                        if (!marketWindow.IsReady)
                        {
                            Logging.Log("Inventory:Process", "Market window is not ready", Logging.White);
                            break;
                        }

                        if (marketWindow.DetailTypeId != TypeId)
                            _state = MarketInfoState.LoadItem;

                        Logging.Log("Inventory:Process", "Get list of orders for Item - " + TypeId.ToString(), Logging.White);

                        _marketInfoItem = new MarketItemInfo();

                        if (_marketInfoItem != null)
                        {
                            Logging.Log("Inventory:Process", "Get list of orders successful", Logging.White);

                            _marketInfoItem.SellOrders = marketWindow.SellOrders;
                            _marketInfoItem.BuyOrders = marketWindow.BuyOrders;
                            _marketInfoItem.TypeId = TypeId;

                            _state = MarketInfoState.Done;
                        }
                    }
                    else
                    {
                        _state = MarketInfoState.OpenMarket;
                    }
                    break;*/
            }
        }
    }
}
