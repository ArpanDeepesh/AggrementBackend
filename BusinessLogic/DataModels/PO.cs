using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public enum PurchaseOrderItemAndPaymentStatus
    {
        Draft=0,
        Active=1,
        Waiting=2,
        Completed=3,
        Expired=4,
        Declined=5,
        Raised=6,
        Claimed=7
    }
    public class PO
    {
        public long Id { get; set; }
        public long PoRaisedBy { get; set; }

        public long PoRaisedFor { get; set; }

        public string PoTitle { get; set; } = null!;

        public string PoDescription { get; set; } = null!;

        public decimal PoNotificationPeriod { get; set; }

        public int PoCompletionDurationInDays { get; set; }
        public DateTime PoExpectedStartDate { get; set; }

        public decimal PoTotalAmount { get; set; }

        public decimal PoDiscount { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}
