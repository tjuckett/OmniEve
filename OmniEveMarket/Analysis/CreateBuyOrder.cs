using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmniEveMarket.Analysis
{
    public class CreateBuyOrder
    {
        public CreateBuyOrder(decimal salesTax, decimal brokersFee)
        {
            _salesTax = salesTax;
            _brokersFee = brokersFee;
        }

        private decimal _salesTax = 1.5m;
        private decimal _brokersFee = 1.0m;

        public decimal ProfitMargin { get; set; }
        public decimal Profit { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }

        public bool AnalyzeOrders(List<Order> sellOrders, List<Order> buyOrders)
        {
            try
            {
                Order minSellOrder = sellOrders.FirstOrDefault();
                Order maxBuyOrder = buyOrders.FirstOrDefault();

                if (minSellOrder != null && maxBuyOrder != null)
                {
                    decimal newBuyPrice = maxBuyOrder.Price + 0.01m;
                    decimal priceDiff = minSellOrder.Price - newBuyPrice;
                    decimal totalFees = minSellOrder.Price * ((_brokersFee + _salesTax) / 100.0m);

                    decimal profit = priceDiff - totalFees;
                    decimal profitMargin = profit / newBuyPrice;

                    BuyPrice = newBuyPrice;
                    Profit = profit;
                    ProfitMargin = profitMargin;
                    SellPrice = minSellOrder.Price;

                    return true;
                }
                else
                {
                    return false;
                }
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
