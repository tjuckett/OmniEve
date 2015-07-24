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

    public class ItemHanger : IScript
    {
        public delegate void ItemHangerActionFinished(List<DirectItem> _hangerItems);
        public event ItemHangerActionFinished OnItemHangerActionFinished;

        private DateTime _lastAction;
        private ItemHangerState _state = ItemHangerState.Idle;
        private bool _done = false;
        private List<DirectItem> _hangerItems;

        public void Initialize()
        {
            _state = ItemHangerState.Begin;
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

            switch (_state)
            {
                case ItemHangerState.Idle:
                    break;
                case ItemHangerState.Done:
                    if (OnItemHangerActionFinished != null)
                        OnItemHangerActionFinished(_hangerItems);

                    _done = true;
                    break;

                case ItemHangerState.Begin:

                    _hangerItems = new List<DirectItem>();

                    // Don't close the market window if its already up
                    _state = ItemHangerState.LoadItems;
                    break;

                case ItemHangerState.LoadItems:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;
                    
                    DirectContainer itemHanger = Cache.Instance.DirectEve.GetItemHangar();
                    foreach(DirectItem item in itemHanger.Items)
                    {
                        Logging.Log("Inventory:Process", "Item loaded Name - " + item.Name, Logging.White);
                        _hangerItems.Add(item);
                    }

                    _state = ItemHangerState.Done;

                    break;
            }
        }
    }
}
