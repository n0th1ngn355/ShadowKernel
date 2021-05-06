using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ShadowKernel.Classes;
using System.Diagnostics;
namespace ShadowKernel.userControls
{

    public partial class SettingsClient : System.Windows.Controls.UserControl
    {

        public SettingsClient()
        {
            InitializeComponent();
            Init();

        }
        public void Init()
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            SettingsServer stgServer = (SettingsServer)wind.stgServer;
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(stgServer);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            SettingsNet stgNet = (SettingsNet)wind.stgNet;
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(stgNet);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (DNS.Text == null || Port.Text == null || Name.Text == null || Tag.Text == null || UpInt.Text == null)
            //{
            //    MessageBox.Show("Error: One or more text fields is empty.", "Error", MessageBoxButtons.OK,
            //        MessageBoxIcon.Error);
            //    return;
            //}
            Builder ClientBuilder = new Builder();
            try
            {
                Convert.ToInt16(Port.Text);
                Convert.ToInt16(UpInt.Text);
            }
            catch (Exception EX)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + EX.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //string Install = cbEnableInstallation.Checked ? "True" : "False";
            //string Startup = cbEnableStartup.Checked ? "True" : "False";

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = Name.Text; // Default file name
            //dlg.InitialDirectory = Environment.CurrentDirectory + @"\Clients";
            dlg.DefaultExt = ".exe"; // Default file extension
            dlg.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";

            if (dlg.ShowDialog() != DialogResult.Cancel)
            {
                // Save document
                ClientBuilder.BuildClient(Port.Text, DNS.Text, dlg.FileName, Tag.Text, UpInt.Text,
                "False", "False");
                Process.Start("explorer.exe",dlg.FileName.Substring(0,dlg.FileName.LastIndexOf("\\")));

            }
            else { return; }
            
            
        }

        private void Port_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var port = GetPortSafe();
            if (port == 0 || port > 65535)
            {
                Port.Text = "1";
            }
            else { return; }
        }
        private ushort GetPortSafe()
        {
            var portValue = Port.Text.ToString();
            ushort port;
            return (!ushort.TryParse(portValue, out port)) ? (ushort)0 : port;
        }

        private void UpInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            ushort port;
            port = (!ushort.TryParse(UpInt.Text, out port)) ? (ushort)0 : port;
            if (port == 0 || port > 10000)
            {
                UpInt.Text = "1";
            }
            else { return; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostAddresses(hostName)[1].ToString();
            Port.Text = Properties.Settings.Default.Port.ToString();
            DNS.Text = myIP;
        }
    }
}