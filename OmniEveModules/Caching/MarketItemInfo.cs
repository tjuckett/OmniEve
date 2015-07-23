using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Caching
{
    using DirectEve;

    public class MarketItem
    {
        public int TypeId { get; set; }
        public List<DirectOrder> SellOrders { get; set; }
        public List<DirectOrder> BuyOrders { get; set; }
    }
}
