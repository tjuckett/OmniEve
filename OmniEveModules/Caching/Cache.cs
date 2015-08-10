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
        private DirectContainer _itemHanger;
        private List<DirectOrder> _myBuyOrders { get; set; }
        private List<DirectOrder> _mySellOrders { get; set; }

        private Dictionary<int, MarketItem> _marketItems = new Dictionary<int, MarketItem>();
        
        public DirectEve DirectEve { get; set; }

        public int OrderCap { get; set; }

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

        public DirectContainer ItemHanger
        {
            get
            {
                try
                {
                    return _itemHanger;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:ItemHangar", "Exception [" + ex + "]", Logging.Debug);
                    return null;
                }
            }

            set 
            {
                try
                {
                    _itemHanger = value;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:ItemHangar", "Exception [" + ex + "]", Logging.Debug);
                } 
            }
        }

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
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MyBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }
        }

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
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MySellOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }
        }

        public Dictionary<int, MarketItem> MarketItems
        {
            get
            {
                try
                {
                    return _marketItems;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MarketItems", "Exception [" + ex + "]", Logging.Debug);
                    return null;
                }
            }
            set
            {
                try
                {
                    _marketItems = value;
                }
                catch (Exception ex)
                {
                    Logging.Log("Cache:MySellOrders", "Exception [" + ex + "]", Logging.Debug);
                }
            }
        }

        public MarketItem GetMarketItem(int typeId)
        {
            try
            {
                MarketItem marketItem;
                if (_marketItems.TryGetValue(typeId, out marketItem))
                    return marketItem;

                return null;
            }
            catch (Exception ex)
            {
                Logging.Log("Cache:SellOrders", "Exception [" + ex + "]", Logging.Debug);
                return null;
            }
        }
        public void SetMarketItem(int typeId, MarketItem info)
        {
            try
            {
                _marketItems[typeId] = info;
            }
            catch (Exception ex)
            {
                Logging.Log("Cache:SellOrders", "Exception [" + ex + "]", Logging.Debug);
            }
        }
    }
}
