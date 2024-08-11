using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderLineItem
{
    public long Id { get; set; }

    public long? PurchaseOrderId { get; set; }

    public string LiTitle { get; set; } = null!;

    public string? LiDescription { get; set; }

    public decimal LiQuantity { get; set; }

    public decimal? LiRate { get; set; }

    public int LiItemCompletionDuration { get; set; }

    public sbyte? LineItemStatus { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
