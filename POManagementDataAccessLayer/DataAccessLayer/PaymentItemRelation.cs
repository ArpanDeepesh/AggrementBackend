using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PaymentItemRelation
{
    public long Id { get; set; }

    public long? PaymentId { get; set; }

    public long? LineItemId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
