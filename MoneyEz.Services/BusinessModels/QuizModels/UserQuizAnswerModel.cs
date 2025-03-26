using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateUserQuizAnswerModel
    {
        public Guid? AnswerOptionId { get; set; }
        public string AnswerContent { get; set; }
    }

    public class UserQuizAnswerModel : BaseEntity
    {

        public Guid? AnswerOptionId { get; set; }
        public string AnswerContent { get; set; }
    }
}
