﻿#nullable disable
using System;
using System.Collections.Generic;

namespace MoneyEz.Repositories.Entities;

public partial class UserQuizResult : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? QuizId { get; set; }

    public string QuizData { get; set; }

    public virtual Quiz Quiz { get; set; }

    public virtual User User { get; set; }
}