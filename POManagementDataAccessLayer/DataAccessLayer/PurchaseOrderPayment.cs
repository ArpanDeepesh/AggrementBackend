using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderPayment
{
    public long Id { get; set; }

    public long? PurchaseOrderId { get; set; }

    public int SequenceNo { get; set; }

    public DateTime DueDate { get; set; }

    public sbyte PaymentCompletionCriteria { get; set; }

    public sbyte PaymentStatus { get; set; }

    public decimal? PaymentAmount { get; set; }

    public string? PaymentNotes { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
