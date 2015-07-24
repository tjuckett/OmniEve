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

    public class MarketInfoForList : IScript
    {
        public delegate void MarketInfoForListFinished();
        public event MarketInfoForListFinished OnMarketInfoForListFinished;

        public event MarketInfo.MarketInfoFinished OnMarketInfoFinished;

        public List<int> TypeIds { get; set; }

        private MarketInfoForListState _state = MarketInfoForListState.Idle;
        private bool _done = false;

        private List<MarketInfo> _marketInfos = new List<MarketInfo>();
        private MarketInfo _currentMarketInfo = null;

        public MarketInfoForList(List<int> typeIds)
        {
            TypeIds = typeIds;
        }

        public void Initialize()
        {
            _state = MarketInfoForListState.Begin;
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
                case MarketInfoForListState.Idle:
                    break;
                case MarketInfoForListState.Done:
                    if (OnMarketInfoForListFinished != null)
                        OnMarketInfoForListFinished();

                    _done = true;
                    break;

                case MarketInfoForListState.Begin:

                    foreach(int typeId in TypeIds)
                    {
                        MarketInfo marketInfo = new MarketInfo(typeId);
                        marketInfo.OnMarketInfoFinished += MarketInfoFinished;
                        _marketInfos.Add(marketInfo);
                    }

                    // Don't close the market window if its already up
                    _state = MarketInfoForListState.PopNext;
                    break;

                case MarketInfoForListState.PopNext:
                    _currentMarketInfo = _marketInfos.FirstOrDefault();

                    if (_currentMarketInfo != null)
                    {
                        _marketInfos.Remove(_currentMarketInfo);

                        Logging.Log("MarketInfoForList:Process", "Popping next market info script to run", Logging.White);

                        _currentMarketInfo.Initialize();
                        _state = MarketInfoForListState.Process;
                    }
                    else
                    {
                        Logging.Log("MarketInfoForList:Process", "No more market info scripts left, going to done state", Logging.White);
                        _state = MarketInfoForListState.Done;
                    }
                    break;

                case MarketInfoForListState.Process:

                    if (_currentMarketInfo != null)
                    {
                        _currentMarketInfo.Process();

                        // If the current script is done then pop the next one
                        if (_currentMarketInfo.IsDone() == true)
                        {
                            Logging.Log("MarketInfoForList:Process", "Market info script is done, executing callback and popping next", Logging.White);

                            _state = MarketInfoForListState.PopNext;
                        }
                    }
                    break;
            }
        }

        private void MarketInfoFinished(MarketItem marketItem)
        {
            OnMarketInfoFinished(marketItem);
        }
    }
}
