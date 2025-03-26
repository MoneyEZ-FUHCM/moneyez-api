#nullable disable
using System;

namespace MoneyEz.Repositories.Entities;

public partial class UserQuizAnswer : BaseEntity
{
    public Guid? AnswerOptionId { get; set; }

    public Guid? UserQuizResultId { get; set; }

    public string AnswerContent { get; set; }

    public virtual AnswerOption AnswerOption { get; set; }

    public virtual UserQuizResult UserQuizResult { get; set; }
}
