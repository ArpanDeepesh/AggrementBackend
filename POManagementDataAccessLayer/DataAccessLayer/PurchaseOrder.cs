using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrder
{
    public long Id { get; set; }

    public long? PoRaisedBy { get; set; }

    public long? PoRaisedFor { get; set; }

    public sbyte? PoStatus { get; set; }

    public string PoTitle { get; set; } = null!;

    public string PoDescription { get; set; } = null!;

    public decimal PoNotificationPeriodInDays { get; set; }

    public int PoDurationInDays { get; set; }

    public DateTime PoStartDate { get; set; }

    public decimal PoTotalAmount { get; set; }

    public decimal PoDiscount { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
