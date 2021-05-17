using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using ShadowKernel.NetClasses;

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для Net.xaml
    /// </summary>
    public partial class Net : System.Windows.Controls.UserControl
    {

        List<Task> tasks;
        Dictionary<string, string> ips;


        public Net()
        {
            InitializeComponent();
            
            //string myHost = System.Net.Dns.GetHostName();

            //System.Net.IPHostEntry myIPs = System.Net.Dns.GetHostEntry(myHost);

            //// Loop through all IP addresses and display each 

            //foreach (System.Net.IPAddress myIP in myIPs.AddressList)
            //{
            //	nbours.Items.Add(myIP.ToString());

            //}

        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }




        public async void f1(string p)
        {
            await Task.Run(() =>
            {
                try
                {

                    IPHostEntry iPHost = Dns.GetHostEntry(p);
                    ips.Add(p , iPHost.HostName);
                    this.Dispatcher.Invoke(() =>
                    {
                        TreeViewItem lb = new TreeViewItem();
                        lb.Header = p;
                        lb.Tag = "1";
                        var d = new TreeViewItem();
                        d.Header = iPHost.HostName;
                        d.Tag = "0";
                        lb.Items.Add(d);
                        nbours.Items.Add(lb);
                        nbours.Items.Refresh();
                    });
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        TreeViewItem lb = new TreeViewItem();
                        lb.Header = p;
                        lb.Tag = "1";
                        nbours.Items.Add(lb);
                        nbours.Items.Refresh();
                    });
                }
            });

        }


        public async void f()
		{
			await Task.WhenAll(tasks).ContinueWith(t =>
			{
                tglBtn = !tglBtn;
			}); 
		}
		private async Task PingAndUpdateAsync(System.Net.NetworkInformation.Ping ping, string ip)
		{
			var reply = await ping.SendPingAsync(ip, 500);
			if (reply.Status == IPStatus.Success)
			{
                TreeViewItem lb = new TreeViewItem();
                lb.Header = ip;
                lb.Tag = "1";
                nbours.Items.Add(lb);
                nbours.Items.Refresh();
            }
		}

        bool tglBtn;

        private void scanIps_Click(object sender, RoutedEventArgs e)
        {
            if (tglBtn) return;
            tglBtn = !tglBtn;
            nbours.Items.Clear();
            tasks = new List<Task>();
            ips = new Dictionary<string, string>();


            string ip;
            for (int i = 1; i <= 255; i++)
            {
                ip = GetLocalIPAddress().Substring(0, GetLocalIPAddress().LastIndexOf(".") + 1) + i.ToString();
                Ping p = new Ping();
                var task = PingAndUpdateAsync(p, ip);
                tasks.Add(task);
            }

            this.Dispatcher.BeginInvoke(new Action(() => f()));
        }









        #region supercode


        /// <summary>
        /// used to rake the underlying packets;
        /// </summary>
        List<Monitor> monitorList = new List<Monitor>();

        /// <summary>
        /// presenting packets;
        /// </summary>
        List<Packet> pList = new List<Packet>();

        /// <summary>
        /// the packets sniffed -- all;
        /// </summary>
        List<Packet> allList = new List<Packet>();

        /// <summary>
        /// used to refresh the packets sniffed and listView and all the related info;
        /// </summary>
        /// <param name="p"></param>
        delegate void refresh(Packet p);




        private void startRaking()
        {
            monitorList.Clear();
            IPAddress[] hosts = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            if (hosts == null || hosts.Length == 0)
            {
                MessageBox.Show("No hosts detected, please check your network!");
            }
            for (int i = 0; i < hosts.Length; i++)
            {
                Monitor monitor = new Monitor(hosts[i]);
                monitor.newPacketEventHandler += new Monitor.NewPacketEventHandler(onNewPacket);
                monitorList.Add(monitor);
            }
            foreach (Monitor monitor in monitorList)
            {
                monitor.start();
            }
        }

        private void onNewPacket(Monitor monitor, Packet p)
        {

            this.Dispatcher.Invoke(new refresh(onRefresh), p);
            //this.BeginInvoke(new refresh(onRefresh), p);
        }

        private void onRefresh(Packet p)
        {
            string[] conditions = getFilterCondition();
            if (isIPOkay(p, conditions[0]) && isPORTOkay(p, conditions[1])
                && (conditions[2] == "" || conditions[2] == p.Type))
            {
                addAndUpdatePackets(p);
            }
            
        }


        private string[] getFilterCondition()
        {
            string[] conditions = { "", "", "" };
            string tmpString = filter.Text;
            int port = 0;
            IPAddress ip;
            if (tmpString.Contains('/') || tmpString.Contains(':'))//IP:PORT OR IP/PORT
            {
                string[] arr = { null, null };
                if (tmpString.Contains('/'))
                    arr = tmpString.Split(new char[] { '/' });
                else
                    arr = tmpString.Split(new char[] { ':' });
                conditions[0] = arr[0];
                conditions[1] = arr[1];
            }
            else if (int.TryParse(tmpString, out port))//just port;
                conditions[1] = port.ToString();
            else if (IPAddress.TryParse(tmpString, out ip))//just IP;
                conditions[0] = tmpString;
            else
                conditions[2] = tmpString.ToUpper();
            //Console.WriteLine(conditions);
            return conditions;
        }



        /// <summary>
        /// ip is "" or is equal to p.Src_IP or p.Des_IP;
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private bool isIPOkay(Packet p, string ip)
        {
            return ip == "" || p.Src_IP == ip || p.Des_IP == ip;
        }

        /// <summary>
        /// port is either "" or is equal to p.Src_PORT or p.Des_PORT;
        /// </summary>
        /// <param name="p"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool isPORTOkay(Packet p, string port)
        {
            return port == "" || p.Src_PORT == port || p.Des_PORT == port;
        }

        /// <summary>
        /// add one packet to pList and the listView;
        /// besides, refresh the totalCount, totalLength and allList globally;
        /// </summary>
        /// <param name="p"></param>
        private void addAndUpdatePackets(Packet p)
        {
            allList.Add(p);
            pList.Add(p);
            this.listView.Items.Add(new MyStr
            {
                srcIP = p.Src_IP + ":" + p.Src_PORT,
                endIP = p.Des_IP + ":" + p.Des_PORT,
                type = p.Type,
                time = p.Time,
                length = p.TotalLength.ToString(),
                data = p.getCharString()
            });
            ScrollToLastItem();
        }

        public void ScrollToLastItem()
        {
            if (listView.Items.Count > 0)
            {
                var listView1 = listView;
                listView1.ScrollIntoView(listView1.Items.GetItemAt(listView.Items.Count - 1));
                //item.Focus();
            }
        }


        /// <summary>
        /// stop sniffing the network;
        /// </summary>
        private void stopReceiving()
        {
            foreach (Monitor monitor in monitorList)
            {
                monitor.stop();
            }
        }

        /// <summary>
        /// when not selecting the list item, clear the details of the previous item;
        /// </summary>
        private void clearDetail()
        {
            this.charTextBox.Text = "";
            this.hexTextBox.Text = "";
        }


        /// <summary>
        /// Display all the sniffed packets in listView;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allButton_Click(object sender, EventArgs e)
        {
            this.listView.Items.Clear();
            pList.Clear();
            Packet p;
            for (int i = 0; i < allList.Count; i++)
            {
                p = allList[i];
                pList.Add(p);
                this.listView.Items.Add(new MyStr
                {
                    srcIP = p.Src_IP + ":"+ p.Src_PORT,
                    endIP = p.Des_IP + ":"+ p.Des_PORT,
                    type = p.Type,
                    time = p.Time,
                    length = p.TotalLength.ToString(),
                    data = p.getCharString()
                });
            }
            clearDetail();
        }


        //count a certain char in a string
        private int getCharCount(string s, char c)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// return the indexes of the substring - s0 in its parentString
        /// </summary>
        /// <param name="s">this value can be changed within the method</param>
        /// <param name="s0"></param>
        /// <returns></returns>
        private List<int> getStringIndex(string s, string s0)
        {
            List<int> countList = new List<int>();
            int index = 0;///indicate the current index
            while (s.Contains(s0))
            {
                index = s.IndexOf(s0);
                s = s.Substring(0, index + s0.Length);
                countList.Add(index);///record the relative indexes
            }
            for (int i = 1; i < countList.Count; i++)///get the absolute indexes
            {
                countList[i] += (countList[i - 1] + s0.Length);
            }
            return countList;
        }

        //private void charTextBox_SelectionChanged(object sender, System.EventArgs e)
        //{
        //    ///get the started position and the tmpString.Length
        //    string charString = this.charTextBox.Text;
        //    //int start0 = this.charTextBox.Text.IndexOf(this.charTextBox.SelectedText);///there can be exactly the same string as the selected
        //    string selectedString = this.charTextBox.SelectedText;
        //    int selectedLength = selectedString.Length;

        //    ///maybe this is not quite enough to make that difference outstanding
        //    ///just the same string just around the the start point
        //    ///ToDo!
        //    int start0 = this.charTextBox.SelectionStart - selectedLength;
        //    int start1 = this.charTextBox.SelectionStart;

        //    int index = 0;
        //    if (start0 > -1 && charString.Substring(start0, selectedLength).Equals(selectedString))
        //    {
        //        index = start0;
        //    }
        //    else
        //    {
        //        index = start1;
        //    }

        //    string tmpString = this.charTextBox.Text.Substring(0, index);
        //    int spaceCount = getCharCount(tmpString, '\n');
        //    /*
        //    string tmp = "I just love her, without any reason!";
        //    if (spaceCount > 1)
        //        MessageBox.Show(tmp.IndexOf("love").ToString());
        //    */
        //    int start = tmpString.Length * 3 - 2 * spaceCount;
        //    int selectedHexLength = this.charTextBox.SelectedText.Length * 3 - 2 * getCharCount(this.charTextBox.SelectedText, '\n');
        //    if (selectedHexLength > 0)
        //    {
        //        //reset backcolor
        //        this.hexTextBox.SelectionStart = 0;
        //        this.hexTextBox.SelectionLength = this.hexTextBox.Text.Length;
        //        this.hexTextBox.SelectionBackColor = Color.White;

        //        this.hexTextBox.SelectionStart = start;
        //        this.hexTextBox.SelectionLength = selectedHexLength;
        //        //this.hexTextBox.SelectionColor = Color.Red;
        //        this.hexTextBox.SelectionBackColor = Color.Red;
        //    }
        //    /*
        //    if (len > 50)
        //        MessageBox.Show(spaceCount.ToString());
        //   */
        //}

        /// <summary>
        /// in case the mouseClick event clear all the selection in charTextBox
        /// and the hexTextBox just remain the same - with backcolor changed to red;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void charTextBox_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (this.charTextBox.SelectedText.Length == 0)
        //    {
        //        //reset backcolor
        //        this.hexTextBox.SelectionStart = 0;
        //        this.hexTextBox.SelectionLength = this.hexTextBox.Text.Length;
        //        this.hexTextBox.SelectionBackColor = Color.White;
        //    }
        //}

        //private void filterButton_Click(object sender, EventArgs e)
        //{
        //    if (this.listView.Items.Count < 1)
        //    {
        //        MessageBox.Show("Please sniff or show all the sniffed packets first！");
        //    }
        //    showIPPackets(getFilterCondition());
        //    clearDetail();
        //}

        /// <summary>
        /// according to the ipTextBox and typeComboBox get the filter conditions including ip, port and type;
        /// default value "";
        /// </summary>
        /// <returns></returns>

        //private string[] getFilterCondition()
        //{
        //    string[] conditions = { "", "", "" };
        //    string tmpString = this.ipTextBox.Text;
        //    int port = 0;
        //    if (this.typeComboBox.SelectedIndex > -1)
        //        conditions[2] = this.typeComboBox.SelectedItem.ToString();
        //    if (tmpString.Contains('/') || tmpString.Contains(':'))//IP:PORT OR IP/PORT
        //    {
        //        string[] arr = { null, null };
        //        if (tmpString.Contains('/'))
        //            arr = tmpString.Split(new char[] { '/' });
        //        else
        //            arr = tmpString.Split(new char[] { ':' });
        //        conditions[0] = arr[0];
        //        conditions[1] = arr[1];
        //    }
        //    else if (int.TryParse(tmpString, out port))//just port;
        //        conditions[1] = port.ToString();
        //    else//just IP;
        //        conditions[0] = tmpString;
        //    //Console.WriteLine(conditions);
        //    return conditions;
        //}

        private void showIPPackets(string[] conditions)
        {
            string ipString = conditions[0];
            string port = conditions[1];
            string type = conditions[2];
            Packet p;
            this.listView.Items.Clear();
            pList.Clear();
            for (int i = 0; i < allList.Count; i++)
            {
                p = allList[i];
                if (isIPOkay(p, conditions[0]) && isPORTOkay(p, conditions[1])
                    && (conditions[2] == "" || conditions[2] == p.Type))
                {
                    pList.Add(p);
                    this.listView.Items.Add(new MyStr
                    {
                        srcIP = p.Src_IP + ":" + p.Src_PORT,
                        endIP = p.Des_IP + ":" + p.Des_PORT,
                        type = p.Type,
                        time = p.Time,
                        length = p.TotalLength.ToString(),
                        data = p.getCharString()
                    });
                }
            }
        }



        #endregion



        struct MyStr
        {
            public string srcIP { get; set; }
            public string endIP { get; set; }
            public string type { get; set; }
            public string time { get; set; }
            public string length { get; set; }
            public string data { get; set; }
        }


        private void start_Click(object sender, RoutedEventArgs e)
        {
            if ((string)start.Content == "Начать")
            {
                clearDetail();
                startRaking();
                start.Content = "Остановить";
                filter.IsEnabled = false;
            }
            else
            {
                clearDetail();
                stopReceiving();
                start.Content = "Начать";
                filter.IsEnabled = true;
            }
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            this.listView.Items.Clear();
            pList.Clear();
            clearDetail();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView.SelectedItem != null && listView.SelectedItems.Count != 0)
            {
                Packet p = pList[listView.SelectedIndex];
                this.hexTextBox.Text = p.getHexString();
                this.charTextBox.Text = p.getCharString();
            }
        }

        private void nbours_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            TreeViewItem a = ((TreeViewItem)nbours.SelectedItem);
            if (a.Tag.ToString() == "1")
            {
                filter.Text = a.Header.ToString();
            }
        }
    }
}
