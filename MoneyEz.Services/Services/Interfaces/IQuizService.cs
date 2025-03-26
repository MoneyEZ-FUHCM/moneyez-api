using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ChatModels;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IQuizService
    {
        Task<BaseResultModel> CreateQuizAsync(CreateQuizModel createQuizModel);
        Task<BaseResultModel> SubmitQuizAttemptAsync(CreateQuizAttemptModel quizAttemptModel);
        Task<BaseResultModel> GetQuizByIdAsync(Guid quizId);
        Task<BaseResultModel> GetQuizListAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> UpdateQuizAsync(QuizModel quizModel);
        Task<BaseResultModel> DeleteQuizAsync(Guid quizId);
        Task<BaseResultModel> GetQuestionsByQuizIdAsync(Guid quizId);
        Task<BaseResultModel> CreateQuestionAsync(Guid quizId, CreateQuestionModel questionModel);
        Task<BaseResultModel> UpdateQuestionAsync(Guid questionId, QuestionModel questionModel);
        Task<BaseResultModel> DeleteQuestionAsync(Guid questionId);
        Task<BaseResultModel> CreateAnswerOptionAsync(Guid questionId, CreateAnswerOptionModel answerOptionModel);
        Task<BaseResultModel> UpdateAnswerOptionAsync(Guid answerOptionId, AnswerOptionModel answerOptionModel);
        Task<BaseResultModel> DeleteAnswerOptionAsync(Guid answerOptionId);
        Task<BaseResultModel> GetAllUserQuizResultsAsync(PaginationParameter paginationParameter);
        Task<BaseResultModel> GetUserQuizResultByIdAsync(Guid id);
        Task<BaseResultModel> GetUserQuizResultsByUserIdAsync(PaginationParameter paginationParameter);
    }
}
