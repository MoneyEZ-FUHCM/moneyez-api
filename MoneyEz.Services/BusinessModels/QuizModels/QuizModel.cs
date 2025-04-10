using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateQuizModel
    {
        [Required(ErrorMessage = "Tiêu đề bộ quiz không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Chi tiết bộ câu hỏi không được để trống")]
        public string Description { get; set; }

        public CommonsStatus Status { get; set; } = CommonsStatus.INACTIVE;

        public List<QuizQuestionModel> Questions { get; set; } = new();
    }

    public class UpdateQuizModel 
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bộ quiz không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Chi tiết bộ câu hỏi không được để trống")]
        public string Description { get; set; }

        //public CommonsStatus? Status { get; set; }

        public List<QuizQuestionModel> Questions { get; set; } = new();
    }

    public class QuizModel : BaseEntity
    {
        [Required(ErrorMessage = "Tiêu đề bộ quiz không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Chi tiết bộ câu hỏi không được để trống")]
        public string Description { get; set; }

        public CommonsStatus Status { get; set; }
        
        public string Version { get; set; }

        public List<QuizQuestionModel> Questions { get; set; } = new();
    }

    public class QuizQuestionModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string Content { get; set; }
        
        [MinLength(2, ErrorMessage = "Phải có ít nhất 2 câu trả lời cho câu hỏi")]
        public List<QuizAnswerOptionModel> AnswerOptions { get; set; } = new();
    }

    public class QuizAnswerOptionModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
    }
    public class QuestionAnswerPair
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

}
