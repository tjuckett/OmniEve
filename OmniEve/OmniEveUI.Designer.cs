namespace OmniEve
{
    partial class OmniEveUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OmniEveUI));
            this.metroTabControl1 = new MetroFramework.Controls.MetroTabControl();
            this.ordersPage = new MetroFramework.Controls.MetroTabPage();
            this.refreshOrdersButton = new MetroFramework.Controls.MetroButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.metroPanel2 = new MetroFramework.Controls.MetroPanel();
            this.buyingGrid = new MetroFramework.Controls.MetroGrid();
            this.metroPanel3 = new MetroFramework.Controls.MetroPanel();
            this.sellingGrid = new MetroFramework.Controls.MetroGrid();
            this.metroPanel4 = new MetroFramework.Controls.MetroPanel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.metroPanel5 = new MetroFramework.Controls.MetroPanel();
            this.modifyOrdersButton = new MetroFramework.Controls.MetroButton();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroPanel6 = new MetroFramework.Controls.MetroPanel();
            this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
            this.marketTabPage = new MetroFramework.Controls.MetroTabPage();
            this.logTextBox = new MetroFramework.Controls.MetroTextBox();
            this.Selling_Select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Selling_TypeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_OrderId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_Quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_OrderPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_MarketPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_Station = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Selling_Region = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Select = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Buying_TypeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_OrderId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_OrderPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_MarketPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Station = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Region = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_Range = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Buying_MinVolume = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metroTabControl1.SuspendLayout();
            this.ordersPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.metroPanel1.SuspendLayout();
            this.metroPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.buyingGrid)).BeginInit();
            this.metroPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sellingGrid)).BeginInit();
            this.metroPanel4.SuspendLayout();
            this.metroPanel5.SuspendLayout();
            this.metroPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroTabControl1
            // 
            this.metroTabControl1.Controls.Add(this.ordersPage);
            this.metroTabControl1.Controls.Add(this.marketTabPage);
            this.metroTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroTabControl1.Location = new System.Drawing.Point(20, 60);
            this.metroTabControl1.Name = "metroTabControl1";
            this.metroTabControl1.SelectedIndex = 0;
            this.metroTabControl1.Size = new System.Drawing.Size(1159, 578);
            this.metroTabControl1.TabIndex = 0;
            this.metroTabControl1.UseSelectable = true;
            // 
            // ordersPage
            // 
            this.ordersPage.Controls.Add(this.modifyOrdersButton);
            this.ordersPage.Controls.Add(this.refreshOrdersButton);
            this.ordersPage.Controls.Add(this.tableLayoutPanel1);
            this.ordersPage.HorizontalScrollbarBarColor = true;
            this.ordersPage.HorizontalScrollbarHighlightOnWheel = false;
            this.ordersPage.HorizontalScrollbarSize = 0;
            this.ordersPage.Location = new System.Drawing.Point(4, 38);
            this.ordersPage.Name = "ordersPage";
            this.ordersPage.Size = new System.Drawing.Size(1151, 536);
            this.ordersPage.TabIndex = 0;
            this.ordersPage.Text = "Orders";
            this.ordersPage.VerticalScrollbarBarColor = true;
            this.ordersPage.VerticalScrollbarHighlightOnWheel = false;
            this.ordersPage.VerticalScrollbarSize = 0;
            // 
            // refreshOrdersButton
            // 
            this.refreshOrdersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshOrdersButton.Location = new System.Drawing.Point(1042, 513);
            this.refreshOrdersButton.Name = "refreshOrdersButton";
            this.refreshOrdersButton.Size = new System.Drawing.Size(109, 23);
            this.refreshOrdersButton.TabIndex = 4;
            this.refreshOrdersButton.Text = "Refresh Orders";
            this.refreshOrdersButton.UseSelectable = true;
            this.refreshOrdersButton.Click += new System.EventHandler(this.refreshOrdersButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.metroPanel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.metroPanel2, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroPanel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroPanel4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.metroPanel5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.metroPanel6, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1151, 508);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.logTextBox);
            this.metroPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(100, 308);
            this.metroPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(1051, 200);
            this.metroPanel1.TabIndex = 0;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // metroPanel2
            // 
            this.metroPanel2.Controls.Add(this.buyingGrid);
            this.metroPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel2.HorizontalScrollbarBarColor = true;
            this.metroPanel2.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel2.HorizontalScrollbarSize = 10;
            this.metroPanel2.Location = new System.Drawing.Point(100, 154);
            this.metroPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel2.Name = "metroPanel2";
            this.metroPanel2.Size = new System.Drawing.Size(1051, 154);
            this.metroPanel2.TabIndex = 1;
            this.metroPanel2.VerticalScrollbarBarColor = true;
            this.metroPanel2.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel2.VerticalScrollbarSize = 10;
            // 
            // buyingGrid
            // 
            this.buyingGrid.AllowUserToAddRows = false;
            this.buyingGrid.AllowUserToResizeRows = false;
            this.buyingGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.buyingGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.buyingGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.buyingGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.buyingGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.buyingGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.buyingGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Buying_Select,
            this.Buying_TypeId,
            this.Buying_OrderId,
            this.Buying_Name,
            this.Buying_Quantity,
            this.Buying_OrderPrice,
            this.Buying_MarketPrice,
            this.Buying_Station,
            this.Buying_Region,
            this.Buying_Range,
            this.Buying_MinVolume});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.buyingGrid.DefaultCellStyle = dataGridViewCellStyle4;
            this.buyingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buyingGrid.EnableHeadersVisualStyles = false;
            this.buyingGrid.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.buyingGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.buyingGrid.Location = new System.Drawing.Point(0, 0);
            this.buyingGrid.Name = "buyingGrid";
            this.buyingGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.buyingGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.buyingGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.buyingGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.buyingGrid.Size = new System.Drawing.Size(1051, 154);
            this.buyingGrid.TabIndex = 2;
            // 
            // metroPanel3
            // 
            this.metroPanel3.Controls.Add(this.sellingGrid);
            this.metroPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel3.HorizontalScrollbarBarColor = true;
            this.metroPanel3.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel3.HorizontalScrollbarSize = 10;
            this.metroPanel3.Location = new System.Drawing.Point(100, 0);
            this.metroPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel3.Name = "metroPanel3";
            this.metroPanel3.Size = new System.Drawing.Size(1051, 154);
            this.metroPanel3.TabIndex = 2;
            this.metroPanel3.VerticalScrollbarBarColor = true;
            this.metroPanel3.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel3.VerticalScrollbarSize = 10;
            // 
            // sellingGrid
            // 
            this.sellingGrid.AllowUserToAddRows = false;
            this.sellingGrid.AllowUserToResizeRows = false;
            this.sellingGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sellingGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.sellingGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.sellingGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sellingGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.sellingGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.sellingGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Selling_Select,
            this.Selling_TypeId,
            this.Selling_OrderId,
            this.Selling_Name,
            this.Selling_Quantity,
            this.Selling_OrderPrice,
            this.Selling_MarketPrice,
            this.Selling_Station,
            this.Selling_Region});
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.sellingGrid.DefaultCellStyle = dataGridViewCellStyle12;
            this.sellingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sellingGrid.EnableHeadersVisualStyles = false;
            this.sellingGrid.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.sellingGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.sellingGrid.Location = new System.Drawing.Point(0, 0);
            this.sellingGrid.Name = "sellingGrid";
            this.sellingGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle13.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle13.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle13.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.sellingGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle13;
            this.sellingGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.sellingGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.sellingGrid.Size = new System.Drawing.Size(1051, 154);
            this.sellingGrid.TabIndex = 2;
            // 
            // metroPanel4
            // 
            this.metroPanel4.Controls.Add(this.metroLabel1);
            this.metroPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel4.HorizontalScrollbarBarColor = true;
            this.metroPanel4.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel4.HorizontalScrollbarSize = 10;
            this.metroPanel4.Location = new System.Drawing.Point(0, 0);
            this.metroPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel4.Name = "metroPanel4";
            this.metroPanel4.Size = new System.Drawing.Size(100, 154);
            this.metroPanel4.TabIndex = 3;
            this.metroPanel4.VerticalScrollbarBarColor = true;
            this.metroPanel4.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel4.VerticalScrollbarSize = 10;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(4, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(47, 19);
            this.metroLabel1.TabIndex = 2;
            this.metroLabel1.Text = "Selling";
            // 
            // metroPanel5
            // 
            this.metroPanel5.Controls.Add(this.metroLabel2);
            this.metroPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel5.HorizontalScrollbarBarColor = true;
            this.metroPanel5.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel5.HorizontalScrollbarSize = 10;
            this.metroPanel5.Location = new System.Drawing.Point(0, 154);
            this.metroPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel5.Name = "metroPanel5";
            this.metroPanel5.Size = new System.Drawing.Size(100, 154);
            this.metroPanel5.TabIndex = 4;
            this.metroPanel5.VerticalScrollbarBarColor = true;
            this.metroPanel5.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel5.VerticalScrollbarSize = 10;
            // 
            // modifyOrdersButton
            // 
            this.modifyOrdersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.modifyOrdersButton.Location = new System.Drawing.Point(943, 513);
            this.modifyOrdersButton.Name = "modifyOrdersButton";
            this.modifyOrdersButton.Size = new System.Drawing.Size(93, 23);
            this.modifyOrdersButton.TabIndex = 5;
            this.modifyOrdersButton.Text = " Modify Orders";
            this.modifyOrdersButton.UseSelectable = true;
            this.modifyOrdersButton.Click += new System.EventHandler(this.modifyOrdersButton_Click);
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(4, 0);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(48, 19);
            this.metroLabel2.TabIndex = 3;
            this.metroLabel2.Text = "Buying";
            // 
            // metroPanel6
            // 
            this.metroPanel6.Controls.Add(this.metroLabel3);
            this.metroPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel6.HorizontalScrollbarBarColor = true;
            this.metroPanel6.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel6.HorizontalScrollbarSize = 10;
            this.metroPanel6.Location = new System.Drawing.Point(0, 308);
            this.metroPanel6.Margin = new System.Windows.Forms.Padding(0);
            this.metroPanel6.Name = "metroPanel6";
            this.metroPanel6.Size = new System.Drawing.Size(100, 200);
            this.metroPanel6.TabIndex = 5;
            this.metroPanel6.VerticalScrollbarBarColor = true;
            this.metroPanel6.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel6.VerticalScrollbarSize = 10;
            // 
            // metroLabel3
            // 
            this.metroLabel3.AutoSize = true;
            this.metroLabel3.Location = new System.Drawing.Point(3, 0);
            this.metroLabel3.Name = "metroLabel3";
            this.metroLabel3.Size = new System.Drawing.Size(57, 19);
            this.metroLabel3.TabIndex = 4;
            this.metroLabel3.Text = "Logging";
            // 
            // marketTabPage
            // 
            this.marketTabPage.HorizontalScrollbarBarColor = true;
            this.marketTabPage.HorizontalScrollbarHighlightOnWheel = false;
            this.marketTabPage.HorizontalScrollbarSize = 0;
            this.marketTabPage.Location = new System.Drawing.Point(4, 38);
            this.marketTabPage.Name = "marketTabPage";
            this.marketTabPage.Size = new System.Drawing.Size(1151, 536);
            this.marketTabPage.TabIndex = 1;
            this.marketTabPage.Text = "Market";
            this.marketTabPage.VerticalScrollbarBarColor = true;
            this.marketTabPage.VerticalScrollbarHighlightOnWheel = false;
            this.marketTabPage.VerticalScrollbarSize = 0;
            // 
            // logTextBox
            // 
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Lines = new string[0];
            this.logTextBox.Location = new System.Drawing.Point(0, 0);
            this.logTextBox.MaxLength = 32767;
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.PasswordChar = '\0';
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.SelectedText = "";
            this.logTextBox.Size = new System.Drawing.Size(1051, 200);
            this.logTextBox.TabIndex = 2;
            this.logTextBox.UseSelectable = true;
            // 
            // Selling_Select
            // 
            this.Selling_Select.HeaderText = "Select";
            this.Selling_Select.Name = "Selling_Select";
            // 
            // Selling_TypeId
            // 
            dataGridViewCellStyle7.Format = "N0";
            dataGridViewCellStyle7.NullValue = null;
            this.Selling_TypeId.DefaultCellStyle = dataGridViewCellStyle7;
            this.Selling_TypeId.HeaderText = "TypeId";
            this.Selling_TypeId.Name = "Selling_TypeId";
            this.Selling_TypeId.ReadOnly = true;
            this.Selling_TypeId.Visible = false;
            // 
            // Selling_OrderId
            // 
            dataGridViewCellStyle8.Format = "N0";
            dataGridViewCellStyle8.NullValue = null;
            this.Selling_OrderId.DefaultCellStyle = dataGridViewCellStyle8;
            this.Selling_OrderId.HeaderText = "OrderId";
            this.Selling_OrderId.Name = "Selling_OrderId";
            this.Selling_OrderId.ReadOnly = true;
            this.Selling_OrderId.Visible = false;
            // 
            // Selling_Name
            // 
            this.Selling_Name.HeaderText = "Name";
            this.Selling_Name.Name = "Selling_Name";
            this.Selling_Name.ReadOnly = true;
            // 
            // Selling_Quantity
            // 
            dataGridViewCellStyle9.Format = "N0";
            dataGridViewCellStyle9.NullValue = null;
            this.Selling_Quantity.DefaultCellStyle = dataGridViewCellStyle9;
            this.Selling_Quantity.HeaderText = "Quantity";
            this.Selling_Quantity.Name = "Selling_Quantity";
            this.Selling_Quantity.ReadOnly = true;
            // 
            // Selling_OrderPrice
            // 
            dataGridViewCellStyle10.Format = "C2";
            dataGridViewCellStyle10.NullValue = null;
            this.Selling_OrderPrice.DefaultCellStyle = dataGridViewCellStyle10;
            this.Selling_OrderPrice.HeaderText = "Order Price";
            this.Selling_OrderPrice.Name = "Selling_OrderPrice";
            this.Selling_OrderPrice.ReadOnly = true;
            // 
            // Selling_MarketPrice
            // 
            dataGridViewCellStyle11.Format = "C2";
            dataGridViewCellStyle11.NullValue = null;
            this.Selling_MarketPrice.DefaultCellStyle = dataGridViewCellStyle11;
            this.Selling_MarketPrice.HeaderText = "Market Price";
            this.Selling_MarketPrice.Name = "Selling_MarketPrice";
            this.Selling_MarketPrice.ReadOnly = true;
            // 
            // Selling_Station
            // 
            this.Selling_Station.HeaderText = "Station";
            this.Selling_Station.Name = "Selling_Station";
            this.Selling_Station.ReadOnly = true;
            // 
            // Selling_Region
            // 
            this.Selling_Region.HeaderText = "Region";
            this.Selling_Region.Name = "Selling_Region";
            this.Selling_Region.ReadOnly = true;
            // 
            // Buying_Select
            // 
            this.Buying_Select.HeaderText = "Select";
            this.Buying_Select.Name = "Buying_Select";
            // 
            // Buying_TypeId
            // 
            this.Buying_TypeId.HeaderText = "TypeId";
            this.Buying_TypeId.Name = "Buying_TypeId";
            this.Buying_TypeId.ReadOnly = true;
            this.Buying_TypeId.Visible = false;
            // 
            // Buying_OrderId
            // 
            this.Buying_OrderId.HeaderText = "OrderId";
            this.Buying_OrderId.Name = "Buying_OrderId";
            this.Buying_OrderId.ReadOnly = true;
            this.Buying_OrderId.Visible = false;
            // 
            // Buying_Name
            // 
            this.Buying_Name.HeaderText = "Name";
            this.Buying_Name.Name = "Buying_Name";
            this.Buying_Name.ReadOnly = true;
            // 
            // Buying_Quantity
            // 
            this.Buying_Quantity.HeaderText = "Quantity";
            this.Buying_Quantity.Name = "Buying_Quantity";
            this.Buying_Quantity.ReadOnly = true;
            // 
            // Buying_OrderPrice
            // 
            dataGridViewCellStyle2.Format = "C2";
            dataGridViewCellStyle2.NullValue = null;
            this.Buying_OrderPrice.DefaultCellStyle = dataGridViewCellStyle2;
            this.Buying_OrderPrice.HeaderText = "Order Price";
            this.Buying_OrderPrice.Name = "Buying_OrderPrice";
            this.Buying_OrderPrice.ReadOnly = true;
            // 
            // Buying_MarketPrice
            // 
            dataGridViewCellStyle3.Format = "C2";
            dataGridViewCellStyle3.NullValue = null;
            this.Buying_MarketPrice.DefaultCellStyle = dataGridViewCellStyle3;
            this.Buying_MarketPrice.HeaderText = "Market Price";
            this.Buying_MarketPrice.Name = "Buying_MarketPrice";
            this.Buying_MarketPrice.ReadOnly = true;
            // 
            // Buying_Station
            // 
            this.Buying_Station.HeaderText = "Station";
            this.Buying_Station.Name = "Buying_Station";
            this.Buying_Station.ReadOnly = true;
            // 
            // Buying_Region
            // 
            this.Buying_Region.HeaderText = "Region";
            this.Buying_Region.Name = "Buying_Region";
            this.Buying_Region.ReadOnly = true;
            // 
            // Buying_Range
            // 
            this.Buying_Range.HeaderText = "Range";
            this.Buying_Range.Name = "Buying_Range";
            this.Buying_Range.ReadOnly = true;
            // 
            // Buying_MinVolume
            // 
            this.Buying_MinVolume.HeaderText = "Min Volume";
            this.Buying_MinVolume.Name = "Buying_MinVolume";
            this.Buying_MinVolume.ReadOnly = true;
            // 
            // OmniEveUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1199, 658);
            this.Controls.Add(this.metroTabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OmniEveUI";
            this.Text = "OmniEve";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OmniEveUI_FormClosing);
            this.metroTabControl1.ResumeLayout(false);
            this.ordersPage.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.buyingGrid)).EndInit();
            this.metroPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sellingGrid)).EndInit();
            this.metroPanel4.ResumeLayout(false);
            this.metroPanel4.PerformLayout();
            this.metroPanel5.ResumeLayout(false);
            this.metroPanel5.PerformLayout();
            this.metroPanel6.ResumeLayout(false);
            this.metroPanel6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroTabControl metroTabControl1;
        private MetroFramework.Controls.MetroTabPage ordersPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroPanel metroPanel2;
        private MetroFramework.Controls.MetroGrid buyingGrid;
        private MetroFramework.Controls.MetroPanel metroPanel3;
        private MetroFramework.Controls.MetroGrid sellingGrid;
        private MetroFramework.Controls.MetroPanel metroPanel4;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroPanel metroPanel5;
        private MetroFramework.Controls.MetroButton modifyOrdersButton;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroPanel metroPanel6;
        private MetroFramework.Controls.MetroTabPage marketTabPage;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroButton refreshOrdersButton;
        private MetroFramework.Controls.MetroTextBox logTextBox;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Selling_Select;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_TypeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_OrderId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_Quantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_OrderPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_MarketPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_Station;
        private System.Windows.Forms.DataGridViewTextBoxColumn Selling_Region;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Buying_Select;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_TypeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_OrderId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_Quantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_OrderPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_MarketPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_Station;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_Region;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_Range;
        private System.Windows.Forms.DataGridViewTextBoxColumn Buying_MinVolume;

    }
}

