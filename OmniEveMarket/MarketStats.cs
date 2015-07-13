using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace OmniEveMarket
{
    public class MarketStat
    {
        public long Volume { get; set; }
        public double Avg { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double Stddev { get; set; }
        public double Median { get; set; }
        public double Percentile { get; set; }
    }

    public class MarketStats
    {
        public MarketStats(int typeId)
        {
            TypeId = typeId;
        }

        public MarketStat Buy { get; set; }
        public MarketStat Sell { get; set; }
        public MarketStat All { get; set; }

        public int TypeId { get; set; }

        public bool LoadFromEveCentral()
        {
            try
            {
                string url = "http://api.eve-central.com/api/marketstat?typeid=" + TypeId.ToString() + "&usesystem=30000142";

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

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resString);

                Sell = new MarketStat();
                Sell.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/volume").InnerText);
                Sell.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/avg").InnerText);
                Sell.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/max").InnerText);
                Sell.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/min").InnerText);
                Sell.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/stddev").InnerText);
                Sell.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/median").InnerText);
                Sell.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/percentile").InnerText);

                Buy = new MarketStat();
                Buy.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/volume").InnerText);
                Buy.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/avg").InnerText);
                Buy.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/max").InnerText);
                Buy.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/min").InnerText);
                Buy.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/stddev").InnerText);
                Buy.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/median").InnerText);
                Buy.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/percentile").InnerText);

                All = new MarketStat();
                All.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/volume").InnerText);
                All.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/avg").InnerText);
                All.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/max").InnerText);
                All.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/min").InnerText);
                All.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/stddev").InnerText);
                All.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/median").InnerText);
                All.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/percentile").InnerText);

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
