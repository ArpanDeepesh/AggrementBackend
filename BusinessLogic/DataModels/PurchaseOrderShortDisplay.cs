using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class PurchaseOrderShortDisplay
    {
        public long PoId { get; set; }
        public long RaisedById { get; set; }
        public string RaisedByName { get; set; }
        public string RaisedByPhoneNo { get; set; }
        public long RaisedForId { get; set; }
        public string RaisedForName { get; set; }
        public string RaisedForPhoneNo { get; set; }
        public decimal POAmount { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public List<string> DelaysAndWaitingResponse { get; set; }
        public List<PayStatusDisplay> PaymentStatus { get; set; }
        public List<PayStatusDisplay> WorkDoneStatus { get; set; }
    }
    public class PayStatusDisplay {
        public decimal Pay { get; set; }
        public PurchaseOrderItemAndPaymentStatus Status { get; set; }
    }
}
