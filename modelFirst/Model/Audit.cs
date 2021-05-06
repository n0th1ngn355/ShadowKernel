using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modelFirst.Model
{
    public class Audit
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String AuditedCompanyName { get; set; }

        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public string UpdateAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        public virtual Auditer Auditer { get; set; }
        public virtual ObservableCollection<Question> Questions { get; set; }
    }
}
