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
            SellOrders
        }

        public delegate void SellItemsFinished(List<DirectItem> itemsSold);
        public event SellItemsFinished OnSellItemsFinished;

        private State _state = State.Idle;
        private bool _done = false;

        private List<DirectItem> _items = null;
        private List<DirectItem> _itemsSold = new List<DirectItem>();
        private List<Sell> _sellItems = new List<Sell>();
        private Sell _currentSell = null;
        
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
                        Sell sellItem = new Sell(item, true);
                        sellItem.OnSellFinished += SellFinished;
                        _sellItems.Add(sellItem);
                    }

                    _state = State.SellOrders;
                    break;

                case State.SellOrders:
                    if (_currentSell == null)
                    {
                        _currentSell = PullNextSell();
                    }

                    if (_currentSell != null)
                        _currentSell.Process();
                    else
                        _state = State.Done;

                    _state = State.SellOrders;
                    break;
            }
        }

        private void SellFinished(DirectItem item)
        {
            _itemsSold.Add(item);

            _currentSell = PullNextSell();

            if (_currentSell == null)
                _state = State.SellOrders;
        }

        private Sell PullNextSell()
        {
            Sell sell = _sellItems.FirstOrDefault();

            if (sell == null)
                return null;

            sell.Initialize();

            _sellItems.Remove(sell);

            return sell;
        }
    }
}
