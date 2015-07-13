using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OmniEveMarket
{
    public class PriceHistory
    {
        public PriceHistory(int typeId)
        {
            TypeId = typeId;
        }

        public class Day
        {
            public DateTime Date { get; set; }
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public decimal AvgPrice { get; set; }
            public long Volume { get; set; }
            public long OrderCount { get; set; }
        }

        public List<Day> Days { get; set; }

        public decimal AvgMinPrice { get; set; }
        public decimal AvgMaxPrice { get; set; }
        public decimal AvgPrice { get; set; }
        public long AvgOrders { get; set; }
        public long AvgVolume { get; set; }

        public long TypeId { get; set; }

        public bool LoadFromEveCrest()
        {
            try
            {
                string url = "https://public-crest.eveonline.com/market/10000002/types/" + TypeId.ToString() + "/history/";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    if (Logging.ShowDebugMessages == true)
                        MessageBox.Show(ex.Message);
                    return false;
                }

                Days = new List<Day>();

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                string resString = reader.ReadToEnd();

                JsonHistoryPage page = JsonConvert.DeserializeObject<JsonHistoryPage>(resString);

                long totalOrdersMovement = 0;
                long totalVolumeMovement = 0;
                decimal totalAvgPrice = 0.0m;
                decimal totalMaxPrice = 0.0m;
                decimal totalMinPrice = 0.0m;

                foreach (JsonHistoryItem jsonItem in page.Items)
                {
                    PriceHistory.Day day = new PriceHistory.Day();

                    day.AvgPrice = jsonItem.AvgPrice;
                    day.MaxPrice = jsonItem.HighPrice;
                    day.MinPrice = jsonItem.LowPrice;
                    day.OrderCount = jsonItem.OrderCount;
                    day.Volume = jsonItem.Volume;
                    day.Date = Convert.ToDateTime(jsonItem.Date);

                    totalAvgPrice += day.AvgPrice;
                    totalMaxPrice += day.MaxPrice;
                    totalMinPrice += day.MinPrice;
                    totalOrdersMovement += day.OrderCount;
                    totalVolumeMovement += day.Volume;

                    Days.Add(day);
                }

                if (page.Items.Count() > 0)
                {
                    AvgVolume = totalVolumeMovement / page.Items.Count();
                    AvgPrice = totalAvgPrice / page.Items.Count();
                    AvgOrders = totalOrdersMovement / page.Items.Count();
                    AvgMinPrice = totalMinPrice / page.Items.Count();
                    AvgMaxPrice = totalVolumeMovement / page.Items.Count();
                }

                Days = Days.OrderByDescending(d => d.Date).ToList();

                //if (page.Next != null && page.Next.Href.Count() >= 0)
                //    LoadFromEveCrest(page.Next.Href);

                return true;
            }
            catch (Exception ex)
            {
                if (Logging.ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return false;
            }
        }

        public bool LoadFromEveMarketData()
        {
            try
            {
                string url = "http://api.eve-marketdata.com/api/item_history2.txt?char_name=Caidin Moss&region_ids=10000002&type_ids=" + TypeId.ToString();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    if (Logging.ShowDebugMessages == true)
                        MessageBox.Show(ex.Message);
                    return false;
                }

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                string resString = reader.ReadToEnd();

                string[] historyLines = resString.Split(new char[] { '\n' });

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
                        day.OrderCount = long.Parse(variables[7]);
                        day.Volume = long.Parse(variables[6]);

                        totalAvgPrice += day.AvgPrice;
                        totalMaxPrice += day.MaxPrice;
                        totalMinPrice += day.MinPrice;
                        totalOrdersMovement += day.OrderCount;
                        totalVolumeMovement += day.Volume;

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
                if (Logging.ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return false;
            }
        }
    }
}
