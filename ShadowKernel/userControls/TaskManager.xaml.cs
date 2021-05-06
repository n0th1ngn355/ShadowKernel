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
using static ShadowKernel.Classes.Server;

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для TaskManager.xaml
    /// </summary>
    public partial class TaskManager : Window
    {
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public TaskManager()
        {
            InitializeComponent();
            Update = true;
            this.Left = 0;
            this.Top = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
        }

        private void dtgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void  KillProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SettingsServer.Process myItem = (SettingsServer.Process)clientProcesses.SelectedItem;
                MainServer.Send(Convert.ToInt16(ConnectionID), Encoding.UTF8.GetBytes("EndProcess<{" + myItem.pID + "}>"));
                MainServer.Send(Convert.ToInt16(ConnectionID), Encoding.UTF8.GetBytes("GetProcesses"));
            }
            catch { }

        }

        private void RefreshProc_Click(object sender, RoutedEventArgs e)
        {
            MainServer.Send(Convert.ToInt16(ConnectionID), Encoding.UTF8.GetBytes("GetProcesses"));
            
        }
    }
}
