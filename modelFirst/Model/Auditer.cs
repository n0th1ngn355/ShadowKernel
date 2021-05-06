using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace modelFirst.Model
{
    public class Auditer
    {
        public int Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Login { get; set; }
        public String Password { get; set; }
        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public string UpdateAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        public virtual ObservableCollection<Audit> Audits { get; set; }
    }
}
