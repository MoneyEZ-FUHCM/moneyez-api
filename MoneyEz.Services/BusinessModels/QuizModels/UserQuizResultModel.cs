using MoneyEz.Repositories.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateUserQuizResultModel
    {
        [Required(ErrorMessage = "User ID không được để trống")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Quiz ID không được để trống")]
        public Guid QuizId { get; set; }

        public List<CreateUserQuizAnswerModel> UserQuizAnswers { get; set; } = new();
    }

    public class UserQuizResultModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public string? QuizVersion { get; set; }
        public DateTime TakenAt { get; set; }
        public RecomendModelResponse? RecommendedModel { get; set; }
        public List<UserAnswerModel> Answers { get; set; } = new List<UserAnswerModel>();
    }

    public class RecomendModelResponse
    {
        public RecommendModel? RecommendedModel { get; set; }

        public List<RecommendModel> AlternativeModels { get; set; } = new List<RecommendModel>();

        public string? Reasoning { get; set; }
    }

    public class RecommendModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
