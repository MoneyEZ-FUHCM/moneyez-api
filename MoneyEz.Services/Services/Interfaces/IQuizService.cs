using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IQuizService
    {
        // Admin functions
        Task<BaseResultModel> CreateQuizAsync(CreateQuizModel createQuizModel);
        Task<BaseResultModel> GetQuizByIdAsync(Guid id);
        Task<BaseResultModel> GetAllQuizzesAsync(PaginationParameter paginationParameter, FilterBase filter);
        Task<BaseResultModel> UpdateQuizAsync(UpdateQuizModel quizModel);
        Task<BaseResultModel> ActivateQuizAsync(Guid id);
        Task<BaseResultModel> DeleteQuizAsync(Guid id);

        // User functions
        Task<BaseResultModel> GetActiveQuizAsync();
        Task<BaseResultModel> SubmitQuizAnswersAsync(CreateQuizAttemptModel quizAttempt);
        Task<BaseResultModel> GetUserQuizHistoryAsync(PaginationParameter paginationParameter);
    }
}
