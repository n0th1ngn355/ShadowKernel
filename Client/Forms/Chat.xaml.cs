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
using Client.Helpers;
using Client.Helpers.Networking;

namespace Client.Forms
{
    /// <summary>
    /// Логика взаимодействия для Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {

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
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.MessageType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes(msg.Text));
                    Networking.MainClient.Send(ToSend.ToArray()); 
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
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int)DataType.MessageType);
                    ToSend.AddRange(Encoding.UTF8.GetBytes(msg.Text));
                    Networking.MainClient.Send(ToSend.ToArray());
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
