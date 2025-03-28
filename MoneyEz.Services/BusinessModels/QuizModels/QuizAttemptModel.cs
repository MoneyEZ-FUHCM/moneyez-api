using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateQuizAttemptModel
    {
        public Guid QuizId { get; set; }
        public List<CreateUserQuizAnswerModel> Answers { get; set; } = new();
    }
    public class QuizAttemptModel
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public List<UserQuizAnswerModel> Answers { get; set; } = new();
    }
}
