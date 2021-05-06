using modelFirst.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace modelFirst
{
    class Program
    {
        static void Main(string[] args)
        {
            using ( var auditContext = new AuditContext())
            {
                Thread.Sleep(0);
                List<Audit> audits = auditContext.Audits.Include("Auditer").Include("Questions.Answer").ToList();
                foreach (var audit in audits)//.Include("Auditer").Include("Questions.Answer"))
                {
                    //auditContext.Entry(audit).Reference(a => a.Auditer).Load();
                    //Console.WriteLine(audit.Auditer.Email+" "+audit.Auditer.Password + " "+ BCrypt.Net.BCrypt.Verify("123456789",audit.Auditer.Password, false, BCrypt.Net.HashType.SHA256));
                    //auditContext.Entry(audit).Collection(a => a.Questions).Load();
                    List<Question> questions = auditContext.Questions.ToList();

                    foreach (var item in auditContext.Questions.ToList())
                    {
                        //auditContext.Entry(item).Reference(a => a.Answer).Load();
                        Console.WriteLine(item.Details);
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
