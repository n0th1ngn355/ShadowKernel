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

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для CompInfo.xaml
    /// </summary>
    public partial class CompInfo : Window
    {

        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public CompInfo()
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
    }
}
