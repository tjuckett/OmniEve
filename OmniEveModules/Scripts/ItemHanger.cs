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
        public enum State
        {
            Idle,
            Done,
            Begin,
            LoadHanger,
            CompareMyOrders
        }

        public delegate void ItemHangerFinished(List<DirectItem> hangerItems);
        public event ItemHangerFinished OnItemHangerFinished;

        private DateTime _lastAction;
        private State _state = State.Idle;
        private bool _done = false;
        private List<DirectItem> _hangerItems;

        public void Initialize()
        {
            _state = State.Begin;
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
                case State.Idle:
                    break;
                case State.Done:
                    _done = true;

                    if (OnItemHangerFinished != null)
                        OnItemHangerFinished(_hangerItems);
                    break;

                case State.Begin:

                    _hangerItems = new List<DirectItem>();

                    _state = State.LoadHanger;
                    break;

                case State.LoadHanger:
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    _lastAction = DateTime.UtcNow;
                    
                    DirectContainer itemHanger = Cache.Instance.DirectEve.GetItemHangar();
                    Cache.Instance.ItemHanger = itemHanger;
                    foreach(DirectItem item in itemHanger.Items)
                    {
                        Logging.Log("Inventory:Process", "Item loaded Name - " + item.Name, Logging.White);
                        _hangerItems.Add(item);
                    }

                    _state = State.Done;
                    break;
            }
        }
    }
}
