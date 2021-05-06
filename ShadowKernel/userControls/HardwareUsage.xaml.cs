using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
    /// Логика взаимодействия для HardwareUsage.xaml
    /// </summary>
    public partial class HardwareUsage : Window
    {
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public bool UpdateForm { get; set; }

        public HardwareUsage()
        {
            InitializeComponent();
            Update = true;
            UpdateForm = true;
            this.Left = SystemParameters.PrimaryScreenWidth/2 - 350;
            this.Top = 10;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
            UpdateForm = false;
            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StopUsageStream"));
            Thread.Sleep(1000);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void CPU_TextChanged(object sender, TextChangedEventArgs e)
        {
            cpuArc.EndAngle = Convert.ToInt16(CPU.Text) * 2.4 - 120;
        }

        private void RAM_TextChanged(object sender, TextChangedEventArgs e)
        {
            ramArc.EndAngle = Convert.ToInt16(RAM.Text) * 2.4 - 120;
        }

        private void DISK_TextChanged(object sender, TextChangedEventArgs e)
        {
            diskArc.EndAngle = Convert.ToInt16(DISK.Text) * 2.4 - 120;
        }

    }
}
