using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderTaxis
{
    public long Id { get; set; }

    public long? PurchaseOrderId { get; set; }

    public string TaxTitle { get; set; } = null!;

    public decimal TaxPercentage { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
