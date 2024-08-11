using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderRemark
{
    public long Id { get; set; }

    public string RemarkTxt { get; set; } = null!;

    public long? PurchaseOrderId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
