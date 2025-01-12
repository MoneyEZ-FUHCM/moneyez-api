using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Image : BaseEntity
{
    public Guid? EntityId { get; set; }

    public string? EntityName { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Post? Entity { get; set; }

    public virtual Transaction? EntityNavigation { get; set; }
}
