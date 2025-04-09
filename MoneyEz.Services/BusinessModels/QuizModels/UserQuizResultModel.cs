using MoneyEz.Repositories.Entities;
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
        public string QuizVersion { get; set; }
        public DateTime TakenAt { get; set; }
        public string RecommendedModel { get; set; }
        public List<UserAnswerModel> Answers { get; set; } = new List<UserAnswerModel>();
    }
}
