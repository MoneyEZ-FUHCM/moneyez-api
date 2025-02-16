﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class QuizAnswer : BaseEntity
{
    public Guid? QuestionId { get; set; }

    public string AnswerContent { get; set; }

    public int? Weight { get; set; }

    public virtual QuizQuestion Question { get; set; }
}