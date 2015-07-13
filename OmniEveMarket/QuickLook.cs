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
    public class QuickLook
    {
        enum NodeType { region, station, station_name, security, range, price, vol_remain, min_volume, expires, reported_time, none };

        public QuickLook(int typeId)
        {
            TypeId = typeId;
        }

        public List<Order> SellOrders { get; set; }
        public List<Order> BuyOrders { get; set; }

        public int TypeId { get; set; }

        public bool LoadFromEveCentral()
        {
            try
            {
                string url = "http://api.eve-central.com/api/quicklook?typeid=" + TypeId.ToString() + "&usesystem=30000142";

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

                XmlTextReader xmlReader = new XmlTextReader(resStream);

                bool isSellOrder = false;
                Order order = null;

                List<Order> sellOrders = new List<Order>();
                List<Order> buyOrders = new List<Order>();

                NodeType nodeType = NodeType.none;

                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (xmlReader.Name == "sell_orders") isSellOrder = true;
                            else if (xmlReader.Name == "buy_orders") isSellOrder = false;
                            else if (xmlReader.Name == "order") order = new Order();
                            else if (xmlReader.Name == "region") nodeType = NodeType.region;
                            else if (xmlReader.Name == "station") nodeType = NodeType.station;
                            else if (xmlReader.Name == "station_name") nodeType = NodeType.station_name;
                            else if (xmlReader.Name == "security") nodeType = NodeType.security;
                            else if (xmlReader.Name == "range") nodeType = NodeType.range;
                            else if (xmlReader.Name == "price") nodeType = NodeType.price;
                            else if (xmlReader.Name == "vol_remain") nodeType = NodeType.vol_remain;
                            else if (xmlReader.Name == "min_volume") nodeType = NodeType.min_volume;
                            else if (xmlReader.Name == "expires") nodeType = NodeType.expires;
                            else if (xmlReader.Name == "reported_time") nodeType = NodeType.reported_time;
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            if (order != null)
                            {
                                if (nodeType == NodeType.region)
                                    order.Region = int.Parse(xmlReader.Value);
                                if (nodeType == NodeType.station)
                                    order.Station = int.Parse(xmlReader.Value);
                                if (nodeType == NodeType.station_name)
                                    order.StationName = xmlReader.Value;
                                if (nodeType == NodeType.security)
                                    order.Security = double.Parse(xmlReader.Value);
                                if (nodeType == NodeType.range)
                                    order.Range = int.Parse(xmlReader.Value);
                                if (nodeType == NodeType.price)
                                    order.Price = decimal.Parse(xmlReader.Value);
                                if (nodeType == NodeType.vol_remain)
                                    order.VolRemain = int.Parse(xmlReader.Value);
                                if (nodeType == NodeType.min_volume)
                                    order.MinVolume = int.Parse(xmlReader.Value);
                                if (nodeType == NodeType.expires)
                                    order.Expires = xmlReader.Value;
                                if (nodeType == NodeType.reported_time)
                                    order.ReportedTime = xmlReader.Value;
                            }
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            if (xmlReader.Name == "order")
                            {
                                if (isSellOrder == true)
                                    sellOrders.Add(order);
                                else
                                    buyOrders.Add(order);

                                order = null;
                            }

                            nodeType = NodeType.none;
                            break;
                    }
                }

                SellOrders = sellOrders.OrderBy(o => o.Price).ToList();
                BuyOrders = buyOrders.OrderByDescending(o => o.Price).ToList();

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
