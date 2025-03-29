using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.QuizModels
{
    public class CreateAnswerOptionModel
    {
        public string Content { get; set; }
        public string? Type { get; set; }
    }

    public class AnswerOptionModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string? Type { get; set; }
    }
}
