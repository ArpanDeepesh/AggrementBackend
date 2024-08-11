using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PaymentRemarkAttachment
{
    public long Id { get; set; }

    public long? RemarkId { get; set; }

    public string AttachmentLink { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
