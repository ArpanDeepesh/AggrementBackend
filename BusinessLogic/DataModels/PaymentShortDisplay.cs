using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class PaymentShortDisplay
    {
        public long PaymentId { get; set; }
        public int SequenceNo { get; set; }

        public DateTime DueDate { get; set; }

        public List<string> LineItemsRelation { get; set; }

        public string PaymentStatus { get; set; }

        public decimal PaymentAmount { get; set; }

        public string PaymentNotes { get; set; }
        public List<RemarksDisplay> Remarks { get; set; }

    }
}
