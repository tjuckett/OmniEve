using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmniEveMarket.Analysis
{
    public class BuySellOrders
    {
        public BuySellOrders(decimal salesTax, decimal brokersFee)
        {
            _salesTax = salesTax;
            _brokersFee = brokersFee;
        }

        private decimal _salesTax = 1.5m;
        private decimal _brokersFee = 1.0m;

        public int SellOrderCount { get; set; }
        public int SellOrdersBought { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal Profit { get; set; }
        public decimal SellPrice { get; set; }
        public long VolumeBought { get; set; }

        public bool AnalyzeOrders(List<Order> sellOrders)
        {
            try
            {
                List<Order> ordersBought = new List<Order>();

                SellOrderCount = sellOrders.Count;

                for (int index = 0; index < sellOrders.Count - 1; ++index)
                {
                    Order order = sellOrders[index];
                    Order nextOrder = sellOrders[index + 1];

                    decimal newPrice = nextOrder.Price - 0.01m;
                    decimal priceDiff = newPrice - order.Price;
                    decimal totalFees = newPrice * ((_brokersFee + _salesTax) / 100.0m);

                    long volumeBought = order.VolRemain;
                    decimal totalSpent = order.Price * order.VolRemain;
                    decimal profit = (priceDiff * order.VolRemain) - totalFees;

                    foreach (Order boughtOrder in ordersBought)
                    {
                        decimal newPriceDiff = newPrice - boughtOrder.Price;

                        volumeBought += boughtOrder.VolRemain;
                        totalSpent += boughtOrder.Price * boughtOrder.VolRemain;
                        profit += (newPriceDiff * boughtOrder.VolRemain) - totalFees;
                    }

                    decimal profitMargin = profit / totalSpent;

                    ordersBought.Add(order);

                    SellOrdersBought = ordersBought.Count;
                    Profit = profit;
                    ProfitMargin = profitMargin;
                    TotalSpent = totalSpent;
                    VolumeBought = volumeBought;
                    SellPrice = newPrice;

                    if (profitMargin > 0.05m)
                    {
                        break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                if (Logging.ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
