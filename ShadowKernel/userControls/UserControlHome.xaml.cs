using ShadowKernel.helper;
using modelFirst.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShadowKernel.userControls
{

    public partial class UserControlHome : UserControl
    {
        public UserControlHome()
        {
            InitializeComponent();
            if (Session.CurrentAuditer.Login == "Админ")
            {
                cbxC.Visibility = Visibility.Visible;
            }
            else
            {
                dtgAudits.ItemsSource = Session.CurrentAuditer.Audits.ToList();
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
            e.Column.Header = propertyDescriptor.DisplayName;
            if (propertyDescriptor.DisplayName == "Audits" ||propertyDescriptor.DisplayName == "Password" ||propertyDescriptor.DisplayName == "Id" || propertyDescriptor.DisplayName == "UpdateAt" || propertyDescriptor.DisplayName == "Auditer" || propertyDescriptor.DisplayName == "Questions")
            {
                e.Cancel = true;
            }
            if (propertyDescriptor.DisplayName == "FirstName")
            {
                e.Column.Header = "Имя";
            }
            if (propertyDescriptor.DisplayName == "LastName")
            {
                e.Column.Header = "Фамилия";
            }
            if (propertyDescriptor.DisplayName == "Login")
            {
                e.Column.Header = "Логин";
            }
            if (propertyDescriptor.DisplayName == "AuditedCompanyName")
            {
                e.Column.Header = "Название компании";
            }
            if (propertyDescriptor.DisplayName == "Name")
            {
                e.Column.Header = "Аудит";
            }
            if (propertyDescriptor.DisplayName == "CreatedAt")
            {
                e.Column.Header = "Создано";
            }
            if (propertyDescriptor.DisplayName == "UpdateAt")
            {
                e.Column.Header = "Изменено";
            }

        }

        private void DtgAudits_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new Audit();
            ((Audit)e.NewItem).Auditer = Session.CurrentAuditer;
            List<Question> questions = new List<Question>();
            foreach (var q in Session.AuditContext.Questions.Include("Audits").Include("Category").Include("Answers").ToList())
            {
                q.Audits.Add(((Audit)e.NewItem));
            }
            ((Audit)e.NewItem).Questions = new System.Collections.ObjectModel.ObservableCollection<Question>(questions);

        }

        private void dtgAudits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dtgAudits.SelectedItem != null)
                {
                    if (cbxC.Visibility == Visibility.Visible)
                    {
                        ComboBoxItem it = (ComboBoxItem)cbxC.SelectedItem;
                        if (it.Content.ToString() == "Аудиторы")
                            Session.AuditContext.Auditers.Remove((Auditer)dtgAudits.SelectedItem);
                    }
                    else
                    {
                        Session.AuditContext.Audits.Remove((Audit)dtgAudits.SelectedItem);
                        MainWindow wind = (MainWindow)Window.GetWindow(this);
                        wind.uscAudit = new UserControlCreate();
                    }
                    Session.Save();
                    admUpdate();
                }
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Session.Save();
                admUpdate();
            }
            catch { }
            
        }

        private void admUpdate()
        {
            if (cbxC.Visibility == Visibility.Visible)
            {
                ComboBoxItem it = (ComboBoxItem)cbxC.SelectedItem;
                if (it.Content.ToString() == "Аудиторы")
                {
                    dtgAudits.ItemsSource = Session.AuditContext.Auditers.Where(a => a.Login != "Админ").ToList();
                    dtgAudits.CanUserAddRows = false;
                    dtgAudits.Items.Refresh();
                }
                else
                {
                    Update();
                }
            }
            else
            {
                Update();
            }
        }
        private void Update()
        {
            dtgAudits.ItemsSource = Session.CurrentAuditer.Audits.ToList();
            dtgAudits.CanUserAddRows = true;
            dtgAudits.Items.Refresh();
        }

        private void cbxC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            admUpdate();
        }
    }
}
