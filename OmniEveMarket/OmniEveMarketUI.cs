using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using MetroFramework.Forms;
using MetroFramework.Controls;

namespace OmniEveMarket
{
    public partial class OmniEveMarketUI : MetroForm
    {
        private bool ShowDebugMessages = true;

        private List<MetroGrid> _grids = new List<MetroGrid>();

        private OpenFileDialog _openfileDialog;

        private decimal _salesTax = 1.5m;
        private decimal _brokersFee = 1.0m;
        private decimal _iskLimit = 0.0m;
        private decimal _profitMarginPct = 20.0m;
        private decimal _minProfit = 0.0m;
        private bool _priceHistory = true;
        private bool _optionsChanged = false;
        private bool _cancelMarketLoad = false;

        private List<ItemData> _itemData = new List<ItemData>();
        private List<ItemData> _updatedItems = new List<ItemData>();
        private List<ItemData> _uiUpdatedItems = new List<ItemData>();
        private List<Item> _allItems = new List<Item>();

        BackgroundWorker backgroundWorker = new BackgroundWorker();

        public class Item
        {
            public string Name { get; set; }
            public int TypeId { get; set; }
        }

        public struct MarketStat
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
            public MarketStat Buy = new MarketStat();
            public MarketStat Sell = new MarketStat();
            public MarketStat All = new MarketStat();
        }

        public class QuickLook
        {
            public List<Order> SellOrders { get; set; }
            public List<Order> BuyOrders { get; set; }
        }

        public class PriceHistory
        {
            public string Date { get; set; }
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public decimal AvgPrice { get; set; }
            public long VolumeMovement { get; set; }
            public long OrdersMovement { get; set; }
        }

        public class History
        {
            public History() { PriceHistory = new List<PriceHistory>(); }

            public List<PriceHistory> PriceHistory { get; set; }
            public decimal AvgMinPrice { get; set; }
            public decimal AvgMaxPrice { get; set; }
            public decimal AvgPrice { get; set; }
            public long AvgOrders { get; set; }
            public long AvgVolume { get; set; }
        }

        public class Order
        {
            public int Region { get; set; }
            public int Station { get; set; }
            public string StationName { get; set; }
            public double Security { get; set; }
            public int Range { get; set; }
            public decimal Price { get; set; }
            public int VolRemain { get; set; }
            public int MinVolume { get; set; }
            public string Expires { get; set; }
            public string ReportedTime { get; set; }
        }

        public class BuySellOrdersAnalysis
        {
            public int SellOrderCount { get; set; }
            public int SellOrdersBought { get; set; }
            public decimal TotalSpent { get; set; }
            public decimal ProfitMargin { get; set; }
            public decimal Profit { get; set; }
            public decimal SellPrice { get; set; }
            public long VolumeBought { get; set; }
        }

        public class CreateBuyOrdersAnalysis
        {
            public decimal ProfitMargin { get; set; }
            public decimal Profit { get; set; }
            public decimal BuyPrice { get; set; }
            public decimal SellPrice { get; set; }
        }

        public class ItemData
        {
            public string Name { get; set; }
            public int TypeId { get; set; }
            public bool Updated { get; set; }

            public BuySellOrdersAnalysis BuySellOrdersAnalysis { get; set; }
            public CreateBuyOrdersAnalysis CreateBuyOrdersAnalysis { get; set; }
            public MarketStats MarketStats { get; set; }
            public History History { get; set; }
        }

        enum NodeType { region, station, station_name, security, range, price, vol_remain, min_volume, expires, reported_time, none };

        public OmniEveMarketUI()
        {
            InitializeComponent();

            _openfileDialog = new OpenFileDialog();

            _salesTax = Properties.Settings.Default.SalesTax;
            _brokersFee = Properties.Settings.Default.BrokersFee;
            _iskLimit = Properties.Settings.Default.ISKLimit;
            _minProfit = Properties.Settings.Default.MinProfit;
            _profitMarginPct = Properties.Settings.Default.ProfitMarginPct;
            _priceHistory = Properties.Settings.Default.PriceHistory;

            marketItemsFileTextBox.Text = Properties.Settings.Default.MarketDataFile;
            marketItemsFileTextBox.TextChanged += marketItemsTextChanged;

            typeIdsFileTextBox.Text = Properties.Settings.Default.TypeIdsFile;
            typeIdsFileTextBox.TextChanged += typeIdsTextChanged;

            salesTaxTextBox.Text = _salesTax.ToString();
            salesTaxTextBox.TextChanged += salesTaxTextChanged;

            iskLimitTextBox.Text = _iskLimit.ToString();
            iskLimitTextBox.TextChanged += iskLimitTextChanged;

            brokersFeeTextBox.Text = _brokersFee.ToString();
            brokersFeeTextBox.TextChanged += brokersFeeTextChanged;

            minProfitTextBox.Text = _minProfit.ToString();
            minProfitTextBox.TextChanged += minProfitTextChanged;

            profitMarginPctTextBox.Text = _profitMarginPct.ToString();
            profitMarginPctTextBox.TextChanged += profitMarginPctTextChanged;

            priceHistoryCheckBox.Checked = _priceHistory;
            priceHistoryCheckBox.CheckedChanged += priceHistoryCheckBox_CheckedChanged;

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkCompleted;
            backgroundWorker.WorkerReportsProgress = true;
        }

        private void LoadAllItemIds()
        {
            _allItems.Clear();

            List<string> lines = File.ReadLines(typeIdsFileTextBox.Text).ToList();
            foreach (string line in lines)
            {
                string[] variables = line.Split(new char[] {' '}, 2);
                Item item = new Item();
                item.TypeId = int.Parse(variables[0]);
                item.Name = variables[1].TrimStart();

                _allItems.Add(item);
            }
        }

        private void LoadItems()
        {
            try
            {
                if (marketItemsFileTextBox.Text.Count() > 0 && typeIdsFileTextBox.Text.Count() > 0)
                {
                    LoadAllItemIds();

                    _itemData.Clear();

                    List<string> lines = File.ReadLines(marketItemsFileTextBox.Text).ToList();
                    foreach (string line in lines)
                    {
                        int typeId = GetTypeIdByName(line.Trim());
                        string name = line.Trim();

                        ItemData itemData = new ItemData();
                        itemData.TypeId = typeId;
                        itemData.Name = name;
                        _itemData.Add(itemData);
                    }
                }
            }
            catch(Exception ex)
            {
                if(ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        private MarketStats GetMarketStats(int typeId)
        {
            try
            { 
                string url = "http://api.eve-central.com/api/marketstat?typeid=" + typeId.ToString() + "&usesystem=30000142";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    if (ShowDebugMessages == true)
                        MessageBox.Show(ex.Message);
                    return null;
                }

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                string resString = reader.ReadToEnd();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(resString);

                MarketStats marketStats = new MarketStats();

                marketStats.Sell.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/volume").InnerText);
                marketStats.Sell.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/avg").InnerText);
                marketStats.Sell.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/max").InnerText);
                marketStats.Sell.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/min").InnerText);
                marketStats.Sell.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/stddev").InnerText);
                marketStats.Sell.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/median").InnerText);
                marketStats.Sell.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/sell/percentile").InnerText);

                marketStats.Buy.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/volume").InnerText);
                marketStats.Buy.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/avg").InnerText);
                marketStats.Buy.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/max").InnerText);
                marketStats.Buy.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/min").InnerText);
                marketStats.Buy.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/stddev").InnerText);
                marketStats.Buy.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/median").InnerText);
                marketStats.Buy.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/buy/percentile").InnerText);

                marketStats.All.Volume = long.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/volume").InnerText);
                marketStats.All.Avg = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/avg").InnerText);
                marketStats.All.Max = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/max").InnerText);
                marketStats.All.Min = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/min").InnerText);
                marketStats.All.Stddev = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/stddev").InnerText);
                marketStats.All.Median = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/median").InnerText);
                marketStats.All.Percentile = double.Parse(doc.DocumentElement.SelectSingleNode("/evec_api/marketstat/type/all/percentile").InnerText);

                return marketStats;
            }
            catch(Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return null;
            }
        }

        private QuickLook GetQuickLook(int typeId)
        {

            try
            {
                string url = "http://api.eve-central.com/api/quicklook?typeid=" + typeId.ToString() + "&usesystem=30000142";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = null;

                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    if (ShowDebugMessages == true)
                        MessageBox.Show(ex.Message);
                    return null;
                }

                Stream resStream = response.GetResponseStream();

                XmlTextReader xmlReader = new XmlTextReader(resStream);

                bool isSellOrder = false;
                Order order = null;

                List<Order> sellOrders = new List<Order>();
                List<Order> buyOrders = new List<Order>();

                NodeType nodeType = NodeType.none;

                while(xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if(xmlReader.Name == "sell_orders") isSellOrder = true;
                            else if(xmlReader.Name == "buy_orders") isSellOrder = false;
                            else if(xmlReader.Name == "order") order = new Order();
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
                            if(order != null)
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

                QuickLook quickLook = new QuickLook();

                quickLook.SellOrders = sellOrders.OrderBy(o => o.Price).ToList();
                quickLook.BuyOrders = buyOrders.OrderByDescending(o => o.Price).ToList();

                return quickLook;
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return null;
            }
        }

        public History GetHistory(int typeId)
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
                    if (ShowDebugMessages == true)
                        MessageBox.Show(ex.Message);
                    return null;
                }

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
                string resString = reader.ReadToEnd();

                string[] historyLines = resString.Split(new char[] {'\n'});

                History history = new History();

                if(historyLines.Count() <= 0)
                    return null;

                int priceHistoryCount = 0;
                long totalOrdersMovement = 0;
                long totalVolumeMovement = 0;
                decimal totalAvgPrice = 0.0m;
                decimal totalMaxPrice = 0.0m;
                decimal totalMinPrice = 0.0m;

                foreach (string line in historyLines)
                {
                    if(line.Count() > 0)
                    { 
                        string[] variables = line.Split(new char[] {'\t'});

                        PriceHistory priceHistory = new PriceHistory();

                        priceHistory.AvgPrice = decimal.Parse(variables[5]);
                        priceHistory.MaxPrice = decimal.Parse(variables[4]);
                        priceHistory.MinPrice = decimal.Parse(variables[3]);
                        priceHistory.OrdersMovement = long.Parse(variables[7]);
                        priceHistory.VolumeMovement = long.Parse(variables[6]);

                        totalAvgPrice += priceHistory.AvgPrice;
                        totalMaxPrice += priceHistory.MaxPrice;
                        totalMinPrice += priceHistory.MinPrice;
                        totalOrdersMovement += priceHistory.OrdersMovement;
                        totalVolumeMovement += priceHistory.VolumeMovement;

                        history.PriceHistory.Add(priceHistory);

                        priceHistoryCount++;
                    }
                }

                history.AvgVolume = (priceHistoryCount > 0) ? totalVolumeMovement / priceHistoryCount : 0;
                history.AvgPrice = (priceHistoryCount > 0) ? totalAvgPrice / priceHistoryCount : 0;
                history.AvgOrders = (priceHistoryCount > 0) ? totalOrdersMovement / priceHistoryCount : 0;
                history.AvgMinPrice = (priceHistoryCount > 0) ? totalMinPrice / priceHistoryCount : 0;
                history.AvgMaxPrice = (priceHistoryCount > 0) ? totalVolumeMovement / priceHistoryCount : 0;

                return history;
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void EnableSorting(MetroGrid grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
        }

        public BuySellOrdersAnalysis AnaylzeBuySellOrders(List<Order> sellOrders)
        {
            try
            {
                BuySellOrdersAnalysis analysis = new BuySellOrdersAnalysis();

                List<Order> ordersBought = new List<Order>();

                analysis.SellOrderCount = sellOrders.Count;
                
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

                    analysis.SellOrdersBought = ordersBought.Count;
                    analysis.Profit = profit;
                    analysis.ProfitMargin = profitMargin;
                    analysis.TotalSpent = totalSpent;
                    analysis.VolumeBought = volumeBought;
                    analysis.SellPrice = newPrice;

                    if (profitMargin > 0.05m)
                    {
                        break;
                    }
                }

                return analysis;
            }
            catch(Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return null;
            }
        }

        public CreateBuyOrdersAnalysis AnaylzeCreateBuyOrders(List<Order> sellOrders, List<Order> buyOrders, long avgVolumeMovement)
        {
            try
            {
                CreateBuyOrdersAnalysis analysis = new CreateBuyOrdersAnalysis();

                Order minSellOrder = sellOrders.FirstOrDefault();
                Order maxBuyOrder = buyOrders.FirstOrDefault();

                if (minSellOrder != null && maxBuyOrder != null)
                {
                    decimal newBuyPrice = maxBuyOrder.Price + 0.01m;
                    decimal priceDiff = minSellOrder.Price - newBuyPrice;
                    decimal totalFees = minSellOrder.Price * ((_brokersFee + _salesTax) / 100.0m);

                    decimal profitMargin = 0;
                    decimal profit = 0;

                    if (avgVolumeMovement != 0)
                    {
                        profit = (priceDiff * avgVolumeMovement) - totalFees;
                        profitMargin = profit / (newBuyPrice * avgVolumeMovement);
                    }
                    
                    analysis.BuyPrice = newBuyPrice;
                    analysis.Profit = profit;
                    analysis.ProfitMargin = profitMargin;
                    analysis.SellPrice = minSellOrder.Price;

                    return analysis;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return null;
            }
        }

        public void ClearSellOrdersToBuy()
        {
            buySellOrdersGrid.Rows.Clear();
        }

        public void ClearBuyOrdersToCreate()
        {
            createBuyOrdersGrid.Rows.Clear();
        }

        public void GetSavedSettings()
        {
            _brokersFee = Properties.Settings.Default.BrokersFee;
            _salesTax = Properties.Settings.Default.SalesTax;
            _iskLimit = Properties.Settings.Default.ISKLimit;
            _minProfit = Properties.Settings.Default.MinProfit;
            _profitMarginPct = Properties.Settings.Default.ProfitMarginPct;
            _priceHistory = Properties.Settings.Default.PriceHistory;
        }

        public bool ShouldBuySellOrder(ItemData itemData)
        {
            if (itemData == null || itemData.BuySellOrdersAnalysis == null || itemData.MarketStats == null)
                return false;

            if (itemData.BuySellOrdersAnalysis.SellOrdersBought < (itemData.BuySellOrdersAnalysis.SellOrderCount * 0.35) &&
                itemData.BuySellOrdersAnalysis.VolumeBought < (itemData.MarketStats.Sell.Volume * 0.35) &&
                itemData.BuySellOrdersAnalysis.ProfitMargin > (_profitMarginPct / 100.0m) &&
                itemData.BuySellOrdersAnalysis.Profit > _minProfit &&
                (itemData.BuySellOrdersAnalysis.TotalSpent < _iskLimit || _iskLimit == 0.0m))
            {
                if (_priceHistory == false)
                {
                    return true;
                }
                else if (_priceHistory == true && itemData.History != null)
                {
                    int priceHistoryMatch = 0;

                    foreach (PriceHistory history in itemData.History.PriceHistory)
                    {
                        if (itemData.BuySellOrdersAnalysis.SellOrdersBought < history.OrdersMovement &&
                            itemData.BuySellOrdersAnalysis.VolumeBought < history.VolumeMovement &&
                            itemData.BuySellOrdersAnalysis.SellPrice > history.MaxPrice * 0.9m &&
                            itemData.BuySellOrdersAnalysis.SellPrice < history.MaxPrice * 1.1m)
                        {
                            priceHistoryMatch++;    
                        }
                    }

                    if (itemData.History.PriceHistory.Count() > 0 && priceHistoryMatch / itemData.History.PriceHistory.Count() > 0.5)
                        return true;
                }
            }

            return false;
        }

        public bool ShouldCreateBuyOrder(ItemData itemData)
        {
            try
            {
                if (itemData == null || itemData.CreateBuyOrdersAnalysis == null || itemData.MarketStats == null)
                    return false;

                if (itemData.MarketStats.Sell.Min > itemData.MarketStats.Buy.Max &&
                    itemData.CreateBuyOrdersAnalysis.ProfitMargin > (_profitMarginPct / 100.0m) &&
                    itemData.CreateBuyOrdersAnalysis.Profit > _minProfit)
                {
                    if (_priceHistory == false)
                    {
                        return true;
                    }
                    else if(_priceHistory == true && itemData.History.PriceHistory != null)
                    {
                        int priceHistoryMinMatch = 0;
                        int priceHistoryMaxMatch = 0;

                        foreach (PriceHistory history in itemData.History.PriceHistory)
                        {
                            if (itemData.CreateBuyOrdersAnalysis.BuyPrice > history.MinPrice * 0.8m)
                            {
                                priceHistoryMinMatch++;
                            }
                            if (itemData.CreateBuyOrdersAnalysis.SellPrice < history.MaxPrice * 1.2m)
                            {
                                priceHistoryMaxMatch++;
                            }
                        }

                        if (itemData.History.PriceHistory.Count() > 0 && 
                            priceHistoryMinMatch / itemData.History.PriceHistory.Count() > 0.5 &&
                            priceHistoryMaxMatch / itemData.History.PriceHistory.Count() > 0.5)
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }

            return false;
        }

        public void FindSellOrdersToBuy(bool clearExistingOrders)
        {
            if (backgroundWorker.IsBusy == true)
                return;

            GetSavedSettings();

            if (clearExistingOrders == true)
                ClearSellOrdersToBuy();

            foreach (ItemData itemData in _itemData)
            {
                CheckAndBuySellOrder(itemData);
            }
        }

        public void FindBuyOrdersToCreate(bool clearExistingOrders)
        {
            try
            {
                if (backgroundWorker.IsBusy == true)
                    return;

                GetSavedSettings();

                if (clearExistingOrders == true)
                    ClearBuyOrdersToCreate();

                foreach (ItemData itemData in _itemData)
                {
                    CheckAndCreateBuyOrder(itemData);
                }
            }
            catch(Exception ex)
            {
                if(ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public void BuySellOrder(ItemData itemData)
        {
            if (itemData == null || itemData.BuySellOrdersAnalysis == null || itemData.MarketStats == null)
                return;

            int rowIndex = buySellOrdersGrid.Rows.Add();

            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_Name"].Value = itemData.Name;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_TypeId"].Value = itemData.TypeId;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_InitialBuyPrice"].Value = itemData.MarketStats.Sell.Min;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OpenSellVolume"].Value = itemData.MarketStats.Sell.Volume;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OpenSellOrders"].Value = itemData.BuySellOrdersAnalysis.SellOrderCount;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_FinalSellPrice"].Value = itemData.BuySellOrdersAnalysis.SellPrice;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OrdersBought"].Value = itemData.BuySellOrdersAnalysis.SellOrdersBought;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_VolumeBought"].Value = itemData.BuySellOrdersAnalysis.VolumeBought;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_TotalSpent"].Value = itemData.BuySellOrdersAnalysis.TotalSpent;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_Profit"].Value = itemData.BuySellOrdersAnalysis.Profit;
            buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_ProfitMargin"].Value = itemData.BuySellOrdersAnalysis.ProfitMargin;

            if (itemData.History != null)
            {
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OrdersMovement"].Value = itemData.History.AvgOrders;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_VolumeMovement"].Value = itemData.History.AvgVolume;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_AvgPrice"].Value = itemData.History.AvgPrice;
            }
        }

        public void CreateBuyOrder(ItemData itemData)
        {
            try
            {
                if (itemData == null || itemData.CreateBuyOrdersAnalysis == null || itemData.MarketStats == null)
                    return;

                int rowIndex = createBuyOrdersGrid.Rows.Add();

                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_Name"].Value = itemData.Name;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_TypeId"].Value = itemData.TypeId;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_MinSellOrder"].Value = itemData.MarketStats.Sell.Min;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_MaxBuyOrder"].Value = itemData.MarketStats.Buy.Max;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_Profit"].Value = itemData.CreateBuyOrdersAnalysis.Profit;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_ProfitMargin"].Value = itemData.CreateBuyOrdersAnalysis.ProfitMargin;

                if (itemData.History != null)
                {
                    createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_OrdersMovement"].Value = itemData.History.AvgOrders;
                    createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_VolumeMovement"].Value = itemData.History.AvgVolume;
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public void CheckAndBuySellOrder(ItemData itemData)
        {
            if (itemData == null)
                return;

            if (ShouldBuySellOrder(itemData) == true)
            {
                BuySellOrder(itemData);
            }
        }

        public void CheckAndCreateBuyOrder(ItemData itemData)
        {
            try
            {
                if (itemData == null)
                    return;

                if (ShouldCreateBuyOrder(itemData) == true)
                {
                    CreateBuyOrder(itemData);
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public int GetTypeIdByName(string name)
        {
            Item item = _allItems.FirstOrDefault(i=>i.Name == name);

            if(item != null)
            {
                return item.TypeId;
            }

            return 0;
        }
        
        private void browseMarketDataButton_Click(object sender, EventArgs e)
        {
            DialogResult result = _openfileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                marketItemsFileTextBox.Text = _openfileDialog.FileName;
            }
        }

        private void typeIdsBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = _openfileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                typeIdsFileTextBox.Text = _openfileDialog.FileName;
            }
        }

        private void marketItemsTextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MarketDataFile = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        private void typeIdsTextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TypeIdsFile = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        private void salesTaxTextChanged(object sender, EventArgs e)
        {
            decimal salesTax = 0.0m;

            if (((MetroTextBox)sender).Text != "")
                salesTax = decimal.Parse(((MetroTextBox)sender).Text);

            Properties.Settings.Default.SalesTax = salesTax; 
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _salesTax = salesTax;
        }

        private void brokersFeeTextChanged(object sender, EventArgs e)
        {
            decimal brokersFee = 0.0m;

            if (((MetroTextBox)sender).Text != "")
                brokersFee = decimal.Parse(((MetroTextBox)sender).Text);

            Properties.Settings.Default.BrokersFee = brokersFee;
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _brokersFee = brokersFee;
            
        }

        private void iskLimitTextChanged(object sender, EventArgs e)
        {
            decimal iskLimit = 0.0m;

            if (((MetroTextBox)sender).Text != "")
                iskLimit = decimal.Parse(((MetroTextBox)sender).Text);

            Properties.Settings.Default.ISKLimit = iskLimit;
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _iskLimit = iskLimit;
        }

        private void minProfitTextChanged(object sender, EventArgs e)
        {
            decimal minProfit = 0.0m;

            if (((MetroTextBox)sender).Text != "")
                minProfit = decimal.Parse(((MetroTextBox)sender).Text);

            Properties.Settings.Default.MinProfit = minProfit;
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _minProfit = minProfit;
        }

        private void profitMarginPctTextChanged(object sender, EventArgs e)
        {
            decimal profitMargin = 0.0m;

            if (((MetroTextBox)sender).Text != "")
                profitMargin = decimal.Parse(((MetroTextBox)sender).Text);

            Properties.Settings.Default.ProfitMarginPct = profitMargin;
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _profitMarginPct = profitMargin;
        }

        private void priceHistoryCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PriceHistory = priceHistoryCheckBox.Checked;
            Properties.Settings.Default.Save();

            if (backgroundWorker.IsBusy)
                _optionsChanged = backgroundWorker.IsBusy;
            else
                _priceHistory = priceHistoryCheckBox.Checked;
        }

        private void marketDataButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy == false)
            {
                ClearSellOrdersToBuy();
                ClearBuyOrdersToCreate();

                backgroundWorker.RunWorkerAsync();
            }
            else
                MessageBox.Show("Market items already loading. Please wait till the operation is complete");
        }

        private void findBuyOrdersButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy == false)
            {
                FindSellOrdersToBuy(true);
                FindBuyOrdersToCreate(true);
            }
            else
                MessageBox.Show("Market items are loading. Please wait till the operation is complete");
        }

        private void cancelMarketDataButton_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy == true)
                _cancelMarketLoad = true;
        }

        private void createBuyOrdersButton_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            LoadItems();

            int itemCount = _itemData.Count();
            int itemsDone = 0;
            
            foreach (ItemData itemData in _itemData)
            {
                if (_cancelMarketLoad == true)
                    return;

                if (itemData.TypeId != 0)
                {
                    int typeId = itemData.TypeId;
                        
                    if(typeId == 0)
                    {
                        continue;
                    }

                    MarketStats marketStats = GetMarketStats(typeId);
                    QuickLook quicklook = GetQuickLook(typeId);
                    History history = GetHistory(typeId);

                    long volumeMovement = (history != null) ? history.AvgVolume : 0;

                    BuySellOrdersAnalysis buySellOrdersAnalysis = AnaylzeBuySellOrders(quicklook.SellOrders);
                    CreateBuyOrdersAnalysis createBuyOrdesAnalysis = AnaylzeCreateBuyOrders(quicklook.SellOrders, quicklook.BuyOrders, volumeMovement);

                    itemData.MarketStats = marketStats;
                    itemData.History = history;
                    itemData.BuySellOrdersAnalysis = buySellOrdersAnalysis;
                    itemData.CreateBuyOrdersAnalysis = createBuyOrdesAnalysis;
                    itemData.TypeId = typeId;
                    itemData.Updated = true;

                    lock (_updatedItems)
                    { 
                        _updatedItems.Add(itemData);
                    }
                }

                itemsDone++;
                int percentDone = (int)(((decimal)itemsDone / (decimal)itemCount) * 100.0m);

                backgroundWorker.ReportProgress(percentDone);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            try
            {
                lock (_updatedItems)
                {
                    foreach(ItemData updatedItemData in _updatedItems)
                    {
                        _uiUpdatedItems.Add(updatedItemData);
                    }

                    _updatedItems.Clear();
                }

                ItemData itemData = null;
                
                if(_uiUpdatedItems.Count > 0)
                { 
                    itemData = _uiUpdatedItems[0];
                    _uiUpdatedItems.RemoveAt(0);
                }

                if (itemData != null && itemData.Updated == true)
                {
                    CheckAndBuySellOrder(itemData);
                    CheckAndCreateBuyOrder(itemData);

                    itemData.Updated = false;
                }
            }
            catch(Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
            

            itemsProgressBar.Value = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (_optionsChanged)
            {
                FindSellOrdersToBuy(true);
                FindBuyOrdersToCreate(true);
                _optionsChanged = false;
            }

            MessageBox.Show("Complete");
        }
    }
}
