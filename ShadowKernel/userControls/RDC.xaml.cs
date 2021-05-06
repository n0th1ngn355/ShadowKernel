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
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static ShadowKernel.Classes.Server;
using Color = System.Drawing.Color;

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для RDC.xaml
    /// </summary>
    public partial class RDC : Window
    {
        public int ConnectionID { get; set; }
        public static bool Update { get; set; }

        public BitmapImage bitm = new BitmapImage();

        public RDC()
        {
            
            InitializeComponent();
            Update = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StopRD"));
        }

    }
}
