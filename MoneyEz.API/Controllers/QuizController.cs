using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/quiz")]
    [ApiController]
    public class QuizController : BaseController
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        #region Quiz Endpoints

        [HttpGet]
        [Authorize]
        public Task<IActionResult> GetAllQuizzes([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _quizService.GetAllQuizzesAsync(paginationParameter));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public Task<IActionResult> GetQuizById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.GetQuizByIdAsync(id));
        }

        [HttpGet]
        [Route("active")]
        [Authorize]
        public Task<IActionResult> GetActiveQuiz()
        {
            return ValidateAndExecute(() => _quizService.GetActiveQuizAsync());
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> CreateQuiz(CreateQuizModel model)
        {
            return ValidateAndExecute(() => _quizService.CreateQuizAsync(model));
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> UpdateQuiz(UpdateQuizModel quizModel)
        {
            return ValidateAndExecute(() => _quizService.UpdateQuizAsync(quizModel));
        }
        
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> DeleteQuiz([FromRoute]Guid id)
        {
            return ValidateAndExecute(() => _quizService.DeleteQuizAsync(id));
        }

        [HttpPost]
        [Route("{id}/activate")]
        [Authorize(Roles = "ADMIN")]
        public Task<IActionResult> SetActiveQuiz([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.ActivateQuizAsync(id));
        }

        #endregion

        #region Quiz Attempt Endpoints

        [HttpPost]
        [Route("submit")]
        [Authorize]
        public Task<IActionResult> SubmitQuizAttempt(CreateQuizAttemptModel model)
        {
            return ValidateAndExecute(() => _quizService.SubmitQuizAnswersAsync(model));
        }

        #endregion

        #region User Quiz Results Endpoints

        [HttpGet]
        [Route("user-quiz-results")]
        [Authorize]
        public Task<IActionResult> GetAllUserQuizResults([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _quizService.GetUserQuizHistoryAsync(paginationParameter));
        }

        #endregion
    }
}
