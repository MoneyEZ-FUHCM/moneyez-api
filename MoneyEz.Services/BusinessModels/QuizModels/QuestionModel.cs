using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateQuestionModel
    {
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string Content { get; set; }

        [MinLength(2, ErrorMessage = "Phải có ít nhất 2 câu trả lời cho câu hỏi")]
        public List<CreateAnswerOptionModel> AnswerOptions { get; set; } = new();
    }
    public class QuestionModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string Content { get; set; }

        [MinLength(2, ErrorMessage = "Phải có ít nhất 2 câu trả lời cho câu hỏi")]
        public List<AnswerOptionModel> AnswerOptions { get; set; } = new List<AnswerOptionModel>();
    }
}
