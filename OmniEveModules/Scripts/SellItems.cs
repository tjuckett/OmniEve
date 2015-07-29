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

    public class SellItems : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            PopNext,
            Process
        }

        public delegate void SellItemsFinished(List<DirectItem> itemsSold);
        public event SellItemsFinished OnSellItemsFinished;

        public event SellItem.SellItemFinished OnSellItemFinished;

        private State _state = State.Idle;
        private bool _done = false;

        private List<DirectItem> _items = null;
        private List<DirectItem> _itemsSold = new List<DirectItem>();
        private List<SellItem> _sellItems = new List<SellItem>();
        private SellItem _currentSell = null;
        
        public SellItems(List<DirectItem> items)
        {
            _items = items;
        }

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
                    if (OnSellItemsFinished != null)
                        OnSellItemsFinished(_itemsSold);

                    _done = true;
                    break;

                case State.Begin:

                    foreach(DirectItem item in _items)
                    {
                        SellItem sellItem = new SellItem(item, true);
                        sellItem.OnSellItemFinished += SellFinished;
                        sellItem.OnSellItemFinished += OnSellItemFinished;
                        _sellItems.Add(sellItem);
                    }

                    _state = State.PopNext;
                    break;

                case State.PopNext:
                    _currentSell = _sellItems.FirstOrDefault();

                    if (_currentSell != null)
                    {
                        _sellItems.Remove(_currentSell);

                        Logging.Log("SellItems:Process", "Popping next sell script to run", Logging.White);

                        _currentSell.Initialize();
                        _state = State.Process;
                    }
                    else
                    {
                        Logging.Log("SellItems:Process", "No more sell scripts left, going to done state", Logging.White);
                        _state = State.Done;
                    }
                    break;

                case State.Process:
                    if (_currentSell != null)
                    {
                        _currentSell.Process();

                        // If the current script is done then pop the next one
                        if (_currentSell.IsDone() == true)
                        {
                            Logging.Log("SellItems:Process", "Sell script is done, executing callback and popping next", Logging.White);
                            _state = State.PopNext;
                        }
                    }

                    break;
            }
        }

        private void SellFinished(DirectItem item)
        {
            _itemsSold.Add(item);
        }
    }
}
