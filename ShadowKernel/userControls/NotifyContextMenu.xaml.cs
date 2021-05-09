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
using System.Windows.Media.Animation;

namespace ShadowKernel.userControls
{
    /// <summary>
    /// Логика взаимодействия для NotifyContextMenu.xaml
    /// </summary>
    public partial class NotifyContextMenu : Window
    {
        public MainWindow wind;
        public NotifyContextMenu(MainWindow main)
        {
            ShowInTaskbar = false;
            InitializeComponent();
            int PSBH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int TaskBarHeight = PSBH - System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;

            this.Left = SystemParameters.PrimaryScreenWidth;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - TaskBarHeight;

            Storyboard sb1 = new Storyboard();

            DoubleAnimation an1 = new DoubleAnimation();
            DoubleAnimation an2 = new DoubleAnimation();
            an1.From = SystemParameters.PrimaryScreenWidth;
            an1.To = SystemParameters.PrimaryScreenWidth - this.Width;
            an1.Duration = TimeSpan.FromMilliseconds(300);
            Storyboard.SetTargetProperty(an1, new PropertyPath(Window.LeftProperty));
            an2.From = 0;
            an2.To = 1;
            an2.Duration = TimeSpan.FromMilliseconds(800);
            Storyboard.SetTargetProperty(an2, new PropertyPath(Window.OpacityProperty));
            sb1.Children.Add(an1);
            sb1.Children.Add(an2);


            Storyboard sb2 = new Storyboard();

            DoubleAnimation an3 = new DoubleAnimation();
            DoubleAnimation an4 = new DoubleAnimation();
            an3.From = SystemParameters.PrimaryScreenWidth - this.Width;
            an3.To =  SystemParameters.PrimaryScreenWidth;
            an3.Duration = TimeSpan.FromMilliseconds(300);
            Storyboard.SetTargetProperty(an3, new PropertyPath(Window.LeftProperty));
            an4.From = 1;
            an4.To = 0;
            an4.Duration = TimeSpan.FromMilliseconds(800);
            Storyboard.SetTargetProperty(an4, new PropertyPath(Window.OpacityProperty));
            sb2.Children.Add(an3);
            sb2.Children.Add(an4);


            Trigger trigger = new Trigger();
            trigger.Property = Window.VisibilityProperty;
            trigger.Value = Visibility.Visible;
            trigger.EnterActions.Add(new BeginStoryboard() {Storyboard = sb1 });
            trigger.ExitActions.Add(new BeginStoryboard() {Storyboard = sb2 });
            Style s = new Style();
            s.Triggers.Add(trigger);
            this.Style = s;


            wind = main;

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;
            Left = SystemParameters.PrimaryScreenWidth;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void tglServer_Click(object sender, RoutedEventArgs e)
        {
            SettingsServer stg =(SettingsServer) wind.stgServer;
            stg.Button_Click(null, null);
            this.Aud.Content = wind.AC.Text;
            this.Serv.Content = wind.serverText.Text;
            this.Resources["SBrush"] = new SolidColorBrush(((SolidColorBrush)wind.serverInd.Fill).Color);
        }

        private void notifOff_Click(object sender, RoutedEventArgs e)
        {
            SettingsServer stg = (SettingsServer)wind.stgServer;
            stg.notify.IsChecked = !stg.notify.IsChecked;
            if (stg.notify.IsChecked.Value)
            {
                notif.Content = "Откл. уведомления";
            }
            else
            {
                notif.Content = "Вкл. уведомления";
            }
        }

        private void AC_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).ContextMenu.IsOpen= true;
        }

        private void PC_Click(object sender, RoutedEventArgs e)
        {
            wind.Hide();
            SignUp signUp = new SignUp(wind);
            signUp.IsPerCabinet = true;
            signUp.Show();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            wind.BtnLogout();
        }

        private void stgs_Click(object sender, RoutedEventArgs e)
        {
            wind.Show(); 
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(wind.stgServer);
        }
    }
}
