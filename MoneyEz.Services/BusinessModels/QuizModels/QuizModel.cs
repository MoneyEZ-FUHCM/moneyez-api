using MoneyEz.Repositories.Entities;
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

        public List<CreateQuestionModel> Questions { get; set; } = new();
    }

    public class QuizModel : BaseEntity
    {
        [Required(ErrorMessage = "Tiêu đề bộ quiz không được để trống")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Chi tiết bộ câu hỏi không được để trống")]
        public string Description { get; set; }

        public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }
}
