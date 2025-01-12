using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class UserGroup : BaseEntity
{
    public Guid? UserId { get; set; }

    public Guid? GroupId { get; set; }

    public int? RoleGroup { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? User { get; set; }
}
