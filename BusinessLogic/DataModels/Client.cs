using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class Client
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string GSTIN { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string WatsappNumber { get; set; }
        public string Status { get; set; }
    }
}
