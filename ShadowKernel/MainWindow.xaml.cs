using ShadowKernel.userControls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using ShadowKernel.helper;

namespace ShadowKernel
{
    public partial class MainWindow : Window
    {
        public UserControl uscDashboard = null;
        public UserControl uscAudit = null;
        public UserControl server = null;
        public UserControl stgServer = null;
        public UserControl stgNet = null;
        public UserControl net = null;
        public System.Windows.Forms.NotifyIcon notifyIcon = null;
        public NotifyContextMenu n;

        public MainWindow()
        {

            InitializeComponent();
            Init();

            n = new NotifyContextMenu(this);

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "ShadowKernel";
            notifyIcon.Icon = Properties.Resources.icon;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += new EventHandler(NotifyIcon_DoubleClick);
            notifyIcon.MouseClick += NotifyIcon_MouseClick; 
        }

        public void Init()
        {
            if (Session.AuditContext.Audits.Where(a => a.Auditer.Login == Session.CurrentAuditer.Login).Count() > 0)
                Session.AuditContext.Entry(Session.CurrentAuditer).Collection(a => a.Audits).Load();
            this.DataContext = Session.CurrentAuditer;

            uscDashboard = new UserControlHome();
            uscAudit = new UserControlCreate();
            server = new Server();
            stgServer = new SettingsServer(this);
            stgNet = new SettingsNet();
            net = new Net();

            GridMain.Children.Add(server);

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if((e.Button == System.Windows.Forms.MouseButtons.Right) && (n.Visibility != Visibility.Visible))
            {
                n.Left = SystemParameters.PrimaryScreenWidth;
                n.Visibility = Visibility.Visible;
                n.Aud.Content = AC.Text;
                n.Serv.Content = serverText.Text;
                SettingsServer s = (SettingsServer)stgServer;
                if (s.notify.IsChecked.Value)
                {
                    n.notif.Content = "Откл. уведомления";
                }
                else
                {
                    n.notif.Content = "Вкл. уведомления";
                }
                n.Resources["SBrush"] = new SolidColorBrush(((SolidColorBrush)serverInd.Fill).Color);
                n.Activate();
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {

            this.Hide();
        }

        private void MinimizeButton(object sender, RoutedEventArgs e)
        {
            
            this.WindowState = WindowState.Minimized;

        }


        
       

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((ListViewItem)((ListView)sender).SelectedItem).Name == "Menu")
            {
                return;
            }
            GridMain.Children.Clear();

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemHome":
                    GridMain.Children.Add(uscDashboard);
                    break;
                case "ItemCreate":
                    GridMain.Children.Add(uscAudit);
                    break;
                case "Server":
                    GridMain.Children.Add(server);
                    break;
                case "Net":
                    GridMain.Children.Add(net);
                    break;
                case "Settings":
                    GridMain.Children.Add(stgServer);
                    break;
                case "Logout":
                    BtnLogout();
                    break;
                case "Quit":
                    Application.Current.Shutdown();
                    notifyIcon.Dispose();
                    break;
                default:
                    break;
            }
            if(tgl.IsChecked.Value)
            {
                tgl.IsChecked = !tgl.IsChecked;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            Session.Save();
        }

        public void BtnLogout()
        {
            Session.AuditContext.Dispose();
            Session.CurrentAuditer = new modelFirst.Model.Auditer();
            Window login = new Login();
            login.Show();
            Close();
            notifyIcon.Dispose();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            btnMenu.StaysOpen = true;
            btnMenu.IsOpen = true;
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            BtnLogout();
        }

        private void PC_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SignUp signUp = new SignUp(this);
            signUp.IsPerCabinet = true;
            signUp.Show();
        }

        private void Menu_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tgl.IsChecked = !tgl.IsChecked;
        }

    }
}
