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

            settingsINITextBox.Text = Properties.Settings.Default.SettingsIniFile;
            logFolderPathTextBox.Text = Properties.Settings.Default.LogFolderPath;
            logFileNameTextBox.Text = Properties.Settings.Default.LogFileName;

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

        private void logFolderPathButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                logFolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
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
                Process[] processes = Process.GetProcesses();
                
                if(processes != null)
                {
                    Process p = processes.FirstOrDefault(x => x.ProcessName == "exefile");

                    if(p != null)
                    { 
                        //System.Threading.Thread.Sleep(10000);

                        string loaderDll = "OmniEveLoader.dll";
                        string settingsINIFile = Properties.Settings.Default.SettingsIniFile;
                        string logFilePath = Properties.Settings.Default.LogFolderPath;
                        string logFileName = Properties.Settings.Default.LogFileName;

                        EasyHook.RemoteHooking.Inject(p.Id, EasyHook.InjectionOptions.DoNotRequireStrongName, loaderDll, loaderDll, settingsINIFile, logFilePath, logFileName);
                    }
                    else
                    {
                        MessageBox.Show("Eve does not appear to be running, please start eve and inject again");
                    }
                }
                else
                {
                    MessageBox.Show("For some reason we couldn't get the process list, something is wrong, please close and reopen OmniEveInjector");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
