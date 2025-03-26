#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Quiz : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UserQuizResult> UserQuizResults { get; set; } = new List<UserQuizResult>();
}
