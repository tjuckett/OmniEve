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
using MetroFramework.Controls;

namespace OmniEve
{
    using DirectEve;
    using OmniEveModules.Caching;
    using OmniEveModules.Scripts;
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

        private void OnMyOrdersFinished(List<DirectOrder> mySellOrders, List<DirectOrder> myBuyOrders)
        {
            sellingGrid.Invoke((MethodInvoker)delegate { UpdateMySellOrdersGrid_Fill(mySellOrders); });
            buyingGrid.Invoke((MethodInvoker)delegate { UpdateMyBuyOrdersGrid_Fill(myBuyOrders); });

            CheckState();
        }

        private void OnMarketInfoFinished(MarketItem marketItem)
        {
            List<DirectOrder> sellOrders = marketItem.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderBy(o => o.Price).ToList();
            List<DirectOrder> buyOrders = marketItem.BuyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderByDescending(o => o.Price).ToList();
            DirectOrder sellOrder = sellOrders.FirstOrDefault();
            DirectOrder buyOrder = buyOrders.FirstOrDefault();

            sellingGrid.Invoke((MethodInvoker)delegate { UpdateMySellOrdersGrid_LowestMarketOrder(marketItem.TypeId, sellOrder); });
            buyingGrid.Invoke((MethodInvoker)delegate { UpdateMyBuyOrdersGrid_HighestMarketOrder(marketItem.TypeId, buyOrder); });

            CheckState();
        }

        private void OnModifySellOrderFinished(long orderId, double price)
        {
            sellingGrid.Invoke((MethodInvoker)delegate { UpdateMySellOrdersGrid_OrderPrice(orderId, price, true); });

            CheckState();
        }

        private void OnModifyBuyOrderFinished(long orderId, double price)
        {
            buyingGrid.Invoke((MethodInvoker)delegate { UpdateMyBuyOrdersGrid_OrderPrice(orderId, price, true); });
            CheckState();
        }

        private void UpdateMySellOrdersGrid_Fill(List<DirectOrder> mySellOrders)
        {
            Logging.Log("OmniEveUI:UpdateMySellOrders", "Clearing existing sell grid of orders", Logging.White);
            sellingGrid.Rows.Clear();

            Logging.Log("OmniEveUI:UpdateMySellOrders", "Filling selling grid of updated orders", Logging.White);
            foreach (DirectOrder order in mySellOrders)
            {
                DirectType directType = Cache.Instance.DirectEve.GetType(order.TypeId);
                string name = directType.Name;
                string quantity = order.VolumeRemaining.ToString() + "/" + order.VolumeEntered.ToString();

                Logging.Log("OmniEveUI:UpdateMySellOrders", "Order Name - " + name 
                          + " Quantity - " + quantity 
                          + " Price - " + order.Price 
                          + " Station - " + order.StationId 
                          + " Region - " + order.RegionId, Logging.White);

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
            }
        }

        private void UpdateMyBuyOrdersGrid_Fill(List<DirectOrder> myBuyOrders)
        {
            Logging.Log("OmniEveUI:UpdateMyBuyOrders", "Clearing existing buy grid of orders", Logging.White);
            buyingGrid.Rows.Clear();

            Logging.Log("OmniEveUI:UpdateMyBuyOrders", "Filling buying grid of updated orders", Logging.White);
            foreach (DirectOrder order in myBuyOrders)
            {
                DirectType directType = Cache.Instance.DirectEve.GetType(order.TypeId);
                string name = directType.Name;
                string quantity = order.VolumeRemaining.ToString() + "/" + order.VolumeEntered.ToString();

                Logging.Log("OmniEveUI:UpdateMyBuyOrders", "Order Type - " + name
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
            }
        }
        private void UpdateMySellOrdersGrid_LowestMarketOrder(int typeId, DirectOrder sellOrder)
        {
            if (sellOrder != null)
            {
                foreach (DataGridViewRow row in sellingGrid.Rows)
                {
                    if (int.Parse(row.Cells["Selling_TypeId"].Value.ToString()) == typeId)
                    {
                        Logging.Log("OmniEveUI:UpdateMarketInfoSellOrder", "Row has been found, adding markinfo to row", Logging.White);

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

        private void UpdateMyBuyOrdersGrid_HighestMarketOrder(int typeId, DirectOrder buyOrder)
        {
            if (buyOrder != null)
            {
                foreach (DataGridViewRow row in buyingGrid.Rows)
                {
                    if (int.Parse(row.Cells["Buying_TypeId"].Value.ToString()) == typeId)
                    {
                        Logging.Log("OmniEveUI:UpdateMarketInfoBuyOrder", "Row has been found, adding market info to row", Logging.White);

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

        private void UpdateMySellOrdersGrid_OrderPrice(long orderId, double price, bool changeMarket)
        {
            foreach (DataGridViewRow row in sellingGrid.Rows)
            {
                if ((long)row.Cells["Selling_OrderId"].Value == orderId)
                {
                    row.Cells["Selling_OrderPrice"].Value = price.ToString();
                    if (changeMarket == true)
                        row.Cells["Selling_MarketPrice"].Value = price.ToString();

                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void UpdateMyBuyOrdersGrid_OrderPrice(long orderId, double price, bool changeMarket)
        {
            foreach (DataGridViewRow row in buyingGrid.Rows)
            {
                if ((long)row.Cells["Buying_OrderId"].Value == orderId)
                {
                    row.Cells["Buying_OrderPrice"].Value = price.ToString();
                    if (changeMarket == true)
                        row.Cells["Buying_MarketPrice"].Value = price.ToString();

                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void UpdateItemHangerGrid_Fill(List<DirectItem> _hangerItems, List<DirectOrder> mySellOrders)
        {
            Logging.Log("OmniEveUI:UpdateItemHanger", "Clearing existing item hanger grid of items", Logging.White);
            itemHangerGrid.Rows.Clear();

            Logging.Log("OmniEveUI:UpdateItemHanger", "Filling item hanger grid of updated items", Logging.White);

            foreach (DirectItem item in _hangerItems)
            {
                Logging.Log("OmniEveUI:UpdateItemHanger", "Item Name - " + item.Name
                          + " Quantity - " + item.Quantity
                          + " Group - " + item.GroupName
                          + " Volume - " + item.Volume, Logging.White);

                int index = itemHangerGrid.Rows.Add();
                itemHangerGrid.AllowUserToAddRows = true;
                itemHangerGrid.Rows[index].Cells["ItemHanger_Select"].Value = false;
                itemHangerGrid.Rows[index].Cells["ItemHanger_Name"].Value = item.Name;
                itemHangerGrid.Rows[index].Cells["ItemHanger_ItemId"].Value = item.ItemId;
                itemHangerGrid.Rows[index].Cells["ItemHanger_Quantity"].Value = item.Quantity;
                itemHangerGrid.Rows[index].Cells["ItemHanger_Group"].Value = item.GroupName;
                itemHangerGrid.Rows[index].Cells["ItemHanger_Volume"].Value = item.Volume;

                DirectOrder order = mySellOrders.FirstOrDefault(o => o.TypeId == item.TypeId);

                if (order == null)
                {
                    itemHangerGrid.Rows[index].Cells["ItemHanger_Select"].Value = true;
                    itemHangerGrid.Rows[index].DefaultCellStyle.BackColor = Color.Red;
                    itemHangerGrid.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
                }

                itemHangerGrid.AllowUserToAddRows = false;
            }
        }

        private List<DirectOrder> CreateModifySellOrdersList()
        {
            List<DirectOrder> sellOrders = new List<DirectOrder>();

            sellingGrid.Invoke((MethodInvoker)delegate 
            { 
                foreach (DataGridViewRow row in sellingGrid.Rows)
                {
                    try
                    {
                        if (Convert.ToBoolean(row.Cells["Selling_Select"].Value) == true)
                        {
                            long orderId = (long)row.Cells["Selling_OrderId"].Value;
                            DirectOrder order = Cache.Instance.MySellOrders.FirstOrDefault(o => o.OrderId == orderId);
                            if (order != null)
                            {
                                Logging.Log("OmniEveUI:CreateModifySellOrdersList", "Adding sell order to list of orders that need to be modified OrderId - " + orderId, Logging.Debug);
                                sellOrders.Add(order);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logging.Log("OmniEveUI:CreateModifySellOrdersList", "Exception [" + ex + "]", Logging.Debug);
                    }
                }
            });

            return sellOrders;
        }

        private List<DirectOrder> CreateModifyBuyOrdersList()
        {
            List<DirectOrder> buyOrders = new List<DirectOrder>();

            buyingGrid.Invoke((MethodInvoker)delegate 
            {
                foreach (DataGridViewRow row in buyingGrid.Rows)
                {
                    try
                    {
                        if (Convert.ToBoolean(row.Cells["Buying_Select"].Value) == true)
                        {
                            long orderId = (long)row.Cells["Buying_OrderId"].Value;
                            DirectOrder order = Cache.Instance.MyBuyOrders.FirstOrDefault(o => o.OrderId == orderId);
                            if (order != null)
                            {
                                buyOrders.Add(order);
                                Logging.Log("OmniEveUI:CreateModifyBuyOrdersList", "Adding buy order to list of orders that need to be modified OrderId - " + orderId, Logging.Debug);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logging.Log("OmniEveUI:ModifyBuyOrders", "Exception [" + ex + "]", Logging.Debug);
                    }
                }
            });

            return buyOrders;
        }

        private void OnItemHangerFinished(List<DirectItem> hangerItems, List<DirectOrder> sellOrders)
        {
            itemHangerGrid.Invoke((MethodInvoker)delegate { UpdateItemHangerGrid_Fill(hangerItems, sellOrders); });

            CheckState();
        }

        private void OnSellItemFinished(DirectItem itemSold)
        {
            itemHangerGrid.Invoke((MethodInvoker)delegate
            {
                List<DataGridViewRow> rowsToRemove = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in itemHangerGrid.Rows)
                {
                    try
                    {
                        if (itemSold.ItemId == (long)row.Cells["ItemHanger_ItemId"].Value)
                        {
                            rowsToRemove.Add(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("OmniEveUI:OnSellItemFinished", "Exception [" + ex + "]", Logging.Debug);
                    }
                }

                // Remove all the rows that had sell orders created
                foreach (DataGridViewRow row in rowsToRemove)
                    itemHangerGrid.Rows.Remove(row);
            });

            CheckState();
        }

        private void OnSellItemsFinished(List<DirectItem> itemsSold)
        {
            itemHangerGrid.Invoke((MethodInvoker)delegate 
            {
                List<DataGridViewRow> rowsToRemove = new List<DataGridViewRow>();

                foreach (DataGridViewRow row in itemHangerGrid.Rows)
                {
                    try
                    {
                        DirectItem item = itemsSold.FirstOrDefault(i => i.ItemId == (long)row.Cells["ItemHanger_ItemId"].Value);

                        if(item != null)
                        {
                            rowsToRemove.Add(row);
                        }
                        else if(row.DefaultCellStyle.BackColor == Color.Red)
                        {
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                            row.DefaultCellStyle.ForeColor = Color.Black;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("OmniEveUI:OnSellItemsFinished", "Exception [" + ex + "]", Logging.Debug);
                    }
                }

                // Remove all the rows that had sell orders created
                foreach (DataGridViewRow row in rowsToRemove)
                    itemHangerGrid.Rows.Remove(row);
            });

            CheckState();
        }

        private void EnableControls()
        {
            myOrdersButton.Enabled = true;
            modifyButton.Enabled = true;
            marketInfoMyOrdersButton.Enabled = true;
            autoStartButton.Enabled = true;
            autoStopButton.Enabled = true;
            autoSecondsTextBox.Enabled = true;
            refreshItemHangerButton.Enabled = true;
            sellItemsButton.Enabled = true;
        }

        private void DisableControls()
        {
            myOrdersButton.Enabled = false;
            modifyButton.Enabled = false;
            marketInfoMyOrdersButton.Enabled = false;
            refreshItemHangerButton.Enabled = false;
            sellItemsButton.Enabled = false;
            autoStartButton.Enabled = false;
            autoSecondsTextBox.Enabled = false;
            
            if (_mode == Mode.Manual)
                autoStopButton.Enabled = false;
        }

        private void AutoTimerElapsed(Object source, ElapsedEventArgs e)
        {
            // Only refresh orders when the action queue is empty, otherwise wait till the next time or lengthen the time between events
            if(_omniEve != null && _omniEve.IsActionQueueEmpty() == true)
                UpdateAllOrders();
        }

        private void UpdateAllOrders()
        {
            if(_omniEve != null)
            {
                UpdateAllOrders updateAllOrders = new UpdateAllOrders();
                updateAllOrders.OnMyOrdersFinished += OnMyOrdersFinished;
                updateAllOrders.OnMarketInfoFinished += OnMarketInfoFinished;
                updateAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
                updateAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
                _omniEve.AddScript(updateAllOrders);
            }
        }

        private void RefreshOrders()
        {
            if (_omniEve != null)
            {
                MyOrders myOrders = new MyOrders();
                myOrders.OnMyOrdersFinished += OnMyOrdersFinished;
                _omniEve.AddScript(myOrders);
            }
        }

        private void ClearGrid(MetroGrid grid)
        {
            grid.DataSource = null;
            grid.Rows.Clear();
            grid.Refresh();
        }

        public void ModifyOrders()
        {
            List<DirectOrder> modifySellOrderList = CreateModifySellOrdersList();
            List<DirectOrder> modifyBuyOrderList = CreateModifyBuyOrdersList();

            ModifyAllOrders modifyAllOrders = new ModifyAllOrders(modifySellOrderList, modifyBuyOrderList);
            modifyAllOrders.OnModifySellOrderFinished += OnModifySellOrderFinished;
            modifyAllOrders.OnModifyBuyOrderFinished += OnModifyBuyOrderFinished;
            _omniEve.AddScript(modifyAllOrders);
        }

        public void RefreshItemHanger()
        {
            ItemHanger itemHanger = new ItemHanger();
            itemHanger.OnItemHangerFinished += OnItemHangerFinished;
            _omniEve.AddScript(itemHanger);
        }

        public void SellItemsInHanger()
        {
            List<DirectItem> sellItemList = new List<DirectItem>();

            itemHangerGrid.Invoke((MethodInvoker)delegate
            {
                foreach (DataGridViewRow row in itemHangerGrid.Rows)
                {
                    try
                    {
                        if (Convert.ToBoolean(row.Cells["ItemHanger_Select"].Value) == true)
                        {
                            long itemId = (long)row.Cells["ItemHanger_ItemId"].Value;
                            DirectItem item = Cache.Instance.ItemHanger.Items.FirstOrDefault(i => i.ItemId == itemId);
                            if (item != null)
                            {
                                Logging.Log("OmniEveUI:SellItemsInHanger", "Adding item to list of items to be sold ItemId - " + itemId, Logging.Debug);
                                sellItemList.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("OmniEveUI:SellItemsInHanger", "Exception [" + ex + "]", Logging.Debug);
                    }
                }
            });

            SellItems sellItems = new SellItems(sellItemList);
            sellItems.OnSellItemFinished += OnSellItemFinished;
            sellItems.OnSellItemsFinished += OnSellItemsFinished;

            _omniEve.AddScript(sellItems);
        }

        private void OmniEveUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _omniEve.Cleanup = true;

            Logging.TextBoxWriter = null;
        }

        private void myOrdersButton_Click(object sender, EventArgs e)
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

        private void marketInfoButton_Click(object sender, EventArgs e)
        {
            try
            {
                _mode = Mode.Manual;

                // Create a list of market info type ids, this will be a combination of the buy and sell orders, we don't want to get
                // an item twice if we have a buy and sell order, just include it once.
                List<int> marketInfoTypeIds = new List<int>();

                foreach (DirectOrder order in Cache.Instance.MySellOrders)
                    marketInfoTypeIds.Add(order.TypeId);

                foreach (DirectOrder order in Cache.Instance.MyBuyOrders)
                {
                    // If the type id isn't already in the list of ids to get market info for then add it
                    if (marketInfoTypeIds.FirstOrDefault(o => o == order.TypeId) == 0)
                        marketInfoTypeIds.Add(order.TypeId);
                }

                // Add an action for each sell order and get the updated market values
                if (_omniEve != null)
                {
                    MarketInfoForList marketInfoForList = new MarketInfoForList(marketInfoTypeIds);
                    marketInfoForList.OnMarketInfoFinished += OnMarketInfoFinished;
                    _omniEve.AddScript(marketInfoForList);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:MarketInfoMyOrders", "Exception [" + ex + "]", Logging.Debug);
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


                UpdateAllOrders();
                CheckState();
            }
            catch (Exception ex)
            {
                Logging.Log("OmniEveUI:AutoStartButton", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void sellingGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_mode == Mode.Idle)
            {
                _mode = Mode.Manual;

                MetroGrid grid = (MetroGrid)sender;
                MarketInfo marketInfo = new MarketInfo(int.Parse(grid.CurrentRow.Cells["Selling_TypeId"].Value.ToString()));
                marketInfo.OnMarketInfoFinished += OnMarketInfoFinished;
                _omniEve.AddScript(marketInfo);

                CheckState();
            }
        }

        private void buyingGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_mode == Mode.Idle)
            {
                _mode = Mode.Manual;

                MetroGrid grid = (MetroGrid)sender;
                MarketInfo marketInfo = new MarketInfo(int.Parse(grid.CurrentRow.Cells["Buying_TypeId"].Value.ToString()));
                marketInfo.OnMarketInfoFinished += OnMarketInfoFinished;
                _omniEve.AddScript(marketInfo);

                CheckState();
            }
        }

        private void refreshItemHangerButton_Click(object sender, EventArgs e)
        {
            if (_mode == Mode.Idle)
            {
                _mode = Mode.Manual;

                Logging.Log("OmniEveUI:RefreshItemHangerButton", "Adding item hanger action", Logging.Debug);

                RefreshItemHanger();

                CheckState();
            }
        }

        private void sellItemsButton_Click(object sender, EventArgs e)
        {
            if (_mode == Mode.Idle)
            {
                _mode = Mode.Manual;

                Logging.Log("OmniEveUI:SellItemsButton", "Adding sell items action", Logging.Debug);

                SellItemsInHanger();

                CheckState();
            }
        }
    }
}
