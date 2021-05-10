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
    public partial class Chat : UserControl
    {
        public int ConnectionID { get; set; }
        public Chat()
        { 
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void msg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !string.IsNullOrWhiteSpace(msg.Text))
                try
                {
                    MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("[<MESSAGE>]" + msg.Text));
                    ContentControl t = new ContentControl();
                    t.Content = msg.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm");
                    Style style = this.FindResource("BubbleRightStyle") as Style;
                    t.Style = style;
                    chatPlace.Children.Add(t);
                    msg.Text = "";
                    scrl.ScrollToBottom();
                }
                catch
                {

                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(msg.Text))
                try
                {
                    MainServer.Send(ConnectionID, Encoding.UTF8.GetBytes("[<MESSAGE>]" + msg.Text));
                    ContentControl t = new ContentControl();
                    t.Content = msg.Text + Environment.NewLine + DateTime.Now.ToString("HH:mm");
                    Style style = this.FindResource("BubbleRightStyle") as Style;
                    t.Style = style;
                    chatPlace.Children.Add(t);
                    msg.Text = "";
                    scrl.ScrollToBottom();
                }
                catch
                {

                }
        }
    }
}
