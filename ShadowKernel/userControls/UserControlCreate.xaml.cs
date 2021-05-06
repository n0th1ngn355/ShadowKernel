using ShadowKernel.helper;
using HandlingPdf.pdf;
using MaterialDesignThemes.Wpf;
using modelFirst.Model;
using System;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Microsoft.Expression.Shapes;
using System.Windows.Media;
namespace ShadowKernel.userControls
{

    public partial class UserControlCreate : System.Windows.Controls.UserControl
    {
        private int i;
        private List<int> possibleScores;
        Audit audit;
        Question question;
        PdfGenerator generator;

        public UserControlCreate()
        {
            InitializeComponent();
            Init();
            cbxAudits.Items.Refresh();
        }
        public void Init()
        {
            SnackbarOne.IsActive = false;
            cbxAudits.ItemsSource = Session.AuditContext.Audits.ToList();
            cbxAudits.DisplayMemberPath = "Name";
            i = 0;
            

            generator = new PdfGenerator();
            mbxAnswer.DataContext = new Question();
            stkpAnswer.DataContext = new Answer();
        }


        private void CbxCategories_GotFocus(object sender, RoutedEventArgs e)
        {
            if (cbxAudits.SelectedItem == null && i < 1)
            {
                //MessageBox.Show("Please Choose an audit first", App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                dlgHost1.ShowDialog(mbx);

                cbxAudits.Focus();
                i++;
            }
        }

        private void CbxCategories_LostFocus(object sender, RoutedEventArgs e)
        {
            i = 0;
        }

        private void CbxAudits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            dtgQuestions.ItemsSource = "";
            audit = new Audit();
            audit = (Audit)cbxAudits.SelectedItem;
            //Session.AuditContext.Entry(audit).Collection(a => a.Questions).Load();
            
            cbxCategories.ItemsSource = Session.AuditContext.Categories.Where(c => c.Questions.Any(q => q.Audits.Any(a => a.Id == audit.Id))).ToList();
            cbxCategories.DisplayMemberPath = "Name";
            SnackbarOne.IsActive = true;

            foreach (var quest in Session.AuditContext.Questions)
            {
                quest.Answer = "";
            }
            foreach (var quest in Session.AuditContext.Questions.Where(q=> q.Answers.Any(a=>a.Audit.Id==audit.Id)))
            {
                quest.Answer = quest.Answers.Where(a=> a.Audit.Id == audit.Id).FirstOrDefault().Reply.ToString();

            }
            Session.Save();

        }

        private void CbxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
                SnackbarOne.IsActive = false;

                SnackbarTwo.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(1));
                SnackbarTwo.MessageQueue.Enqueue("Загрузка вопросов...");
                SnackbarTwo.IsActive = true;

                audit = new Audit();
                audit = (Audit)cbxAudits.SelectedItem;
                // Session.AuditContext.Entry(audit).Collection(a => a.Questions).Load();

                Category category = new Category();
                category = (Category)cbxCategories.SelectedItem;

                dtgQuestions.ItemsSource = audit.Questions.Where(q => q.Category.Id == category.Id);


                dtgQuestions.BringIntoView();
            
        }

        private void DtgQuestions_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
            e.Column.Header = propertyDescriptor.DisplayName;
            e.Column.MinWidth = 100;
            if (propertyDescriptor.DisplayName == "Recommandation" ||propertyDescriptor.DisplayName == "Answer" ||propertyDescriptor.DisplayName == "UpdateAt" || propertyDescriptor.DisplayName == "CreatedAt" || propertyDescriptor.DisplayName == "Coefficient" || propertyDescriptor.DisplayName == "Audits" || propertyDescriptor.DisplayName == "Answers" || propertyDescriptor.DisplayName == "Category" || propertyDescriptor.DisplayName == "Risk" || propertyDescriptor.DisplayName == "Details" || propertyDescriptor.DisplayName == "Scale")
            {
                e.Cancel = true;
                
            }
            
            switch (propertyDescriptor.DisplayName)
            {
                case "Intitled":
                    e.Column.Header = "Вопрос";
                    break;
                case "Recommandation":
                    e.Column.Header = "Рекомендация";
                    break;
                case "CreatedAt":
                    e.Column.Header = "Создано";
                    break;
                case "UpdateAt":
                    e.Column.Header = "Изменено";
                    break;
                default:
                    break;
            }

        }

        private void DtgQuestions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine("****************************************************");

            //mbxAnswer.DataContext = new Question();
            //stkpAnswer.DataContext = new Answer();
            if (!dlhHost2.IsVisible)
            {
                question = new Question();
                question = (Question)dtgQuestions.SelectedItem;
                if (question != null)
                {
                    Answer answer = question.Answers.FirstOrDefault(a => a.Audit.Id == ((Audit)cbxAudits.SelectedItem).Id);
                    if (answer == null)
                    {
                        answer = new Answer
                        {
                            Reply = null
                        };
                        answer.Audit = (Audit)cbxAudits.SelectedItem;
                        question.Answers.Add(answer);
                        
                        
                    }

                    
                    if (String.IsNullOrEmpty(answer.RecommandationToApply))
                    {
                        answer.RecommandationToApply = question.Recommandation;
                    }
                    
                    //answer.RiskIncurred = question.Risk;
                    mbxAnswer.DataContext = question;
                    stkpAnswer.DataContext = answer;
                    question.Answer = answer.Reply.ToString();

                    

                    dlhHost2.Visibility = Visibility.Visible;
                    dlhHost2.ShowDialog(mbxAnswer);
                }
                

            }
            else
                {
                    //Console.WriteLine("nnnnnnnnnnnnnnnnnnnnnnnnnnnnnn");
                    SnackbarTwo.MessageQueue.Enqueue("Сначала ответьте на вопрос");
                }
            if(question != null)audit.Questions.First(q => q.Id == question.Id).Answer = question.Answers.Where(a => a.Audit.Id == audit.Id).FirstOrDefault().Reply.ToString();
            Category category = new Category();
                    category = (Category)cbxCategories.SelectedItem;
                    dtgQuestions.ItemsSource = audit.Questions.Where(q => q.Category.Id == category.Id);
                    dtgQuestions.BringIntoView();

        }

        private void ButtonSaveAnswer_Click(object sender, RoutedEventArgs e)
        {
            dlhHost2.Visibility = Visibility.Hidden;
            if (question != null) audit.Questions.First(q => q.Id == question.Id).Answer = question.Answers.Where(a => a.Audit.Id == audit.Id).FirstOrDefault().Reply.ToString();
            Category category = new Category();
            category = (Category)cbxCategories.SelectedItem;
            dtgQuestions.ItemsSource = audit.Questions.Where(q => q.Category.Id == category.Id);
            dtgQuestions.BringIntoView();

            EnqueueThisMessageInSnackbarTwo("Ответ сохранён");
            Session.Save();

        }

        private void EnqueueThisMessageInSnackbarTwo(String message)
        {
            SnackbarTwo.MessageQueue.Enqueue(message);
            SnackbarTwo.IsActive = true;
        }

        private async void ButtonGenerateReport_Click(object sender, RoutedEventArgs e)
        {
          
                audit = (Audit)cbxAudits.SelectedItem;



                
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "Отчёт_" + audit.Name; // Default file name
                dlg.DefaultExt = ".pdf"; // Default file extension
                dlg.Filter = "(.pdf)|*.pdf|All files(*.*)|*.*";

                // Show save file dialog box

                // Process save file dialog box results
                if (dlg.ShowDialog() != DialogResult.Cancel)
                {
                    // Save document
                    string filename = dlg.FileName;
                    generator.OuthPutPath = filename;

                    
                }
                else { return; }
                generator.EmployeeName = Session.CurrentAuditer.FirstName + " " + Session.CurrentAuditer.LastName;
                generator.CompanyName = audit.AuditedCompanyName;
                generator.Auditedcompany = audit.AuditedCompanyName;
                UnHideProgressBar();
                dlgHost3.ShowDialog(mbxProgress);

                
                await Task.Delay(TimeSpan.FromSeconds(2))
                .ContinueWith((t, _) => generator.generateReport(audit.Id), null,
                    TaskScheduler.FromCurrentSynchronizationContext());

                await Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith((t, _) => HideProgressBar(), null,
                    TaskScheduler.FromCurrentSynchronizationContext());


            
        }

        private void UnHideProgressBar()
        {
            pgrbReport.IsIndeterminate = true;
            pgrbReport.Value = 33;

            dlgHost3.Visibility = Visibility.Visible;
            mbxProgress.Visibility = Visibility.Visible;
        }

        private void HideProgressBar()
        {
            pgrbReport.IsIndeterminate = false;
            pgrbReport.Value = 100;
            
            dlgHost3.IsOpen = false;
            EnqueueThisMessageInSnackbarTwo("Отчёт был создан");
        }





        private void cbxAudits_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            cbxAudits.ItemsSource = Session.CurrentAuditer.Audits.ToList();
        }

        public Report report;
        public float per;
        public float chet = 0;
        public Arc tempArc;

        struct MyDispatch
        {
            public string Name;
            public int Yes;
            public int All;
            public string Color;
        }
        ObservableCollection<MyDispatch> dispatchers;
        public int dispCounter = 0;
        public float tot;
        private void show_Click1()
        {
            per = 0;
            chet = 0;
            List<Category> listCat = Session.AuditContext.Categories.ToList();
            tot = Session.AuditContext.Questions.Count();
            float yes = Session.AuditContext.Answers.Where(a => a.Audit.Id == audit.Id && a.Reply == true).Count();
            Random r = new Random();
            dispatchers = new ObservableCollection<MyDispatch>();
            Report.MyData[] items = new Report.MyData[listCat.Count()];
            int i = 0;
            foreach (Category cat in listCat)
            {
                int totalquestion = Session.AuditContext.Questions.Where(q => q.Category.Id == cat.Id).Count();
                int answeredYes = Session.AuditContext.Answers.Where(a => a.Audit.Id == audit.Id && a.Question.Category.Id == cat.Id && a.Reply == true).Count();
                int answeredNo = Session.AuditContext.Answers.Where(a => a.Audit.Id == audit.Id && a.Question.Category.Id == cat.Id && a.Reply == false).Count();
                items[i] = new Report.MyData { Color = Color.FromArgb(255, (byte)(i < listCat.Count / 2 ? 120 : 120 - (10 * i - 150)), (byte)(i < listCat.Count/2 ? 10 * i: 120), 70).ToString(), 
                    Category = cat.Name, Protection = Math.Round((double)100 * answeredYes / totalquestion, 2) + "%", 
                    Questions = totalquestion, Answered = (answeredNo + answeredYes), Yes = answeredYes, No = answeredNo, NA = (totalquestion - answeredYes - answeredNo) };
                if (items[i].Yes != 0)
                {
                    dispatchers.Add(new MyDispatch {Name = items[i].Category,Yes = (items[i].Yes), All = (items[i].Answered + items[i].NA) , Color = items[i].Color});
                }
                i++;
            }
            report.dtgReport.Items.Clear();
            report.dtgReport.ItemsSource = items;
            report.dtgReport.Items.Refresh();
            tempArc = null;
            if (dispatchers.Count != 0) dispatch();
        }

        private void dispatch()
        {
            Style style = report.FindResource("ArcStyle") as Style;
            per +=(100 * dispatchers[dispCounter].Yes / tot);
            Arc arc = new Arc();
            if (tempArc != null) arc.StartAngle = tempArc.EndAngle;
            else arc.StartAngle = 0;
            tempArc = arc;
            tempArc.Style = style;
            tempArc.ToolTip = dispatchers[dispCounter].Name + " (" + String.Format("{0:0.#}", 100 * dispatchers[dispCounter].Yes / tot) + "% из " +
                String.Format("{0:0.#}", 100 * dispatchers[dispCounter].All / tot) + "%)"; 
            var converter = new BrushConverter();
            tempArc.Stroke = (Brush)converter.ConvertFromString(dispatchers[dispCounter].Color);
            report.MainArc.Children.Add(tempArc);
            DispatcherTimer dispatcher = new DispatcherTimer();
            dispatcher.Tick += dispatcherTimer_Tick;
            dispatcher.Interval = TimeSpan.FromMilliseconds(0);
            dispatcher.Start();
            dispCounter++;
        }
        private void show_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer Counter_Timer1 = new DispatcherTimer();
            report = new Report();
            report.arc.EndAngle = 0;
            report.title.Text = "Отчёт " + audit.Name;
            MainWindow wind = (MainWindow)Window.GetWindow(this);
            wind.GridMain.Children.Clear();
            wind.GridMain.Children.Add(report);
            Counter_Timer1.Interval = TimeSpan.FromMilliseconds(0);
            Counter_Timer1.Tick += dispatcherTimer1_Tick;
            Counter_Timer1.Start();
        }
        

        public void dispatcherTimer1_Tick(object sender, object e)
        {
            if (chet <= 100)
            {
                report.arc.EndAngle = chet*3.6;
                chet += (float)0.1;
            }
            else
            {
                (sender as DispatcherTimer).Stop();
                show_Click1();
            }
        }

        public void dispatcherTimer_Tick(object sender, object e)
        {
            if (chet <= per)
            {
                tempArc.EndAngle = chet * 3.6;
                report.arctext.Text = String.Format("{0:0.#}", chet) + "%"; 
                chet += (float)0.1;
            }
            else
            {
                (sender as DispatcherTimer).Stop();
                if(dispCounter < dispatchers.Count)
                    dispatch();
                else
                {
                    dispCounter = 0;
                    if (per < 20)
                    {
                        report.Shield.Kind = PackIconKind.ShieldRemoveOutline;
                        report.rate.Text = "Неудовлетворительно";
                    }else if(per < 40)
                    {
                        report.Shield.Kind = PackIconKind.ShieldOutline;
                        report.rate.Text = "Слабо";
                    }
                    else if (per < 60)
                    {
                        report.Shield.Kind = PackIconKind.ShieldHalfFull;
                        report.rate.Text = "Удовлетворительно";
                    }
                    else if (per < 80)
                    {
                        report.Shield.Kind = PackIconKind.Security;
                        report.rate.Text = "Хорошо";
                    }
                    else
                    {
                        report.Shield.Kind = PackIconKind.ShieldCheck;
                        report.rate.Text = "Отлично";
                    }
                    rate_TextShow();
                }
            }
        }

        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer1 = new DispatcherTimer();

        public void Tick(object sender, object e)
        {
            if (report.rate.Opacity < 1)
            {
                report.rate.Opacity += 0.1;
            }
            else
            {
                timer.Stop();
                timer1.Start();
            }
        }
        public void Tick1(object sender, object e)
        {
            if (report.Shield.Opacity < 1)
            {
                report.Shield.Opacity += 0.1;
            }
            else
            {
                timer.Stop();
            }
        }

        public void rate_TextShow()
        {
            timer.Tick += Tick;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Start();
            timer1.Tick += Tick1;
            timer1.Interval = TimeSpan.FromMilliseconds(1);

        }

        public void f()
        {
            dlgHost4.IsOpen = false;
        }
        private void ButtonGenerateReport_Click_1(object sender, RoutedEventArgs e)
        {
            if (cbxAudits.SelectedItem == null)
            {
                //MessageBox.Show("Please Choose an audit first", App.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                dlgHost1.ShowDialog(mbx);

                cbxAudits.Focus();
                i++;
            }
            else
            {
                dlgHost4.ShowDialog(que);
                dlgHost4.Visibility = Visibility.Visible;
                que.Visibility = Visibility.Visible;
                Task.Delay(TimeSpan.FromSeconds(3))
                    .ContinueWith((t, _) => f(), null,
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}
