namespace OmniEveInjector
{
    partial class InjectorUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InjectorUI));
            this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
            this.eveFilePathBrowseButton = new MetroFramework.Controls.MetroButton();
            this.eveFilePathTextBox = new MetroFramework.Controls.MetroTextBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.startOmniLoader = new MetroFramework.Controls.MetroButton();
            this.logFileNameTextBox = new MetroFramework.Controls.MetroTextBox();
            this.logFileNameLabel = new MetroFramework.Controls.MetroLabel();
            this.logFolderPathButton = new MetroFramework.Controls.MetroButton();
            this.logFolderPathTextBox = new MetroFramework.Controls.MetroTextBox();
            this.logFolderPathLabel = new MetroFramework.Controls.MetroLabel();
            this.settingsINIBrowseButton = new MetroFramework.Controls.MetroButton();
            this.settingsINITextBox = new MetroFramework.Controls.MetroTextBox();
            this.settingsIniFileLabel = new MetroFramework.Controls.MetroLabel();
            this.metroPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // metroPanel1
            // 
            this.metroPanel1.Controls.Add(this.eveFilePathBrowseButton);
            this.metroPanel1.Controls.Add(this.eveFilePathTextBox);
            this.metroPanel1.Controls.Add(this.metroLabel1);
            this.metroPanel1.Controls.Add(this.startOmniLoader);
            this.metroPanel1.Controls.Add(this.logFileNameTextBox);
            this.metroPanel1.Controls.Add(this.logFileNameLabel);
            this.metroPanel1.Controls.Add(this.logFolderPathButton);
            this.metroPanel1.Controls.Add(this.logFolderPathTextBox);
            this.metroPanel1.Controls.Add(this.logFolderPathLabel);
            this.metroPanel1.Controls.Add(this.settingsINIBrowseButton);
            this.metroPanel1.Controls.Add(this.settingsINITextBox);
            this.metroPanel1.Controls.Add(this.settingsIniFileLabel);
            this.metroPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.metroPanel1.HorizontalScrollbarBarColor = true;
            this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
            this.metroPanel1.HorizontalScrollbarSize = 10;
            this.metroPanel1.Location = new System.Drawing.Point(20, 60);
            this.metroPanel1.Name = "metroPanel1";
            this.metroPanel1.Size = new System.Drawing.Size(613, 238);
            this.metroPanel1.TabIndex = 0;
            this.metroPanel1.VerticalScrollbarBarColor = true;
            this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
            this.metroPanel1.VerticalScrollbarSize = 10;
            // 
            // eveFilePathBrowseButton
            // 
            this.eveFilePathBrowseButton.Location = new System.Drawing.Point(538, 23);
            this.eveFilePathBrowseButton.Name = "eveFilePathBrowseButton";
            this.eveFilePathBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.eveFilePathBrowseButton.TabIndex = 16;
            this.eveFilePathBrowseButton.Text = "Browse";
            this.eveFilePathBrowseButton.UseSelectable = true;
            this.eveFilePathBrowseButton.Click += new System.EventHandler(this.eveFilePathBrowseButton_Click);
            // 
            // eveFilePathTextBox
            // 
            this.eveFilePathTextBox.Lines = new string[0];
            this.eveFilePathTextBox.Location = new System.Drawing.Point(0, 23);
            this.eveFilePathTextBox.MaxLength = 32767;
            this.eveFilePathTextBox.Name = "eveFilePathTextBox";
            this.eveFilePathTextBox.PasswordChar = '\0';
            this.eveFilePathTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.eveFilePathTextBox.SelectedText = "";
            this.eveFilePathTextBox.Size = new System.Drawing.Size(532, 23);
            this.eveFilePathTextBox.TabIndex = 15;
            this.eveFilePathTextBox.UseSelectable = true;
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(0, 0);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(83, 19);
            this.metroLabel1.TabIndex = 14;
            this.metroLabel1.Text = "Eve File Path";
            // 
            // startOmniLoader
            // 
            this.startOmniLoader.Location = new System.Drawing.Point(434, 196);
            this.startOmniLoader.Name = "startOmniLoader";
            this.startOmniLoader.Size = new System.Drawing.Size(176, 38);
            this.startOmniLoader.TabIndex = 13;
            this.startOmniLoader.Text = "Start";
            this.startOmniLoader.UseSelectable = true;
            this.startOmniLoader.Click += new System.EventHandler(this.startOmniLoader_Click);
            // 
            // logFileNameTextBox
            // 
            this.logFileNameTextBox.Lines = new string[] {
        "OmniLog.log"};
            this.logFileNameTextBox.Location = new System.Drawing.Point(0, 167);
            this.logFileNameTextBox.MaxLength = 32767;
            this.logFileNameTextBox.Name = "logFileNameTextBox";
            this.logFileNameTextBox.PasswordChar = '\0';
            this.logFileNameTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.logFileNameTextBox.SelectedText = "";
            this.logFileNameTextBox.Size = new System.Drawing.Size(532, 23);
            this.logFileNameTextBox.TabIndex = 12;
            this.logFileNameTextBox.Text = "OmniLog.log";
            this.logFileNameTextBox.UseSelectable = true;
            // 
            // logFileNameLabel
            // 
            this.logFileNameLabel.AutoSize = true;
            this.logFileNameLabel.Location = new System.Drawing.Point(0, 145);
            this.logFileNameLabel.Name = "logFileNameLabel";
            this.logFileNameLabel.Size = new System.Drawing.Size(95, 19);
            this.logFileNameLabel.TabIndex = 11;
            this.logFileNameLabel.Text = "Log File Name";
            // 
            // logFolderPathButton
            // 
            this.logFolderPathButton.Location = new System.Drawing.Point(538, 119);
            this.logFolderPathButton.Name = "logFolderPathButton";
            this.logFolderPathButton.Size = new System.Drawing.Size(75, 23);
            this.logFolderPathButton.TabIndex = 10;
            this.logFolderPathButton.Text = "Browse";
            this.logFolderPathButton.UseSelectable = true;
            this.logFolderPathButton.Click += new System.EventHandler(this.logFolderPathButton_Click);
            // 
            // logFolderPathTextBox
            // 
            this.logFolderPathTextBox.Lines = new string[0];
            this.logFolderPathTextBox.Location = new System.Drawing.Point(0, 119);
            this.logFolderPathTextBox.MaxLength = 32767;
            this.logFolderPathTextBox.Name = "logFolderPathTextBox";
            this.logFolderPathTextBox.PasswordChar = '\0';
            this.logFolderPathTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.logFolderPathTextBox.SelectedText = "";
            this.logFolderPathTextBox.Size = new System.Drawing.Size(532, 23);
            this.logFolderPathTextBox.TabIndex = 9;
            this.logFolderPathTextBox.UseSelectable = true;
            // 
            // logFolderPathLabel
            // 
            this.logFolderPathLabel.AutoSize = true;
            this.logFolderPathLabel.Location = new System.Drawing.Point(0, 97);
            this.logFolderPathLabel.Name = "logFolderPathLabel";
            this.logFolderPathLabel.Size = new System.Drawing.Size(103, 19);
            this.logFolderPathLabel.TabIndex = 8;
            this.logFolderPathLabel.Text = "Log Folder Path";
            // 
            // settingsINIBrowseButton
            // 
            this.settingsINIBrowseButton.Location = new System.Drawing.Point(538, 71);
            this.settingsINIBrowseButton.Name = "settingsINIBrowseButton";
            this.settingsINIBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.settingsINIBrowseButton.TabIndex = 7;
            this.settingsINIBrowseButton.Text = "Browse";
            this.settingsINIBrowseButton.UseSelectable = true;
            this.settingsINIBrowseButton.Click += new System.EventHandler(this.settingsINIBrowseButton_Click);
            // 
            // settingsINITextBox
            // 
            this.settingsINITextBox.Lines = new string[0];
            this.settingsINITextBox.Location = new System.Drawing.Point(0, 71);
            this.settingsINITextBox.MaxLength = 32767;
            this.settingsINITextBox.Name = "settingsINITextBox";
            this.settingsINITextBox.PasswordChar = '\0';
            this.settingsINITextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.settingsINITextBox.SelectedText = "";
            this.settingsINITextBox.Size = new System.Drawing.Size(532, 23);
            this.settingsINITextBox.TabIndex = 6;
            this.settingsINITextBox.UseSelectable = true;
            // 
            // settingsIniFileLabel
            // 
            this.settingsIniFileLabel.AutoSize = true;
            this.settingsIniFileLabel.Location = new System.Drawing.Point(0, 49);
            this.settingsIniFileLabel.Name = "settingsIniFileLabel";
            this.settingsIniFileLabel.Size = new System.Drawing.Size(98, 19);
            this.settingsIniFileLabel.TabIndex = 5;
            this.settingsIniFileLabel.Text = "Settings INI File";
            // 
            // InjectorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 318);
            this.Controls.Add(this.metroPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InjectorUI";
            this.Text = "Omni Eve Injector";
            this.metroPanel1.ResumeLayout(false);
            this.metroPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroTextBox settingsINITextBox;
        private MetroFramework.Controls.MetroLabel settingsIniFileLabel;
        private MetroFramework.Controls.MetroButton settingsINIBrowseButton;
        private MetroFramework.Controls.MetroLabel logFolderPathLabel;
        private MetroFramework.Controls.MetroButton logFolderPathButton;
        private MetroFramework.Controls.MetroTextBox logFolderPathTextBox;
        private MetroFramework.Controls.MetroTextBox logFileNameTextBox;
        private MetroFramework.Controls.MetroLabel logFileNameLabel;
        private MetroFramework.Controls.MetroButton startOmniLoader;
        private MetroFramework.Controls.MetroButton eveFilePathBrowseButton;
        private MetroFramework.Controls.MetroTextBox eveFilePathTextBox;
        private MetroFramework.Controls.MetroLabel metroLabel1;
    }
}

