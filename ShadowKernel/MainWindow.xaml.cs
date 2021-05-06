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
        public UserControl stgClient = null;
        public UserControl stgNet = null;
        public UserControl net = null;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        public void Init()
        {
            if(Session.AuditContext.Audits.Where(a=> a.Auditer.Login == Session.CurrentAuditer.Login).Count() > 0)
            Session.AuditContext.Entry(Session.CurrentAuditer).Collection(a => a.Audits).Load();
            this.DataContext = Session.CurrentAuditer;

            uscDashboard = new UserControlHome();
            uscAudit = new UserControlCreate();
            server = new Server();
            stgServer = new SettingsServer(this);
            stgClient = new SettingsClient();
            stgNet = new SettingsNet();
            net = new Net();

            GridMain.Children.Add(server);
        }
       

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                    break;
                case "Menu":
                    GridMain.Children.Add(server);
                    break;
                default:
                    break;
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
    }
}
