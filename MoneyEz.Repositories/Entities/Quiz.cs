#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class Quiz : BaseEntity
{
    public Guid? SpendingModelId { get; set; }

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<UserQuizResult> UserQuizResults { get; set; } = new List<UserQuizResult>();
}