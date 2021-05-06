using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modelFirst.Model
{
    public class Question
    {
        public int Id { get; set; }
        public String Intitled { get; set; }
        public String Details { get; set; }
        public int Coefficient { get; set; }
        public int Scale { get; set; }
        public String Recommandation { get; set; }
        public String Risk { get; set; }

        public String Answer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;

        public virtual Category Category { get; set; }
        public virtual ICollection<Audit> Audits { get; set; }
        public virtual ObservableCollection<Answer> Answers { get; set; }
    }
}
