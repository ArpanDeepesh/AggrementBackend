using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class OrderPaymentPreApproval
    {
        public decimal Amount { get; set; }
        public string Notes { get; set; } 
        public char Criteria { get; set; }
        public long PoId { get; set; }
        public int Sequence { get; set; }
        public string ExtraInfo { get; set; }
    }
}
