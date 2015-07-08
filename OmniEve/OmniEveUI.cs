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
        private OmniEve _omniEve = null;
        
        public OmniEveUI(OmniEve omniEve)
        {
            _omniEve = omniEve;

            InitializeComponent();

            Logging.ControlWriter = new ControlWriter(loggingRichTextBox);
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
                sellingGrid.Rows[index].Cells["Selling_TypeId"].Value = order.TypeId;
                sellingGrid.Rows[index].Cells["Selling_OrderId"].Value = order.OrderId;
                sellingGrid.Rows[index].Cells["Selling_Name"].Value = name;
                sellingGrid.Rows[index].Cells["Selling_Quantity"].Value = quantity;
                sellingGrid.Rows[index].Cells["Selling_OrderPrice"].Value = order.Price.ToString();
                sellingGrid.Rows[index].Cells["Selling_Station"].Value = Cache.Instance.DirectEve.Stations[order.StationId].Name;
                sellingGrid.Rows[index].Cells["Selling_Region"].Value = Cache.Instance.DirectEve.Regions[order.RegionId].Name;
                sellingGrid.AllowUserToAddRows = false;

                // Add an action for each sell order and get the updated market values
                if (_omniEve != null)
                {
                    MarketInfo marketInfo = new MarketInfo();
                    marketInfo.OnMarketInfoActionFinished += MarketInfoFinished;
                    marketInfo.TypeId = order.TypeId;
                    _omniEve.AddAction(marketInfo);
                }
            }
        }

        private void MarketInfoFinished(MarketItemInfo marketInfo)
        {
            List<DirectOrder> orders = marketInfo.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderByDescending(o => o.Price).ToList();
            DirectOrder order = orders.LastOrDefault();

            if(order != null)
            {
                foreach(DataGridViewRow row in sellingGrid.Rows)
                {
                    Logging.Log("OmniEveUI:MarketInfoFinished", "Searching for row with Type Id - " + marketInfo.TypeId, Logging.White);

                    if (int.Parse(row.Cells["Selling_TypeId"].Value.ToString()) == marketInfo.TypeId)
                    {
                        Logging.Log("OmniEveUI:MarketInfoFinished", "Row has been found, adding markinfo to row", Logging.White);

                        row.Cells["Selling_MarketPrice"].Value = order.Price.ToString();
                        if (double.Parse(row.Cells["Selling_OrderPrice"].Value.ToString()) > order.Price)
                        {
                            row.DefaultCellStyle.BackColor = Color.Red;
                            row.DefaultCellStyle.ForeColor = Color.Black;
                        }
                    }
                }
            }
        }

        /*private void MyBuyOrdersUpdated(List<DirectOrder> myBuyOrders)
        {
            Cache.Instance.OnMyBuyOrdersUpdated -= MyBuyOrdersUpdated;

            Logging.Log("OmniEveUI:MyBuyOrdersUpdated", "Clearing existing grid of orders", Logging.White);
            buyingGrid.Rows.Clear();

            Logging.Log("OmniEveUI:MyBuyOrdersUpdated", "Filling grid of updated orders", Logging.White);

            foreach (DirectOrder order in myBuyOrders)
            {
                DirectInvType deType;
                Cache.Instance.DirectEve.InvTypes.TryGetValue(order.TypeId, out deType);
                string quantity = order.VolumeRemaining.ToString() + "/" + order.VolumeEntered.ToString();
                
                int duration = order.Duration;
                DateTime issuedOn = order.IssuedOn;
                TimeSpan timeLeft = issuedOn.AddDays(duration).Subtract(DateTime.UtcNow);

                Logging.Log("OmniEveUI", "Order Type - " + deType.TypeName
                          + " Quantity - " + quantity
                          + " Price - " + order.Price
                          + " Station - " + order.StationId
                          + " Region - " + order.RegionId
                          + " Range - " + order.Range
                          + " Min Volume - " + order.MinimumVolume 
                          + " Duration - " + duration, Logging.White);

                buyingGrid.AllowUserToAddRows = true;
                int index = buyingGrid.Rows.Add();
                buyingGrid.Rows[index].Cells["Buying_TypeId"].Value = order.TypeId;
                buyingGrid.Rows[index].Cells["Buying_OrderId"].Value = order.OrderId;
                buyingGrid.Rows[index].Cells["Buying_Name"].Value = deType.TypeName;
                buyingGrid.Rows[index].Cells["Buying_Quantity"].Value = quantity;
                buyingGrid.Rows[index].Cells["Buying_OrderPrice"].Value = order.Price;
                buyingGrid.Rows[index].Cells["Buying_Station"].Value = order.StationId.ToString();
                buyingGrid.Rows[index].Cells["Buying_Region"].Value = order.RegionId.ToString();
                buyingGrid.Rows[index].Cells["Buying_Range"].Value = order.Range.ToString();
                buyingGrid.Rows[index].Cells["Buying_MinVolume"].Value = order.MinimumVolume.ToString();
                buyingGrid.Rows[index].Cells["Buying_ExpiresIn"].Value = timeLeft.ToString();
                buyingGrid.AllowUserToAddRows = false;

                // Add an action for each sell order and get the updated market values
                if (_omniEve != null)
                {
                    Cache.Instance.OnBuyOrdersUpdated += BuyOrdersUpdated;
                    BuyOrders buyOrders = new BuyOrders();
                    buyOrders.Item = order.TypeId;
                    _omniEve.AddAction(buyOrders);
                }
            }
        }*/

        /*private void BuyOrdersUpdated(int item, List<DirectOrder> buyOrders)
        {
            Cache.Instance.OnBuyOrdersUpdated -= BuyOrdersUpdated;

            List<DirectOrder> orders = buyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderByDescending(o => o.Price).ToList();
            DirectOrder order = orders.FirstOrDefault();

            if (order != null)
            {
                foreach (DataGridViewRow row in buyingGrid.Rows)
                {
                    if ((int)row.Cells["Buying_TypeId"].Value == item)
                    {
                        row.Cells["Buying_MarketPrice"].Value = order.Price;
                    }
                }
            }
        }*/

        private void sellingRefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_omniEve != null)
                {
                    Cache.Instance.OnMySellOrdersUpdated += MySellOrdersUpdated;
                    MyOrders myOrders = new MyOrders();
                    myOrders.IsBid = false;
                    _omniEve.AddAction(myOrders);
                }
            }
            catch (Exception ex)
            {
                Logging.Log("sellingRefresh", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void buyingRefreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_omniEve != null)
                {
                    /*Cache.Instance.OnMyBuyOrdersUpdated += MyBuyOrdersUpdated;
                    MyOrders myOrders = new MyOrders();
                    myOrders.IsBid = true;
                    _omniEve.AddAction(myOrders);*/
                }
            }
            catch (Exception ex)
            {
                Logging.Log("BuyingRefresh", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void sellingModifyButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_omniEve != null)
                {
                    List<DirectOrder> orders = Cache.Instance.MySellOrders;

                    Logging.Log("SellingModify", "0", Logging.Debug);

                    foreach (DataGridViewRow row in sellingGrid.Rows)
                    {
                        Logging.Log("SellingModify", "Marker 1", Logging.Debug);

                        long orderId = (long)row.Cells["Selling_OrderId"].Value;

                        Logging.Log("SellingModify", "Marker 2", Logging.Debug);
                        string orderPriceStr = (string)row.Cells["Selling_OrderPrice"].Value;

                        Logging.Log("SellingModify", "3", Logging.Debug);
                        string marketPriceStr = (string)row.Cells["Selling_MarketPrice"].Value;

                        Logging.Log("SellingModify", "4", Logging.Debug);

                        // To get around the double percision problem we first convert the strings to decimals, do the modification, then convert to double
                        decimal orderPrice = decimal.Parse(orderPriceStr);
                        decimal marketPrice = decimal.Parse(marketPriceStr);
                        decimal newPrice = marketPrice - 0.01m;

                        Logging.Log("SellingModify", "Getting cached sell orders", Logging.Debug);

                        if (orders != null)
                        {
                            Logging.Log("SellingModify", "Getting order with OrderId - " + orderId, Logging.Debug);

                            DirectOrder myOrder = orders.First(o => o.OrderId == orderId);

                            if (myOrder != null && marketPrice < orderPrice)
                            {
                                string newPriceStr = newPrice.ToString();
                                double newPriceDbl = double.Parse(newPriceStr);

                                Logging.Log("SellingModify", "Modifying order with OrderId - " + myOrder.OrderId + " OldPrice - " + myOrder.Price + " MarketPrice - " + marketPrice + " NewPrice - " + newPriceDbl, Logging.Debug);

                                ModifyOrder modifyOrder = new ModifyOrder();
                                modifyOrder.OrderId = myOrder.OrderId;
                                modifyOrder.IsBid = myOrder.IsBid;
                                modifyOrder.Price = newPriceDbl;
                                _omniEve.AddAction(modifyOrder);
                            }
                        }
                        else
                        {
                            Logging.Log("SellingModify", "No orders matching OrderId - " + orderId, Logging.Debug);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("SellingModify", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void buyingModifyButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_omniEve != null)
                {
                    foreach (DataGridViewRow row in buyingGrid.Rows)
                    {
                        long orderId = (long)row.Cells["Buying_OrderId"].Value;
                        double orderPrice = (double)row.Cells["Buying_OrderPrice"].Value;
                        double marketPrice = (double)row.Cells["Buying_MarketPrice"].Value;

                        DirectOrder order = Cache.Instance.MyBuyOrders.Where(o => o.OrderId == orderId).FirstOrDefault();

                        if (order != null && marketPrice < orderPrice)
                        {
                            ModifyOrder modifyOrder = new ModifyOrder();
                            modifyOrder.OrderId = order.OrderId;
                            modifyOrder.IsBid = order.IsBid;
                            modifyOrder.Price = (marketPrice + 0.01);
                            _omniEve.AddAction(modifyOrder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log("BuyingModify", "Exception [" + ex + "]", Logging.Debug);
            }
        }

        private void OmniEveUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _omniEve.Cleanup = true;

            Logging.ControlWriter = null;
        }
    }
}
