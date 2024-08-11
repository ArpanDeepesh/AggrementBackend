using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseTermsAndCondition
{
    public long Id { get; set; }

    public long? PurchaseOrderId { get; set; }

    public int SequenceNo { get; set; }

    public string? TermsAndConditionsValue { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
