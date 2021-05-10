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
    /// Логика взаимодействия для Chat.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        public int ConnectionID { get; set; }
        public bool Update { get; set; }
        public MainWindow wind;
        public ChatControl(MainWindow main)
        {
            InitializeComponent(); 
            Update = true;
            wind = main;
        }


        private void dtgClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if ((SettingsServer.MyItem)dtgClients.SelectedItem == null) { chatPlace.Children.Clear(); return; }
                SettingsServer.MyItem myItem = (SettingsServer.MyItem)dtgClients.SelectedItem;
                SettingsServer stg = (SettingsServer)wind.stgServer;
                chatPlace.Children.Clear();
                Chat chat = stg.chats[Convert.ToInt32(myItem.ID)];
                chat.title.Text = "Чат с " + myItem.Tag + " (" + myItem.IP + ")";
                chat.ConnectionID = Convert.ToInt32(myItem.ID);
                chatPlace.Children.Add(chat);
            }
            catch
            {

            }
        }
    }
}
