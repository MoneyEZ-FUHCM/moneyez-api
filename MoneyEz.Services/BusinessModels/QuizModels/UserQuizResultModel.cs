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

        [Required(ErrorMessage = "Điểm số không được để trống")]
        public int Score { get; set; }

        public List<CreateUserQuizAnswerModel> UserQuizAnswers { get; set; } = new();
    }

    public class UserQuizResultModel : BaseEntity
    {
        [Required(ErrorMessage = "User ID không được để trống")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Quiz ID không được để trống")]
        public Guid QuizId { get; set; }

        [Required(ErrorMessage = "Điểm số không được để trống")]
        public int Score { get; set; }

        public List<UserQuizAnswerModel> UserAnswers { get; set; } = new List<UserQuizAnswerModel>();
    }
}
