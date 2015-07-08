using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Caching
{
    using DirectEve;
    using OmniEveModules.Lookup;
    using OmniEveModules.Logging;
    using OmniEveModules.Status;

    public class Cache
    {
        // Singleton implementation
        private static readonly Cache _instance = new Cache();

        public static Cache Instance
        {
            get { return _instance; }
        }

        //private List<DirectWindow> _windows;
        private DirectContainer _itemHangar { get; set; }
        private List<DirectOrder> _myBuyOrders { get; set; }
        private List<DirectOrder> _mySellOrders { get; set; }
        
        private Dictionary<int, MarketItemInfo> _marketItemInfo = new Dictionary<int, MarketItemInfo>();
        
        public DirectEve DirectEve { get; set; }

        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        /*public List<DirectWindow> Windows
        {
            get
            {
                try
                {
                    if (Status.Instance.InSpace && DateTime.UtcNow > Time.Instance.LastInStation.AddSeconds(20) || (Status.Instance.InStation && DateTime.UtcNow > Time.Instance.LastInSpace.AddSeconds(20)))
                    {
                        return _windows ?? (_windows = DirectEve.Windows);
                    }

                    return new List<DirectWindow>();
                }
                catch (Exception exception)
                {
                    Logging.Log("Cache.Windows", "Exception [" + exception + "]", Logging.Debug);
                }

                return null;
            }
        }*/

        public DirectContainer ItemHangar
        {
            get
            {
                try
                {
                    if (!Status.Instance.InSpace && Status.Instance.InStation)
                    {
                        if (_itemHangar == null)
                        {
                            _itemHangar = Cache.Instance.DirectEve.GetItemHangar();
                        }

                        if (Instance.DirectEve.Windows.All(i => i.Type != "form.StationItems")) // look for windows via the window (via caption of form type) ffs, not what is attached to this DirectCotnainer
                        {
                            if (DateTime.UtcNow > Time.Instance.LastOpenHangar.AddSeconds(10))
                            {
                                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                                Time.Instance.LastOpenHangar = DateTime.UtcNow;
                            }
                        }

                        return _itemHangar;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:ItemHangar", "Exception [" + ex + "]", Logging.Debug);
                    return null;
                }
            }

            set { _itemHangar = value; }
        }

        public delegate void MyBuyOrdersUpdated(List<DirectOrder> myBuyOrders);
        public event MyBuyOrdersUpdated OnMyBuyOrdersUpdated;
        public List<DirectOrder> MyBuyOrders
        {
            get
            {
                try
                {
                    return _myBuyOrders;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MyBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                    return null;
                }
            }

            set
            {
                try
                {
                    _myBuyOrders = value;
                    if (OnMyBuyOrdersUpdated != null)
                        OnMyBuyOrdersUpdated(_myBuyOrders);
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MyBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }
        }

        public delegate void MySellOrdersUpdated(List<DirectOrder> mySellOrders);
        public event MySellOrdersUpdated OnMySellOrdersUpdated;
        public List<DirectOrder> MySellOrders
        {
            get
            {
                try
                {
                    return _mySellOrders;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MySellOrders", "Exception [" + ex + "]", Logging.Debug);
                    return null;
                }
            }

            set
            {
                try
                {
                    _mySellOrders = value;
                    if (OnMySellOrdersUpdated != null)
                        OnMySellOrdersUpdated(_mySellOrders);
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MySellOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }
        }

        public MarketItemInfo GetMarketItemInfo(int typeId)
        {
            try
            {
                return _marketItemInfo[typeId];
            }
            catch (Exception ex)
            {
                Logging.Log("Cache:SellOrders", "Exception [" + ex + "]", Logging.Debug);
                return null;
            }
        }
        public void SetMarketItemInfo(int typeId, MarketItemInfo info)
        {
            try
            {
                _marketItemInfo[typeId] = info;
            }
            catch (Exception ex)
            {
                Logging.Log("Cache:SellOrders", "Exception [" + ex + "]", Logging.Debug);
            }
        }
    }
}
