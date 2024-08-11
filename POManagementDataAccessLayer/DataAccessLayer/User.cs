using System;
using System.Collections.Generic;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class User
{
    public long Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? UserGstin { get; set; }

    public string? UserSmsContact { get; set; }

    public string? UserWatsappContact { get; set; }

    public string? UserEmail { get; set; }

    public bool? UserStatus { get; set; }

    public DateTime CreateOn { get; set; }

    public DateTime ModifiedOn { get; set; }
}
