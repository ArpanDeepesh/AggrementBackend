using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class AppSettings
    {
        public string ValidClientKey { get; set; }
        public string AppSecret { get; set; }
        public int TokenExpiryInHours { get; set; }
    }
}
