#nullable disable
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MoneyEz.Repositories.Entities;

public partial class Quiz : BaseEntity
{
    public string Title { get; set; }

    public string Description { get; set; }

    public CommonsStatus Status { get; set; }

    public string QuestionsJson { get; set; } // Store questions and answers as JSON

    public string Version { get; set; } // For tracking quiz versions

    public virtual ICollection<UserQuizResult> UserQuizResults { get; set; } = new List<UserQuizResult>();

    // Helper methods for JSON serialization/deserialization
    public void SetQuestions(List<QuizQuestion> questions)
    {
        QuestionsJson = JsonSerializer.Serialize(questions);
    }

    public List<QuizQuestion> GetQuestions()
    {
        if (string.IsNullOrEmpty(QuestionsJson))
            return new List<QuizQuestion>();
        
        return JsonSerializer.Deserialize<List<QuizQuestion>>(QuestionsJson) ?? new List<QuizQuestion>();
    }
}

// Inner classes for JSON serialization (not mapped to database)
public class QuizQuestion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
    public List<QuizAnswerOption> AnswerOptions { get; set; } = new List<QuizAnswerOption>();
}

public class QuizAnswerOption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; }
}
