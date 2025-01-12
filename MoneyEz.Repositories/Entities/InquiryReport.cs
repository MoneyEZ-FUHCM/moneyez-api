using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class InquiryReport : BaseEntity
{
    public Guid? GroupId { get; set; }

    public int? ReportType { get; set; }

    public string? Content { get; set; }
}
