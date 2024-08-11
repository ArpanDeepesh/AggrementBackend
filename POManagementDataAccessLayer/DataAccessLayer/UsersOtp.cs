using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class UsersOtp
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string UserOtp { get; set; } = null!;

    public sbyte UserOtpStatus { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
