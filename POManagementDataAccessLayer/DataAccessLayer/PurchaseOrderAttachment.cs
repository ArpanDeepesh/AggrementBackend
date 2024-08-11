using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PurchaseOrderAttachment
{
    public long Id { get; set; }

    public long? PurchaseOrderId { get; set; }

    public sbyte? AttachmentType { get; set; }

    public string AttachmentLink { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
