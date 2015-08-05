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

    public class BuyItems : IScript
    {
        public enum State
        {
            Idle,
            Done,
            Begin,
            PopNext,
            Process
        }

        public delegate void BuyItemsFinished(List<int> typeIdsBought, bool ordersCreated);
        public event BuyItemsFinished OnBuyItemsFinished;

        public event BuyItem.BuyItemFinished OnBuyItemFinished;

        private State _state = State.Idle;
        private bool _done = false;

        private bool _createOrders = false;
        private Dictionary<int, int> _types = null;
        private List<int> _typeIdsBought= new List<int>();
        private List<BuyItem> _buyItems = new List<BuyItem>();
        private BuyItem _currentBuy = null;

        public BuyItems(Dictionary<int, int> types, bool createOrders)
        {
            _types = types;
            _createOrders = createOrders;
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
                    _done = true;

                    if (OnBuyItemsFinished != null)
                        OnBuyItemsFinished(_typeIdsBought, _createOrders);
                    break;

                case State.Begin:

                    foreach (KeyValuePair<int, int> type in _types)
                    {
                        BuyItem buyItem = new BuyItem(type.Key, type.Value, _createOrders);
                        buyItem.OnBuyItemFinished += BuyFinished;
                        buyItem.OnBuyItemFinished += OnBuyItemFinished;
                        _buyItems.Add(buyItem);
                    }

                    _state = State.PopNext;
                    break;

                case State.PopNext:
                    _currentBuy = _buyItems.FirstOrDefault();

                    if (_currentBuy != null)
                    {
                        _buyItems.Remove(_currentBuy);

                        Logging.Log("BuyItems:Process", "Popping next buy script to run", Logging.White);

                        _currentBuy.Initialize();
                        _state = State.Process;
                    }
                    else
                    {
                        Logging.Log("BuyItems:Process", "No more buy scripts left, going to done state", Logging.White);
                        _state = State.Done;
                    }
                    break;

                case State.Process:
                    if (_currentBuy != null)
                    {
                        _currentBuy.Process();

                        // If the current script is done then pop the next one
                        if (_currentBuy.IsDone() == true)
                        {
                            Logging.Log("BuyItems:Process", "Buy script is done, executing callback and popping next", Logging.White);
                            _state = State.PopNext;
                        }
                    }

                    break;
            }
        }

        private void BuyFinished(int typeId, bool orderCreated)
        {
            if (typeId != 0)
                _typeIdsBought.Add(typeId);
        }
    }
}
