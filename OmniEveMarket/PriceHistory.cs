using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OmniEveMarket
{
    public class PriceHistory
    {
        public class Day
        {
            public string Date { get; set; }
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public decimal AvgPrice { get; set; }
            public long VolumeMovement { get; set; }
            public long OrdersMovement { get; set; }
        }

        public List<Day> Days { get; set; }

        public decimal AvgMinPrice { get; set; }
        public decimal AvgMaxPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public long AvgOrders { get; set; }
        public long AvgVolume { get; set; }

        public long TypeId { get; set; }

        public bool LoadPriceHistory(int typeId)
        {
            try
            {
                
                string url = "http://api.eve-marketdata.com/api/item_history2.txt?char_name=Caidin Moss&region_ids=10000002&type_ids=" + typeId.ToString();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    return false;
                }

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                string resString = reader.ReadToEnd();

                string[] historyLines = resString.Split(new char[] { '\n' });

                PriceHistory priceHistory = new PriceHistory();

                if (historyLines.Count() <= 0)
                    return false;

                int priceHistoryCount = 0;
                long totalOrdersMovement = 0;
                long totalVolumeMovement = 0;
                decimal totalAvgPrice = 0.0m;
                decimal totalMaxPrice = 0.0m;
                decimal totalMinPrice = 0.0m;

                foreach (string line in historyLines)
                {
                    if (line.Count() > 0)
                    {
                        string[] variables = line.Split(new char[] { '\t' });

                        PriceHistory.Day day = new PriceHistory.Day();

                        day.AvgPrice = decimal.Parse(variables[5]);
                        day.MaxPrice = decimal.Parse(variables[4]);
                        day.MinPrice = decimal.Parse(variables[3]);
                        day.OrdersMovement = long.Parse(variables[7]);
                        day.VolumeMovement = long.Parse(variables[6]);

                        totalAvgPrice += day.AvgPrice;
                        totalMaxPrice += day.MaxPrice;
                        totalMinPrice += day.MinPrice;
                        totalOrdersMovement += day.OrdersMovement;
                        totalVolumeMovement += day.VolumeMovement;

                        Days.Add(day);

                        priceHistoryCount++;
                    }
                }

                AvgVolume = (priceHistoryCount > 0) ? totalVolumeMovement / priceHistoryCount : 0;
                AvgPrice = (priceHistoryCount > 0) ? totalAvgPrice / priceHistoryCount : 0;
                AvgOrders = (priceHistoryCount > 0) ? totalOrdersMovement / priceHistoryCount : 0;
                AvgMinPrice = (priceHistoryCount > 0) ? totalMinPrice / priceHistoryCount : 0;
                AvgMaxPrice = (priceHistoryCount > 0) ? totalVolumeMovement / priceHistoryCount : 0;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
