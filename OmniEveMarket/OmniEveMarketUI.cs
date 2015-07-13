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
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using MetroFramework.Forms;
using MetroFramework.Controls;
using Newtonsoft.Json;

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

        private List<Item> _items = new List<Item>();
        private List<Item> _updatedItems = new List<Item>();
        private List<Item> _uiUpdatedItems = new List<Item>();

        private int _crestRequests = 0;
        private System.Timers.Timer _crestTimer = new System.Timers.Timer();
        
        BackgroundWorker backgroundWorker = new BackgroundWorker();

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

        private void IncrementCrestRequests()
        {
            if (_crestRequests == 0)
            {
                _crestTimer.Interval = 1000;
                _crestTimer.Elapsed += CrestTimerElapsed;
                _crestTimer.AutoReset = false;
                _crestTimer.Start();
            }

            while (CanMakeCrestRequest() == false) { /* Sit and wait till we can make a crest request*/ }
            
            _crestRequests++;
        }

        private bool CanMakeCrestRequest()
        {
            return _crestRequests < 29;
        }

        private void LoadMarketTypes(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = null;

            IncrementCrestRequests();

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
                return;
            }

            Stream resStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(resStream, Encoding.UTF8);
            string resString = reader.ReadToEnd();

            JsonTypesPage page = JsonConvert.DeserializeObject<JsonTypesPage>(resString);

            foreach (JsonTypesItem jsonItem in page.Items)
                _items.Add(new Item(jsonItem.Type.Id, jsonItem.Type.Name));

            if(page.Next != null && page.Next.Href.Count() >= 0)
                LoadMarketTypes(page.Next.Href);
        }

        public void EnableSorting(MetroGrid grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
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

        
        public void FindSellOrdersToBuy(bool clearExistingOrders)
        {
            try
            {
                if (backgroundWorker.IsBusy == true)
                    return;

                GetSavedSettings();

                if (clearExistingOrders == true)
                    ClearSellOrdersToBuy();

                foreach (Item item in _items)
                {
                    CheckAndBuySellOrder(item);
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
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

                foreach (Item item in _items)
                {
                    CheckAndCreateBuyOrder(item);
                }
            }
            catch(Exception ex)
            {
                if(ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public void BuySellOrder(Item item)
        {
            try
            {
                if (item == null || item.BuySellOrders == null || item.MarketStats == null)
                    return;

                int rowIndex = buySellOrdersGrid.Rows.Add();

                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_Name"].Value = item.Name;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_TypeId"].Value = item.TypeId;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_InitialBuyPrice"].Value = item.MarketStats.Sell.Min;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OpenSellVolume"].Value = item.MarketStats.Sell.Volume;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OpenSellOrders"].Value = item.BuySellOrders.SellOrderCount;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_FinalSellPrice"].Value = item.BuySellOrders.SellPrice;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OrdersBought"].Value = item.BuySellOrders.SellOrdersBought;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_VolumeBought"].Value = item.BuySellOrders.VolumeBought;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_TotalSpent"].Value = item.BuySellOrders.TotalSpent;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_Profit"].Value = item.BuySellOrders.Profit;
                buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_ProfitMargin"].Value = item.BuySellOrders.ProfitMargin;

                if (item.PriceHistory != null)
                {
                    buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_OrdersMovement"].Value = item.PriceHistory.AvgOrders;
                    buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_VolumeMovement"].Value = item.PriceHistory.AvgVolume;
                    buySellOrdersGrid.Rows[rowIndex].Cells["BuySellOrders_AvgPrice"].Value = item.PriceHistory.AvgPrice;
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public void CreateBuyOrder(Item item)
        {
            try
            {
                if (item == null || item.CreateBuyOrder == null || item.MarketStats == null)
                    return;

                int rowIndex = createBuyOrdersGrid.Rows.Add();

                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_Name"].Value = item.Name;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_TypeId"].Value = item.TypeId;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_MinSellOrder"].Value = item.MarketStats.Sell.Min;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_MaxBuyOrder"].Value = item.MarketStats.Buy.Max;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_Profit"].Value = item.CreateBuyOrder.Profit;
                createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_ProfitMargin"].Value = item.CreateBuyOrder.ProfitMargin;

                if (item.PriceHistory != null)
                {
                    createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_TotalProfit"].Value = item.PriceHistory.AvgVolume * item.CreateBuyOrder.Profit;
                    createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_OrdersMovement"].Value = item.PriceHistory.AvgOrders;
                    createBuyOrdersGrid.Rows[rowIndex].Cells["CreateBuyOrders_VolumeMovement"].Value = item.PriceHistory.AvgVolume;
                }
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        public void CheckAndBuySellOrder(Item item)
        {
            if (item != null && item.ShouldBuySellOrder(_minProfit, _profitMarginPct, _iskLimit, _priceHistory) == true)
                BuySellOrder(item);
        }

        public void CheckAndCreateBuyOrder(Item item)
        {
            if (item != null && item.ShouldCreateBuyOrder(_minProfit, _profitMarginPct, _iskLimit, _priceHistory) == true)
                CreateBuyOrder(item);
        }

        private void CrestTimerElapsed(Object source, ElapsedEventArgs e)
        {
            _crestRequests = 0;
        }

        private void ShowBuyersAndSellers(int typeId)
        {
            try
            {
                sellersGrid.Rows.Clear();
                buyersGrid.Rows.Clear();

                Item item = _items.Find(i => i.TypeId == typeId);

                QuickLook quickLook = item.QuickLook;
                List<Order> sellers = quickLook.SellOrders;
                List<Order> buyers = quickLook.BuyOrders;

                foreach (Order order in sellers)
                    sellersGrid.Rows.Add(order.VolRemain.ToString(), order.Price.ToString());

                foreach (Order order in buyers)
                    buyersGrid.Rows.Add(order.VolRemain.ToString(), order.Price.ToString());
            }
            catch(Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        private void ShowPriceHistory(int typeId)
        {
            try
            {
                priceHistoryGrid.Rows.Clear();

                Item item = _items.Find(i => i.TypeId == typeId);

                PriceHistory priceHistory = item.PriceHistory;

                List<PriceHistory.Day> lastNinety = priceHistory.Days.Take(90).ToList();

                foreach (PriceHistory.Day day in lastNinety)
                    priceHistoryGrid.Rows.Add(day.Date.ToString(), day.OrderCount.ToString(), day.Volume.ToString(), day.MinPrice.ToString(), day.MaxPrice.ToString(), day.AvgPrice.ToString());
            }
            catch (Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
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
            MessageBox.Show("Marker 3");

            if (backgroundWorker.IsBusy == true)
                _cancelMarketLoad = true;
        }

        private void createBuyOrdersButton_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                LoadMarketTypes("https://public-crest.eveonline.com/market/types/");

                int itemCount = _items.Count();
                int itemsDone = 0;

                foreach (Item item in _items)
                {
                    if (_cancelMarketLoad == true)
                        return;

                    if (item.TypeId != 0)
                    {
                        int typeId = item.TypeId;

                        if (typeId == 0)
                        {
                            continue;
                        }

                        MarketStats marketStats = new MarketStats(typeId);
                        marketStats.LoadFromEveCentral();

                        QuickLook quickLook = new QuickLook(typeId);
                        quickLook.LoadFromEveCentral();

                        PriceHistory priceHistory = new PriceHistory(typeId);
                        IncrementCrestRequests();
                        priceHistory.LoadFromEveCrest();

                        Analysis.BuySellOrders buySellOrders = new Analysis.BuySellOrders(_salesTax, _brokersFee);
                        buySellOrders.AnalyzeOrders(quickLook.SellOrders);

                        Analysis.CreateBuyOrder createBuyOrder = new Analysis.CreateBuyOrder(_salesTax, _brokersFee);
                        createBuyOrder.AnalyzeOrders(quickLook.SellOrders, quickLook.BuyOrders);

                        item.MarketStats = marketStats;
                        item.PriceHistory = priceHistory;
                        item.QuickLook = quickLook;
                        item.BuySellOrders = buySellOrders;
                        item.CreateBuyOrder = createBuyOrder;
                        item.TypeId = typeId;
                        item.Updated = true;

                        lock (_updatedItems)
                        {
                            _updatedItems.Add(item);
                        }
                    }

                    itemsDone++;
                    int percentDone = (int)(((decimal)itemsDone / (decimal)itemCount) * 100.0m);

                    backgroundWorker.ReportProgress(percentDone);
                }
            }
            catch(Exception ex)
            {
                if (ShowDebugMessages == true)
                    MessageBox.Show(ex.Message);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            try
            {
                lock (_updatedItems)
                {
                    foreach(Item updatedItem in _updatedItems)
                    {
                        _uiUpdatedItems.Add(updatedItem);
                    }

                    _updatedItems.Clear();
                }

                Item item = null;
                
                if(_uiUpdatedItems.Count > 0)
                { 
                    item = _uiUpdatedItems[0];
                    _uiUpdatedItems.RemoveAt(0);
                }

                if (item != null && item.Updated == true)
                {
                    CheckAndBuySellOrder(item);
                    CheckAndCreateBuyOrder(item);

                    item.Updated = false;
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

        private void createBuyOrdersGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            MetroGrid grid = (MetroGrid)sender;
            int typeId = int.Parse(grid.CurrentRow.Cells["CreateBuyOrders_TypeId"].Value.ToString());

            ShowBuyersAndSellers(typeId);
            ShowPriceHistory(typeId);
        }

        private void buySellOrdersGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            MetroGrid grid = (MetroGrid)sender;
            int typeId = int.Parse(grid.CurrentRow.Cells["BuySellOrders_TypeId"].Value.ToString());

            ShowBuyersAndSellers(typeId);
            ShowPriceHistory(typeId);
        }
    }
}
