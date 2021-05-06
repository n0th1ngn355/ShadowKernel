using modelFirst;
using modelFirst.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowKernel.helper
{
    static class Session
    {
        public static Auditer CurrentAuditer { get; set; }
        public static int LoginAttemptCount { get; set; }

        public static AuditContext AuditContext { get; set; }

        public static void Save()
        {
            AuditContext.SaveChanges();
        }
    }
}
