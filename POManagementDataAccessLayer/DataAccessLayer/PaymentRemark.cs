using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class PaymentRemark
{
    public long Id { get; set; }

    public long? PaymentId { get; set; }

    public string RemarkTxt { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
