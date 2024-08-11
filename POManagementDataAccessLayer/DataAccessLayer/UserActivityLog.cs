using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class UserActivityLog
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? OpId { get; set; }

    public sbyte? ActionType { get; set; }

    public string? ActionDescription { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
