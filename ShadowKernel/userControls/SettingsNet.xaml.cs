using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Media;
namespace ShadowKernel.userControls
{

    public partial class SettingsNet : System.Windows.Controls.UserControl
    {

        public SettingsNet()
        {
            InitializeComponent();
            Init();

        }
        public void Init()
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            SettingsClient stgClient = (SettingsClient)wind.stgClient;
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(stgClient);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            SettingsServer stgServer = (SettingsServer)wind.stgServer;
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(stgServer);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            
        }
    }
}