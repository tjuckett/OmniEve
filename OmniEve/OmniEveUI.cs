using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using System.Windows.Forms;
using MetroFramework.Forms;

namespace OmniEve
{
    using DirectEve;
    using OmniEveModules.Caching;
    using OmniEveModules.Actions;
    using OmniEveModules.Logging;

    public partial class OmniEveUI : MetroForm
    {
        private enum Mode
        {
            Idle,
            Automatic,
            Manual
        }

        private OmniEve _omniEve = null;
        private Mode _mode = Mode.Idle;
        private System.Timers.Timer _timer = new System.Timers.Timer();
        
        public OmniEveUI(OmniEve omniEve)
        {
            _omniEve = omniEve;

            InitializeComponent();

            Logging.TextBoxWriter = new TextBoxWriter(logTextBox);
        }

        private void CheckState()
        {
            // If we are in the idle state then clean up the action queue before we allow the user to re-enable the controls
            if (IsInIdleState() == true)
            {
                _omniEve.CleanUpActions();
                _mode = Mode.Idle;

                EnableControls();
            }
            else
            {
                DisableControls();
            }
        }

        private bool IsInIdleState()
        {
            if (_mode == Mode.Manual)
            {
                return (_omniEve != null && _omniEve.IsActionQueueEmpty() == true);
            }
            else if (_mode == Mode.Automatic)
            {
                return (_timer.Enabled == false);
            }

            return true;
        }

        private void MySellOrdersUpdated(List<DirectOrder> mySellOrders)
        {
            Cache.Instance.OnMySellOrdersUpdated -= MySellOrdersUpdated;

            Logging.Log("OmniEveUI:MySellOrdersUpdated", "Clearing existing grid of orders", Logging.White);
            sellingGrid.Rows.Clear();

            Logging.Log("OmniEveUI:MySellOrdersUpdated", "Filling grid of updated orders", Logging.White);

            foreach (DirectOrder order in mySellOrders)
            {
                //DirectInvType deType;
                //Cache.Instance.DirectEve.InvTypes.TryGetValue(order.TypeId, out deType);
                string name = "";//deType.TypeName;
                string quantity = order.VolumeRemaining.ToString() + "/" + order.VolumeEntered.ToString();
                
                Logging.Log("OmniEveUI:MySellOrdersUpdated", "Order Name - " + name + " Quantity - " + quantity + " Price - " + order.Price + " Station - " + order.StationId + " Region - " + order.RegionId, Logging.White);

                sellingGrid.AllowUserToAddRows = true;
                int index = sellingGrid.Rows.Add();
                sellingGrid.Rows[index].Cells["Selling_Select"].Value = false;
                sellingGrid.Rows[index].Cells["Selling_TypeId"].Value = order.TypeId;
                sellingGrid.Rows[index].Cells["Selling_OrderId"].Value = order.OrderId;
                sellingGrid.Rows[index].Cells["Selling_Name"].Value = name;
                sellingGrid.Rows[index].Cells["Selling_Quantity"].Value = quantity;
                sellingGrid.Rows[index].Cells["Selling_OrderPrice"].Value = order.Price.ToString();
                sellingGrid.Rows[index].Cells["Selling_Station"].Value = Cache.Instance.DirectEve.Stations[order.StationId].Name;
                sellingGrid.Rows[index].Cells["Selling_Region"].Value = Cache.Instance.DirectEve.Regions[order.RegionId].Name;
                sellingGrid.AllowUserToAddRows = false;

                // Add an action for each sell order and get the updated market values
                if (_omniEve != null && 
                    (sellingGrid.Rows[index].Cells["Selling_MarketPrice"].Value == null || 
                     sellingGrid.Rows[index].Cells["Selling_MarketPrice"].Value.ToString().Count() <= 0))
                {
                    MarketInfo marketInfo = new MarketInfo();
                    marketInfo.OnMarketInfoActionFinished += MarketInfoFinished;
                    marketInfo.TypeId = order.TypeId;
                    _omniEve.AddAction(marketInfo);
                }
            }

            CheckState();
        }

        private void MarketInfoFinished(MarketItemInfo marketInfo)
        {
            List<DirectOrder> sellOrders = marketInfo.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderBy(o => o.Price).ToList();
            List<DirectOrder> buyOrders = marketInfo.BuyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderByDescending(o => o.Price).ToList();
            DirectOrder sellOrder = sellOrders.FirstOrDefault();
            DirectOrder buyOrder = buyOrders.FirstOrDefault();

            UpdateMarketInfoSellOrder(marketInfo, sellOrder);
            UpdateMarketInfoBuyOrder(marketInfo, buyOrder);

            CheckState();
        }

        private void UpdateMarketInfoSellOrder(MarketItemInfo marketInfo, DirectOrder sellOrder)
        {
            if (sellOrder != null)
            {
                foreach (DataGridViewRow row in sellingGrid.Rows)
                {
                    if (int.Parse(row.Cells["Selling_TypeId"].Value.ToString()) == marketInfo.TypeId)
                    {
                        Logging.Log("OmniEveUI:MarketInfoFinished", "Row has been found, adding markinfo to row", Logging.White);

                        long orderId = (long)row.Cells["Selling_OrderId"].Value;
                        string orderPriceStr = (string)row.Cells["Selling_OrderPrice"].Value;
                        string marketPriceStr = sellOrder.Price.ToString();

                        row.Cells["Selling_MarketPrice"].Value = marketPriceStr;
                        double orderPrice = double.Parse(orderPriceStr);
                        if (orderPrice > sellOrder.Price)
                        {
                            double priceDifference = orderPrice - sellOrder.Price;
                            double priceDifferencePct = priceDifference / orderPrice;

                            if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                            {
                                row.Cells["Selling_Select"].Value = true;
                                row.DefaultCellStyle.BackColor = Color.Red;
                                row.DefaultCellStyle.ForeColor = Color.Black;

                                if (_mode == Mode.Automatic)
                                    ModifySellOrder(orderId, orderPriceStr, marketPriceStr);
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = Color.Yellow;
                                row.DefaultCellStyle.ForeColor = Color.Black;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateMarketInfoBuyOrder(MarketItemInfo marketInfo, DirectOrder buyOrder)
        {
            if (buyOrder != null)
            {
                foreach (DataGridViewRow row in buyingGrid.Rows)
                {
                    if (int.Parse(row.Cells["Buying_TypeId"].Value.ToString()) == marketInfo.TypeId)
                    {
                        Logging.Log("OmniEveUI:MarketInfoFinished", "Row has been found, adding market info to row", Logging.White);

                        long orderId = (long)row.Cells["Buying_OrderId"].Value;
                        string orderPriceStr = (string)row.Cells["Buying_OrderPrice"].Value;
                        string marketPriceStr = buyOrder.Price.ToString();

                        row.Cells["Buying_MarketPrice"].Value = marketPriceStr;
                        double orderPrice = double.Parse(orderPriceStr);
                        if (orderPrice < buyOrder.Price)
                        {
                            double priceDifference = buyOrder.Price - orderPrice;
                            double priceDifferencePct = priceDifference / orderPrice;

                            if (priceDifferencePct < 0.05 && priceDifference < 5000000)
                            {
                                row.Cells["Buying_Select"].Value = true;
                                row.DefaultCellStyle.BackColor = Color.Red;
                                row.DefaultCellStyle.ForeColor = Color.Black;

                                if (_mode == Mode.Automatic)
                                    ModifyBuyOrder(orderId, orderPriceStr, marketPriceStr);
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = Color.Yellow;
                                row.DefaultCellStyle.ForeColor = Color.Black;
                            }
                        }
                    }
                }
            }
        }

        private void MyBuyOrdersUpdated(List<DirectOrder> myBuyOrders)
        {
            Cache.Instance.OnMyBuyOrdersUpdated -= MyBuyOrdersUpdated;

            Logging.Log("OmniEveUI:MyBuyOrdersUpdated", "Clearing existing grid of orders", Logging.White);
            buyingGrid.Rows.Clear();

            Logging.Log("OmniEveUI:MyBuyOrdersUpdated", "Filling grid of updated orders", Logging.White);

            foreach (DirectOrder order in myBuyOrders)
            {
                //DirectInvType deType;
                //Cache.Instance.DirectEve.InvTypes.TryGetValue(order.TypeId, out deType);
                string name = "";//deType.TypeName;
                string quantity = order.VolumeRemaining.ToString() + "/" + order.VolumeEntered.ToString();

                Logging.Log("OmniEveUI", "Order Type - " + name
                          + " Quantity - " + quantity
                          + " Price - " + order.Price
                          + " Station - " + order.StationId
                          + " Region - " + order.RegionId
                          + " Range - " + order.Range
                          + " Min Volume - " + order.MinimumVolume, Logging.White);

                buyingGrid.AllowUserToAddRows = true;
                int index = buyingGrid.Rows.Add();
                buyingGrid.Rows[index].Cells["Buying_Select"].Value = false;
                buyingGrid.Rows[index].Cells["Buying_TypeId"].Value = order.TypeId;
                buyingGrid.Rows[index].Cells["Buying_OrderId"].Value = order.OrderId;
                buyingGrid.Rows[index].Cells["Buying_Name"].Value = name;
                buyingGrid.Rows[index].Cells["Buying_Quantity"].Value = quantity;
                buyingGrid.Rows[index].Cells["Buying_OrderPrice"].Value = order.Price.ToString();
                buyingGrid.Rows[index].Cells["Buying_Station"].Value = Cache.Instance.DirectEve.Stations[order.StationId].Name;
                buyingGrid.Rows[index].Cells["Buying_Region"].Value = Cache.Instance.DirectEve.Regions[order.RegionId].Name;
                buyingGrid.Rows[index].Cells["Buying_Range"].Value = order.Range.ToString();
                buyingGrid.Rows[index].Cells["Buying_MinVolume"].Value = order.MinimumVolume.ToString();
                buyingGrid.AllowUserToAddRows = false;

                // Add an action for each Buy order and get the updated market values
                if (_omniEve != null && 
                    (buyingGrid.Rows[index].Cells["Buying_MarketPrice"].Value == null || 
                     buyingGrid.Rows[index].Cells["Buying_MarketPrice"].Value.ToString().Count() <= 0))
                {
                    MarketInfo marketInfo = new MarketInfo();
                    marketInfo.OnMarketInfoActionFinished += MarketInfoFinished;
                    marketInfo.TypeId = order.TypeId;
                    _omniEve.AddAction(marketInfo);
                }
            }

            CheckState();
        }

        private void ModifySellOrder(long orderId, string orderPriceStr, string marketPriceStr)
        {
            List<DirectOrder> orders = Cache.Instance.MySellOrders;

            Logging.Log("OmniEveUI:ModifySellOrder", "Getting cached sell orders", Logging.Debug);

            if (marketPriceStr == null || marketPriceStr.Count() <= 0)
                Logging.Log("OmniEveUI:ModifySellOrder", "Can't modify order, the market price is empty or hasn't been filled out yet for OrderId - " + orderId, Logging.Debug);

            // To get around the double percision problem we first convert the strings to decimals, do the modification, then convert to double
            decimal orderPrice = decimal.Parse(orderPriceStr);
            decimal marketPrice = decimal.Parse(marketPriceStr);
            decimal newPrice = marketPrice - 0.01m;

            if (orders != null)
            {
                Logging.Log("OmniEveUI:ModifySellOrder", "Getting order with OrderId - " + orderId, Logging.Debug);

                DirectOrder myOrder = orders.First(o => o.OrderId == orderId);

                if (myOrder != null && marketPrice < orderPrice)
                {
                    string newPriceStr = newPrice.ToString();
                    double newPriceDbl = double.Parse(newPriceStr);

                    Logging.Log("OmniEveUI:ModifySellOrders", "Modifying order with OrderId - " + myOrder.OrderId + " OldPrice - " + myOrder.Price + " MarketPrice - " + marketPrice + " NewPrice - " + newPriceDbl, Logging.Debug);

                    ModifyOrder modifyOrder = new ModifyOrder();
                    modifyOrder.OrderId = myOrder.OrderId;
                    modifyOrder.IsBid = myOrder.IsBid;
                    modifyOrder.Price = newPriceDbl;
                    modifyOrder.OnModifyOrderActionFinished += ModifySellOrderFinished;
                    _omniEve.AddAction(modifyOrder);
                }
            }
            else
            {
                Logging.Log("OmniEveUI:ModifySellOrder", "No orders matching OrderId - " + orderId, Logging.Debug);
            }
        }

        private void ModifySellOrders()
        {
            if (_omniEve != null)
            {
                foreach (DataGridViewRow row in sellingGrid.Rows)
                {
                    try
                    {
                        if (Convert.ToBoolean(row.Cells["Selling_Select"].Value) == true)
                        { 
                            long orderId = (long)row.Cells["Selling_OrderId"].Value;
                            string orderPriceStr = (string)row.Cells["Selling_OrderPrice"].Value;
                            string marketPriceStr = (string)row.Cells["Selling_MarketPrice"].Value;

                            ModifySellOrder(orderId, orderPriceStr, marketPriceStr);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logging.Log("OmniEveUI:ModifySellOrders", "Exception [" + ex + "]", Logging.Debug);
                    }
                }
            }

            CheckState();
        }

        private void ModifySellOrderFinished(long orderId, double price)
        {
            foreach (DataGridViewRow row in sellingGrid.Rows)
            {
                if ((long)row.Cells["Selling_OrderId"].Value == orderId)
                {
                    row.Cells["Selling_OrderPrice"].Value = price.ToString();
                    row.Cells["Selling_MarketPrice"].Value = price.ToString();
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }

            CheckState();
        }

        private void ModifyBuyOrder(long orderId, string orderPriceStr, string marketPriceStr)
        {
            List<DirectOrder> orders = Cache.Instance.MyBuyOrders;

            Logging.Log("OmniEveUI:ModifyBuyOrder", "Getting cached buy orders", Logging.Debug);

            if (marketPriceStr == null || marketPriceStr.Count() <= 0)
                Logging.Log("OmniEveUI:ModifyBuyOrder", "Can't modify order, the market price is empty or hasn't been filled out yet for OrderId - " + orderId, Logging.Debug);

            // To get around the double percision problem we first convert the strings to decimals, do the modification, then convert to double
            decimal orderPrice = decimal.Parse(orderPriceStr);
            decimal marketPrice = decimal.Parse(marketPriceStr);
            decimal newPrice = marketPrice + 0.01m;

            if (orders != null)
            {
                Logging.Log("OmniEveUI:ModifyBuyOrder", "Getting order with OrderId - " + orderId, Logging.Debug);

                DirectOrder myOrder = orders.First(o => o.OrderId == orderId);

                if (myOrder != null && marketPrice > orderPrice)
                {
                    string newPriceStr = newPrice.ToString();
                    double newPriceDbl = double.Parse(newPriceStr);

                    Logging.Log("OmniEveUI:ModifyBuyOrder", "Modifying order with OrderId - " + myOrder.OrderId + " OldPrice - " + myOrder.Price + " MarketPrice - " + marketPrice + " NewPrice - " + newPriceDbl, Logging.Debug);

                    ModifyOrder modifyOrder = new ModifyOrder();
                    modifyOrder.OrderId = myOrder.OrderId;
                    modifyOrder.IsBid = myOrder.IsBid;
                    modifyOrder.Price = newPriceDbl;
                    modifyOrder.OnModifyOrderActionFinished += ModifyBuyOrderFinished;
                    _omniEve.AddAction(modifyOrder);
                }
            }
            else
            {
                Logging.Log("OmniEveUI:ModifyBuyOrder", "No orders matching OrderId - " + orderId, Logging.Debug);
            }
        }

        private void ModifyBuyOrders()
        {
            if (_omniEve != null)
            {
                foreach (DataGridViewRow row in buyingGrid.Rows)
                {
                    try
                    {
                        if (Convert.ToBoolean(row.Cells["Buying_Select"].Value) == true)
                        {
                            Logging.Log("OmniEveUI:ModifyBuyOrders", "Row selected, modifying order", Logging.Debug);

                            long orderId = (long)row.Cells["Buying_OrderId"].Value;
                            string orderPriceStr = (string)row.Cells["Buying_OrderPrice"].Value;
                            string marketPriceStr = (string)row.Cells["Buying_MarketPrice"].Value;

                            ModifyBuyOrder(orderId, orderPriceStr, marketPriceStr);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logging.Log("OmniEveUI:ModifyBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                    }
                }
            }

            CheckState();
        }

        private void ModifyBuyOrderFinished(long orderId, double price)
        {
            foreach (DataGridViewRow row in buyingGrid.Rows)
            {
                if ((long)row.Cells["Buying_OrderId"].Value == orderId)
                {
                    row.Cells["Buying_OrderPrice"].Value = price.ToString();
                    row.Cells["Buying_MarketPrice"].Value = price.ToString();
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }

            CheckState();
        }

        private void EnableControls()
        {
            refreshButton.Enabled = true;
            modifyButton.Enabled = true;
            autoStartButton.Enabled = true;
            autoStopButton.Enabled = true;
            autoSecondsTextBox.Enabled = true;
        }

        private void DisableControls()
        {
            if (_mode == Mode.Manual)
            {
                refreshButton.Enabled = false;
                modifyButton.Enabled = false;
                autoStartButton.Enabled = false;
                autoStopButton.Enabled = false;
                autoSecondsTextBox.Enabled = false;
            }
            else if (_mode == Mode.Automatic)
            {
                refreshButton.Enabled = false;
                modifyButton.Enabled = false;
                autoStartButton.Enabled = false;
                autoSecondsTextBox.Enabled = false;
            }
        }

        private void AutoTimerElapsed(Object source, ElapsedEventArgs e)
        {
            // Only refresh orders when the action queue is empty, otherwise wait till the next time or lengthen the time between events
            if(_omniEve != null && _omniEve.IsActionQueueEmpty() == true)
            {
                RefreshOrders();
            }
        }

        private void RefreshOrders()
        {
            if (_omniEve != null)
            {
                sellingGrid.Rows.Clear();
                buyingGrid.Rows.Clear();

                Cache.Instance.OnMySellOrdersUpdated += MySellOrdersUpdated;
                Cache.Instance.OnMyBuyOrdersUpdated += MyBuyOrdersUpdated;
                MyOrders myOrders = new MyOrders();
                _omniEve.AddAction(myOrders);
            }
        }

        public void ModifyOrders()
        {
            ModifySellOrders();
            ModifyBuyOrders();
        }

        private void OmniEveUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _omniEve.Cleanup = true;

            Logging.TextBoxWriter = null;
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                _mode = Mode.Manual;

                RefreshOrders();
                CheckState();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:RefreshOrdersButton", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void modifyButton_Click(object sender, EventArgs e)
        {
            try
            {
                Logging.Log("OmniEveUI:ModifyOrdersButton", "Modifying sell and buy orders", Logging.Debug);

                _mode = Mode.Manual;

                ModifyOrders();
                CheckState();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:ModifyOrdersButton", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void autoStopButton_Click(object sender, EventArgs e)
        {
            try 
            {
                _timer.Stop();
                _timer.Elapsed -= AutoTimerElapsed;

                CheckState();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:AutoStopButton", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void autoStartButton_Click(object sender, EventArgs e)
        {
            try
            {
                _mode = Mode.Automatic;

                _timer.Interval = int.Parse(autoSecondsTextBox.Text) * 1000;
                _timer.Elapsed += AutoTimerElapsed;
                _timer.AutoReset = true;
                _timer.Start();

                RefreshOrders();
                CheckState();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:AutoStartButton", "Exception [" + ex + "]", Logging.Debug);
            }
        }
    }
}
