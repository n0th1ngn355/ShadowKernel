using ShadowKernel.helper;
using modelFirst;
using modelFirst.Model;
using System;
using System.Linq;
using System.Windows;
using HandlingPdf.pdf;
using System.Reflection;
using System.IO;
namespace ShadowKernel
{

    public partial class Login : Window
    {
        public MainWindow wind;
        public Login()
        {
            InitializeComponent();
            Init();
        }
        public void Init()
        {
            Session.CurrentAuditer = new Auditer();
            Session.LoginAttemptCount = 0;
            Session.AuditContext = new AuditContext();

            Auditer auditer = Session.AuditContext.Auditers.FirstOrDefault();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            Session.LoginAttemptCount++;

            string login = "";
            string password = "";

            login = txtboxLogin.Text;
            if (pswd.IsChecked.Value)
            {
                password = txtPassword1.Text;
            }
            else
            {
                password = txtPassword.Password;
            }
            //password = BCrypt.Net.BCrypt.HashPassword(password);

            //Console.WriteLine($"{email} + {password}");

            
                Auditer auditer = Session.AuditContext.Auditers.Include("Audits.Questions.Answers").FirstOrDefault(a => a.Login.Equals(login));
                if (auditer != null && BCrypt.Net.BCrypt.Verify(password, auditer.Password, false, BCrypt.Net.HashType.SHA256))
                {
                    Session.CurrentAuditer = auditer;
                    Window mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                

                }
                else
                {
                    txtblocError.Text = "Некорректный Логин или Пароль, Попробуйте еще раз";
                if (Session.LoginAttemptCount > 3)
                {
                    MessageBox.Show("Слишком много попыток. Приложение закроется.", App.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    App.Current.Shutdown();
                }
            }

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtboxLogin.Focus();
        }

        private void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
            Window signUp = new SignUp();
            signUp.Show();
            Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void pswd_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPassword1.Text;
            txtPassword.Visibility = Visibility.Visible;
            txtPassword1.Visibility = Visibility.Collapsed;
        }

        private void pswd_Checked(object sender, RoutedEventArgs e)
        {
            txtPassword1.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPassword1.Visibility = Visibility.Visible;
        }
    }
}
