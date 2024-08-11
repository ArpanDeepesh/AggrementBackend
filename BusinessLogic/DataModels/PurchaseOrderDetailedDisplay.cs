using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DataModels
{
    public class PurchaseOrderDetailedDisplay
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
        public string Description { get; set; }
        public decimal NotificationPeriodInDays { get; set; }
        public DateTime ExpectedCompletionDate { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Discount { get; set; }
        public List<LineItemShortDisplay> POLineItems { get; set; }
        public List<TaxDisplay> POTaxes { get; set; }
        public List<TermsAndConditionDisplay> POTermsAndConditions { get; set; }
        public List<POAttachmentsDisplay> POAttachments { get; set; }
        public List<RemarksDisplay> PORemarks { get; set; }
        public List<PaymentShortDisplay> POPayments { get; set; }

    }
    public class TaxDisplay {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Percent { get; set; }
    }
    public class TermsAndConditionDisplay {
        public long Id { get; set; }
        public int Sequence { get; set; }
        public string Description { get; set; }
    }
    public class POAttachmentsDisplay: GeneralAttachments
    {
        public string AttachmentType { get; set; }
    }
    public class GeneralAttachments {
        public long Id { get; set; }
        public string Link { get; set; }
    }
    public class RemarksDisplay {
        public long Id { get; set; }
        public string Description { get; set; }
        public List<GeneralAttachments> Attachments { get; set; }
        public DateTime RemarkDate { get; set; }
    }

}
