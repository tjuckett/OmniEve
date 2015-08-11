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
    using OmniEveModules.Logging;
    using OmniEveModules.Status;

    public class UpdateAllOrders : IScript
    {
        public event ModifyOrder.ModifyOrderFinished OnModifySellOrderFinished;
        public event ModifyOrder.ModifyOrderFinished OnModifyBuyOrderFinished;

        public event CancelOrder.CancelOrderFinished OnCancelOrderFinished;

        private bool _isDone = false;

        public override bool IsDone()
        {
            return _isDone;
        }

        public override void OnFrame()
        {
            RunActions(CreateModifySellActions());
            RunActions(CreateModifyBuyAndCancelActions());

            _isDone = true;
        }

        public List<IAction> CreateModifySellActions()
        {
            List<IAction> actions = new List<IAction>();

            foreach (DirectOrder order in Cache.Instance.MySellOrders)
            {
                MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                if (marketItem == null)
                    continue;

                DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
                DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                if (lowestSellOrder != null && lowestSellOrder.Price <= order.Price && lowestSellOrder.OrderId != order.OrderId)
                {
                    double priceDifference = order.Price - lowestSellOrder.Price;
                    double priceDifferencePct = priceDifference / order.Price;
                    double price = double.Parse((decimal.Parse(lowestSellOrder.Price.ToString()) - 0.01m).ToString());

                    bool createModifyOrder = false;

                    if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                        createModifyOrder = true;
                    else if (highestBuyOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                        createModifyOrder = true;

                    if (createModifyOrder == true)
                    {
                        Logging.Log("UpdateAllOrders:Process", "Creating sell modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                        ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, false, price);
                        modifyOrder.OnModifyOrderFinished += OnModifySellOrderFinished;
                        actions.Add(modifyOrder);
                    }
                }
            }

            return actions;
        }

        public List<IAction> CreateModifyBuyAndCancelActions()
        {
            List<IAction> actions = new List<IAction>();

            foreach (DirectOrder order in Cache.Instance.MyBuyOrders)
            {
                MarketItem marketItem = Cache.Instance.GetMarketItem(order.TypeId);

                if (marketItem == null)
                    continue;

                DirectOrder highestBuyOrder = marketItem.BuyOrders.OrderByDescending(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);
                DirectOrder lowestSellOrder = marketItem.SellOrders.OrderBy(o => o.Price).FirstOrDefault(o => o.StationId == order.StationId);

                double profit = lowestSellOrder.Price - highestBuyOrder.Price;
                double tax = lowestSellOrder.Price * .01 + highestBuyOrder.Price * 0.015;
                double profitPct = lowestSellOrder.Price / highestBuyOrder.Price;

                if (highestBuyOrder != null && lowestSellOrder != null && ((profit < 10000000 && profitPct < 1.25) || (profit >= 10000000 && tax > profit * 0.5)))
                {
                    Logging.Log("UpdateAllOrders:Process", "Creating cancel order script for Order Id - " + order.OrderId, Logging.White);
                    CancelOrder cancelOrder = new CancelOrder(order.OrderId, true);
                    cancelOrder.OnCancelOrderFinished += OnCancelOrderFinished;
                    actions.Add(cancelOrder);
                }
                // Don't modify if there isn't a lowest order and don't modify if the lowest order is our own
                else if (highestBuyOrder != null && highestBuyOrder.OrderId != order.OrderId && highestBuyOrder.Price >= order.Price)
                {
                    double priceDifference = highestBuyOrder.Price - order.Price;
                    double priceDifferencePct = priceDifference / order.Price;
                    double price = double.Parse((decimal.Parse(highestBuyOrder.Price.ToString()) + 0.01m).ToString());

                    bool createModifyOrder = false;

                    if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                        createModifyOrder = true;
                    else if (lowestSellOrder != null && lowestSellOrder.Price / highestBuyOrder.Price >= 1.5 && priceDifferencePct < 0.25)
                        createModifyOrder = true;

                    if (createModifyOrder == true)
                    {
                        Logging.Log("UpdateAllOrders:Process", "Creating buy modify order script for Order Id - " + order.OrderId + " Price - " + price, Logging.White);
                        ModifyOrder modifyOrder = new ModifyOrder(order.OrderId, true, price);
                        modifyOrder.OnModifyOrderFinished += OnModifyBuyOrderFinished;
                        actions.Add(modifyOrder);
                    }
                }
            }

            return actions;
        }
    }
}
