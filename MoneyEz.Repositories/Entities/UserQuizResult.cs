using System;
using System.Collections.Generic;
using System.Text.Json;

namespace MoneyEz.Repositories.Entities;

public partial class UserQuizResult : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid? QuizId { get; set; }

    public string? RecommendedModel { get; set; }

    public DateTime TakenAt { get; set; }

    public string? QuizVersion { get; set; } = "";
    
    public string? AnswersJson { get; set; } = "";

    public virtual Quiz? Quiz { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<UserQuizAnswer> UserQuizAnswers { get; set; } = new List<UserQuizAnswer>();
    
    public void SetAnswers(List<UserAnswer> answers)
    {
        AnswersJson = JsonSerializer.Serialize(answers);
    }

    public List<UserAnswer> GetAnswers()
    {
        if (string.IsNullOrEmpty(AnswersJson))
            return new List<UserAnswer>();
        
        return JsonSerializer.Deserialize<List<UserAnswer>>(AnswersJson) ?? new List<UserAnswer>();
    }
}

// Inner class for JSON serialization (not mapped to database)
public class UserAnswer
{
    public Guid QuestionId { get; set; }
    public Guid? AnswerOptionId { get; set; }
    public string AnswerContent { get; set; }
}
