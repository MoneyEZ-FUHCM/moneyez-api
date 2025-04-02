#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Question : BaseEntity
{
    public Guid QuizId { get; set; }

    public string Content { get; set; }

    public virtual Quiz Quiz { get; set; }

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
