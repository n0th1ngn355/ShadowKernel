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
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using modelFirst;
using modelFirst.Model;
using ShadowKernel.helper;
namespace ShadowKernel
{
    /// <summary>
    /// Логика взаимодействия для SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        public bool IsPerCabinet;
        public MainWindow main;
        public SignUp()
        {
            InitializeComponent();
            Session.AuditContext = new AuditContext();

            Auditer auditer = Session.AuditContext.Auditers.FirstOrDefault();

        }
        public SignUp(MainWindow window)
        {
            InitializeComponent();
            Session.AuditContext = new AuditContext();

            Auditer auditer = Session.AuditContext.Auditers.FirstOrDefault();
            main = window;
        }
        private void CloseButton(object sender, RoutedEventArgs e)
        {
            if (IsPerCabinet)
            {
                Close();
                main.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsPerCabinet)
            {
                Auditer auditer1 = Session.AuditContext.Auditers.Where(a => a.Login == Session.CurrentAuditer.Login).First();
                mainLbl.Content = auditer1.Login;
                ButtonLogin.Content = "УДАЛИТЬ АККАУНТ";
                BrushConverter con = new BrushConverter();
                ButtonLogin.Foreground = (Brush)con.ConvertFromString("#FF631919");
                ButtonSignUp.Content = "СОХРАНИТЬ";
                txtName.Text = auditer1.FirstName;
                txtSurName.Text = auditer1.LastName;
                HintAssist.SetHint(txtLogin, "ПАРОЛЬ");
                HintAssist.SetHint(txtPassword, "НОВЫЙ ПАРОЛЬ");
                HintAssist.SetHint(txtPassword1, "НОВЫЙ ПАРОЛЬ");
                txtLogin.Visibility = Visibility.Collapsed;
                txtOldPassword.Visibility = Visibility.Visible;
                pswdCh.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                txtRepPassword.Visibility = Visibility.Collapsed;
            }
            txtName.Focus();

        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (IsPerCabinet)
            {
                if(System.Windows.Forms.MessageBox.Show("Вы уверены, что хотите удалить данного пользователя?", Title, System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    Session.AuditContext.Auditers.Remove(Session.AuditContext.Auditers.Where(a => a.Login == Session.CurrentAuditer.Login).First());
                    Session.Save();
                    main.notifyIcon.Dispose();
                    Window login = new Login();
                    login.Show();
                    Close();

                }
            }
            else
            {
                Window login = new Login();
                login.Show();
                Close();
            }
        }

        private void ButtonSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text == "" | txtName.Text == " " | txtSurName.Text == "" | txtSurName.Text == " " | (!IsPerCabinet &(txtLogin.Text == "" | txtLogin.Text == " "))) { ErrorLbl.Content = "Некоторые обязательные поля были пропущены!"; return; }
            if (pswd.IsChecked.Value)
            {
                if (txtPassword1.Text != txtRepPassword1.Text) { ErrorLbl.Content = "Пароли не совпадают!"; return; }
                if (pswdCh.IsChecked.Value & (txtPassword1.Text == "" | txtPassword1.Text == " " | txtRepPassword1.Text == "" | txtRepPassword1.Text == " ")) { ErrorLbl.Content = "Необходимо ввести пароль!"; return; }
            }
            else
            {
                if (txtPassword.Password != txtRepPassword.Password) { ErrorLbl.Content = "Пароли не совпадают!"; return; }
                if (pswdCh.IsChecked.Value & (txtPassword.Password == "" | txtPassword.Password == " " | txtRepPassword.Password == "" | txtRepPassword.Password == " ")) { ErrorLbl.Content = "Необходимо ввести пароль!"; return; }
            }


            if (IsPerCabinet)
            {
                Auditer auditer1 = Session.AuditContext.Auditers.Where(a => a.Login == Session.CurrentAuditer.Login).First();
                if (pswdCh.IsChecked.Value)
                {
                    if (pswd.IsChecked.Value)
                    {
                        if (!BCrypt.Net.BCrypt.Verify(txtLogin.Text, auditer1.Password, false, BCrypt.Net.HashType.SHA256)) { ErrorLbl.Content = "Введён неверный пароль!"; return; }
                        auditer1.Password = BCrypt.Net.BCrypt.HashPassword(txtPassword1.Text, BCrypt.Net.BCrypt.GenerateSalt(), false, BCrypt.Net.HashType.SHA256);
                    }
                    else
                    {
                        if (!BCrypt.Net.BCrypt.Verify(txtOldPassword.Password, auditer1.Password, false, BCrypt.Net.HashType.SHA256)) { ErrorLbl.Content = "Введён неверный пароль!"; return; }
                        auditer1.Password = BCrypt.Net.BCrypt.HashPassword(txtPassword.Password, BCrypt.Net.BCrypt.GenerateSalt(), false, BCrypt.Net.HashType.SHA256);
                    }
                }
                auditer1.FirstName = txtName.Text;
                auditer1.LastName = txtSurName.Text;
                Session.Save();
                Close();

                main.Show();
                return;
            }

            if (Session.AuditContext.Auditers.Where(q => q.Login == txtLogin.Text).Count() > 0) { ErrorLbl.Content = "Аккаунт с таким логином уже зарегистрирован!"; return; }
            Auditer auditer = new Auditer()
            {
                Id = Session.AuditContext.Auditers.Count() + 1,
                FirstName = txtName.Text,
                LastName = txtSurName.Text,
                Login = txtLogin.Text,
                Password = BCrypt.Net.BCrypt.HashPassword(pswd.IsChecked.Value?txtPassword1.Text:txtPassword.Password, BCrypt.Net.BCrypt.GenerateSalt(), false, BCrypt.Net.HashType.SHA256),
                CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                UpdateAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")

            };
            Session.CurrentAuditer = auditer;
            Session.AuditContext.Auditers.Add(auditer);

            Audit audit = new Audit()
            {
                Id = 1,
                Name = "Аудит 1",
                AuditedCompanyName = "Компания 1",
                CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                UpdateAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            audit.Auditer = auditer;
            List<Question> questions = new List<Question>();
            foreach (var q in Session.AuditContext.Questions.Include("Audits").Include("Category").Include("Answers").ToList())
            {
                q.Audits.Add(audit);
            }
            (audit).Questions = new System.Collections.ObjectModel.ObservableCollection<Question>(questions);
            Session.AuditContext.Audits.Add(audit);
            Session.Save();
            Window mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void pswd_Checked(object sender, RoutedEventArgs e)
        {
            if (pswdCh.IsChecked.Value | pswdCh.Visibility != Visibility.Visible)
            {
                txtPassword1.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPassword1.Visibility = Visibility.Visible;
                txtRepPassword1.Text = txtRepPassword.Password;
                txtRepPassword.Visibility = Visibility.Collapsed;
                txtRepPassword1.Visibility = Visibility.Visible;
            }
            if (IsPerCabinet)
            {
                txtLogin.Text = txtOldPassword.Password;
                txtLogin.Visibility = Visibility.Visible;
                txtOldPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void pswd_Unchecked(object sender, RoutedEventArgs e)
        {
            if (pswdCh.IsChecked.Value | pswdCh.Visibility != Visibility.Visible)
            {
                txtPassword.Password = txtPassword1.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPassword1.Visibility = Visibility.Collapsed;
                txtRepPassword.Password = txtRepPassword1.Text;
                txtRepPassword.Visibility = Visibility.Visible;
                txtRepPassword1.Visibility = Visibility.Collapsed;
            }
            if (IsPerCabinet)
            {
                txtOldPassword.Password = txtLogin.Text;
                txtLogin.Visibility = Visibility.Collapsed;
                txtOldPassword.Visibility = Visibility.Visible;
            }
        }

        private void txtRepPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(txtPassword.Password == txtRepPassword.Password) ErrorLbl.Content = "";
        }

        private void txtRepPassword1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword1.Text == txtRepPassword1.Text) ErrorLbl.Content = "";
        }

        private void pswdCh_Checked(object sender, RoutedEventArgs e)
        {
            if (pswd.IsChecked.Value)
            {
                txtPassword1.Visibility = Visibility.Visible;
                txtRepPassword1.Visibility = Visibility.Visible;
            }
            else
            {
                txtPassword.Visibility = Visibility.Visible;
                txtRepPassword.Visibility = Visibility.Visible;
            }
        }

        private void pswdCh_Unchecked(object sender, RoutedEventArgs e)
        {
            if (pswd.IsChecked.Value)
            {
                txtPassword1.Visibility = Visibility.Collapsed;
                txtRepPassword1.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtPassword.Visibility = Visibility.Collapsed;
                txtRepPassword.Visibility = Visibility.Collapsed;
            }
        }
    }
}
