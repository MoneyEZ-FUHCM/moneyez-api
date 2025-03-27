#nullable disable
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class AnswerOption : BaseEntity
{
    public Guid QuestionId { get; set; }

    public string Content { get; set; }

    public AnswerOptionType? Type { get; set; }

    public virtual Question Question { get; set; }

    public virtual ICollection<UserQuizAnswer> UserQuizAnswers { get; set; } = new List<UserQuizAnswer>();
}
