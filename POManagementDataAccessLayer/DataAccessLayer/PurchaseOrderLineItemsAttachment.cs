using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderLineItemsAttachment
{
    public long Id { get; set; }

    public long? LineItemId { get; set; }

    public string AttachmentLink { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
