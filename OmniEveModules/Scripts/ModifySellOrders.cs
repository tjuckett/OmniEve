using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Scripts
{
    using DirectEve;
    using OmniEveModules.Actions;
    using OmniEveModules.Caching;

    public class ModifySellOrders : IScript
    {
        private bool _isDone = false;
        private List<DirectOrder> _sellOrders;

        public ModifySellOrders(List<DirectOrder> sellOrders)
        {
            _sellOrders = sellOrders;
        }

        public List<IAction> CreateActionList()
        {
            List<IAction> actions = new List<IAction>();

            if (_sellOrders == null)
                return actions;

            foreach (DirectOrder order in _sellOrders)
            {
                MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);
                if(marketItem == null)
                    return null;

                if(marketItem.SellOrders.Count() <= 0)
                    return null;

                ModifyOrder modifyOrder = new ModifyOrder();
                modifyOrder.IsBid = false;
                modifyOrder.OrderId = order.OrderId;
                modifyOrder.Price = double.Parse((decimal.Parse(marketItem.SellOrders.FirstOrDefault().Price.ToString()) - 0.01m).ToString());
                actions.Add(modifyOrder);
            }

            return actions;
        }
    }
}

