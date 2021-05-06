using modelFirst.Model;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modelFirst
{
    public class AuditDBInitializer : SqliteDropCreateDatabaseAlways<AuditContext>
    {
        public AuditDBInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void Seed(AuditContext context)
        {
            base.Seed(context);
            int numberOfObject = 9;
            int numberOfSubObject = 8;
            Random random = new Random();

            for (int i = 1; i <= numberOfObject; i++)
            {
                #region
                List<Category> categories = new List<Category>();
                for (int j = 1; j <= numberOfSubObject; j++)
                {
                    int k = (i - 1) * 10 + j;
                    Category category = new Category()
                    {
                        Id = k,
                        Name = "Category " + k,
                        CreatedAt = DateTime.Now,
                        UpdateAt = DateTime.Now
                    };
                    categories.Add(category);
                }
                context.Categories.AddRange(categories);
                #endregion

                #region
                Auditer auditer = new Auditer()
                {
                    Id = i,
                    FirstName = "Даниил" + i,
                    LastName = "Кирьяков " + i,
                    Login = "Даниил" + i ,
                    Password = BCrypt.Net.BCrypt.HashPassword("JTZgxPY9Dt2C", BCrypt.Net.BCrypt.GenerateSalt(), false, BCrypt.Net.HashType.SHA256),
                    CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    UpdateAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")

            };
                
                context.Auditers.Add(auditer);
                #endregion

                #region
                Audit audit = new Audit()
                {
                    Id = i,
                    Name = "Audit " + i,
                    AuditedCompanyName = "Company " + i,
                    CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    UpdateAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                };
                audit.Auditer = auditer;
                //auditer.Audits.Add(audit);
                context.Audits.Add(audit);
                #endregion


                #region
                List<Answer> answers = new List<Answer>();
                for (int j = 1; j <= numberOfSubObject; j++)
                {
                    int k = (i - 1) * 10 + j;
                    Answer answer = new Answer()
                    {
                        Id = k,
                        Score = k,
                        RecommandationToApply = "Recommandation To Apply" + k,
                        RiskIncurred = "Risk Incurred",
                        Comment = "Comment " + k,
                        FaillureNumber = k,
                        CreatedAt = DateTime.Now,
                        UpdateAt = DateTime.Now
                    };
                    answer.Audit = audit;
                    answers.Add(answer);
                    context.Answers.Add(answer);
                }
                
                #endregion

                #region

                List<Question> questions = new List<Question>();
                for (int j = 1; j <= numberOfSubObject; j++)
                {
                    int k = (i - 1) * 10 + j;
                    Question question = new Question()
                    {
                        Id = k,
                        Intitled = "Intitled " + k,
                        Details = "Details " + k,
                        Coefficient = random.Next(1, 5),
                        Scale = 10,
                        Recommandation = "Recommandation" + k,
                        Risk = "Risk " + k,
                        CreatedAt = DateTime.Now,
                        UpdateAt = DateTime.Now,
                    };
                    
                    question.Category = categories.ElementAt(j-1);
                    question.Answers = new ObservableCollection<Answer>();
                    question.Answers.Add(answers.ElementAt(j-1));
                    questions.Add(question);
                }
                audit.Questions = new ObservableCollection<Question>(questions);
                //category.Questions = questions;

                #endregion

            }
            context.SaveChanges();
        }
    }
}
