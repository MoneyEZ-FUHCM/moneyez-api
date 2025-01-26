#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class QuizQuestion : BaseEntity
{
    public Guid? QuizId { get; set; }

    public string QuizTitle { get; set; }

    public string QuizTitleUnsign { get; set; }

    public virtual Quiz Quiz { get; set; }

    public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
}