using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class LineItemRemark
{
    public long Id { get; set; }

    public long? LineItemId { get; set; }

    public string RemarkTxt { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
