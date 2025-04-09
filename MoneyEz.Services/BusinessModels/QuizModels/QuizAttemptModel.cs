using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateQuizAttemptModel
    {
        [Required(ErrorMessage = "Quiz ID không được để trống")]
        public Guid QuizId { get; set; }

        [Required(ErrorMessage = "Câu trả lời không được để trống")]
        public List<UserAnswerModel> Answers { get; set; } = new();
    }
    
    public class UserAnswerModel
    {
        public Guid? QuestionId { get; set; }
        
        public Guid? AnswerOptionId { get; set; }
        
        [Required(ErrorMessage = "Nội dung câu trả lời không được để trống")]
        public string AnswerContent { get; set; }
    }
    
    public class QuizAttemptModel
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public string QuizVersion { get; set; }
        public List<UserAnswerModel> Answers { get; set; } = new();
    }
}
