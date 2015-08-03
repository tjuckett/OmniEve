using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework.Controls;

namespace OmniEveInjector
{
    public partial class InjectorUI : MetroForm
    {
        private FolderBrowserDialog folderBrowserDialog;
        private OpenFileDialog openfileDialog;

        public InjectorUI()
        {
            InitializeComponent();

            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the directory for the Eve Online exe file.";
            folderBrowserDialog.ShowNewFolderButton = false;

            openfileDialog = new OpenFileDialog();

            eveFilePathTextBox.Text = Properties.Settings.Default.EveExeFolderPath;
            settingsINITextBox.Text = Properties.Settings.Default.SettingsIniFile;
            logFolderPathTextBox.Text = Properties.Settings.Default.LogFolderPath;
            logFileNameTextBox.Text = Properties.Settings.Default.LogFileName;

            eveFilePathTextBox.TextChanged += eveFilePathTextChanged;
            settingsINITextBox.TextChanged += settingsINITextChanged;
            logFolderPathTextBox.TextChanged += logFolderPathChanged;
            logFileNameTextBox.TextChanged += logFileNameTextChanged;
        }

        private void settingsINIBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openfileDialog.ShowDialog();

            if( result == DialogResult.OK )
            {
                settingsINITextBox.Text = openfileDialog.FileName;
            }
        }

        private void eveFilePathBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                eveFilePathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void logFolderPathButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                logFolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        public void eveFilePathTextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EveExeFolderPath = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        public void settingsINITextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SettingsIniFile = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        public void logFolderPathChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LogFolderPath = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        public void logFileNameTextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LogFileName = ((MetroTextBox)sender).Text;
            Properties.Settings.Default.Save();
        }

        private void startOmniLoader_Click(object sender, EventArgs e)
        {
            try
            {
                // the target process - I'm using a dummy process for this
                // if you don't have one, open Task Manager and choose wisely

                string exefilePath = Properties.Settings.Default.EveExeFolderPath;
                string settingsINIFile = Properties.Settings.Default.SettingsIniFile;
                string loaderDll = "OmniEveLoader.dll";
                string logFilePath = Properties.Settings.Default.LogFolderPath;
                string logFileName = Properties.Settings.Default.LogFileName;

                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = "exefile.exe",
                        WorkingDirectory = exefilePath,
                        Arguments = "/triPlatform=dx9 /noconsole"
                    }
                };

                if (p != null)
                {

                    p.Start();
                    p.WaitForInputIdle();

                    System.Threading.Thread.Sleep(500);

                    EasyHook.RemoteHooking.Inject(p.Id, EasyHook.InjectionOptions.DoNotRequireStrongName, loaderDll, loaderDll, settingsINIFile, logFilePath, logFileName);
                }
                else
                {
                    MessageBox.Show("Eve does not appear to be running, please start eve and inject again");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
