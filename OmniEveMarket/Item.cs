using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmniEveMarket
{
    public class Item
    {
        public Item(int typeId, string name)
        {
            TypeId = typeId;
            Name = name;
        }

        public string Name { get; set; }
        public int TypeId { get; set; }
        public bool Updated { get; set; }

        public Analysis.BuySellOrders BuySellOrders { get; set; }
        public Analysis.CreateBuyOrder CreateBuyOrder { get; set; }
        public MarketStats MarketStats { get; set; }
        public PriceHistory PriceHistory { get; set; }
        public QuickLook QuickLook { get; set; }

        public bool ShouldBuySellOrder(decimal minProfit, decimal profitMargin, decimal iskLimit)
        {
            try
            {
                if (BuySellOrders == null || MarketStats == null || PriceHistory == null)
                    return false;

                if (BuySellOrders.SellOrdersBought < (BuySellOrders.SellOrderCount * 0.35) &&
                    BuySellOrders.VolumeBought < (MarketStats.Sell.Volume * 0.35) &&
                    BuySellOrders.ProfitMargin > (profitMargin / 100.0m) &&
                    BuySellOrders.Profit > minProfit &&
                    (BuySellOrders.TotalSpent < iskLimit || iskLimit == 0.0m))
                {
                    int priceHistoryMatch = 0;

                    foreach (PriceHistory.Day day in PriceHistory.Days)
                    {
                        if (BuySellOrders.SellOrdersBought < day.OrderCount &&
                            BuySellOrders.VolumeBought < day.Volume &&
                            BuySellOrders.SellPrice > day.MaxPrice * 0.9m &&
                            BuySellOrders.SellPrice < day.MaxPrice * 1.1m)
                        {
                            priceHistoryMatch++;
                        }
                    }

                    if (PriceHistory.Days.Count() > 0 && priceHistoryMatch / PriceHistory.Days.Count() > 0.5)
                        return true;
                }
            }
            catch (Exception ex)
            {
                if (Logging.ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }

            return false;
        }

        public bool ShouldCreateBuyOrder(decimal minProfit, decimal profitMargin, decimal iskLimit, int minVolume, int minOrders)
        {
            try
            {
                if (CreateBuyOrder == null || MarketStats == null || PriceHistory == null)
                    return false;

                if (MarketStats.Sell.Min > MarketStats.Buy.Max &&
                    CreateBuyOrder.ProfitMargin > (profitMargin / 100.0m))
                {
                    if (PriceHistory.Days != null)
                    {
                        int priceHistoryMinMatch = 0;
                        int priceHistoryMaxMatch = 0;

                        // Only anaylze the last 30 days
                        List<PriceHistory.Day> lastThirty = PriceHistory.Days.Take(30).ToList();
                        foreach (PriceHistory.Day day in lastThirty)
                        {
                            if (CreateBuyOrder.BuyPrice > day.MinPrice * 0.8m)
                            {
                                priceHistoryMinMatch++;
                            }
                            if (CreateBuyOrder.SellPrice < day.MaxPrice * 1.2m)
                            {
                                priceHistoryMaxMatch++;
                            }
                        }

                        if (lastThirty.Count() > 0 &&
                            (decimal)priceHistoryMinMatch / (decimal)lastThirty.Count() > 0.5m &&
                            (decimal)priceHistoryMaxMatch / (decimal)lastThirty.Count() > 0.5m &&
                            PriceHistory.AvgVolume > minVolume && PriceHistory.AvgOrders > minOrders && 
                            CreateBuyOrder.Profit * PriceHistory.AvgVolume > minProfit)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Logging.ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }

            return false;
        }
    }
}
