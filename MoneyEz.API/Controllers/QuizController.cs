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

        [HttpGet]
        //[Authorize]
        public Task<IActionResult> GetAllQuizzes([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _quizService.GetQuizListAsync(paginationParameter));
        }

        [HttpGet]
        [Route("{id}")]
        //[Authorize]
        public Task<IActionResult> GetQuizById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.GetQuizByIdAsync(id));
        }

        [HttpPost]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> CreateQuiz(CreateQuizModel model)
        {
            return ValidateAndExecute(() => _quizService.CreateQuizAsync(model));
        }

        [HttpPut]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> UpdateQuiz(QuizModel model)
        {
            return ValidateAndExecute(() => _quizService.UpdateQuizAsync(model));
        }

        [HttpDelete]
        [Route("{id}")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> DeleteQuiz([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.DeleteQuizAsync(id));
        }

        [HttpPost]
        [Route("submit")]
        //[Authorize]
        public Task<IActionResult> SubmitQuizAttempt(CreateQuizAttemptModel model)
        {
            return ValidateAndExecute(() => _quizService.SubmitQuizAttemptAsync(model));
        }

        [HttpGet]
        [Route("{id}/questions")]
        //[Authorize]
        public Task<IActionResult> GetQuestionsByQuizId([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.GetQuestionsByQuizIdAsync(id));
        }

        [HttpPost]
        [Route("{id}/questions")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> CreateQuestion([FromRoute] Guid id, CreateQuestionModel model)
        {
            return ValidateAndExecute(() => _quizService.CreateQuestionAsync(id, model));
        }

        [HttpPut]
        [Route("questions/{questionId}")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> UpdateQuestion([FromRoute] Guid questionId, QuestionModel model)
        {
            return ValidateAndExecute(() => _quizService.UpdateQuestionAsync(questionId, model));
        }

        [HttpDelete]
        [Route("questions/{questionId}")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> DeleteQuestion([FromRoute] Guid questionId)
        {
            return ValidateAndExecute(() => _quizService.DeleteQuestionAsync(questionId));
        }

        [HttpPost]
        [Route("questions/{questionId}/answer-options")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> CreateAnswerOption([FromRoute] Guid questionId, CreateAnswerOptionModel model)
        {
            return ValidateAndExecute(() => _quizService.CreateAnswerOptionAsync(questionId, model));
        }

        [HttpPut]
        [Route("answer-options/{answerOptionId}")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> UpdateAnswerOption([FromRoute] Guid answerOptionId, AnswerOptionModel model)
        {
            return ValidateAndExecute(() => _quizService.UpdateAnswerOptionAsync(answerOptionId, model));
        }

        [HttpDelete]
        [Route("answer-options/{answerOptionId}")]
        //[Authorize(Roles = "ADMIN")]
        public Task<IActionResult> DeleteAnswerOption([FromRoute] Guid answerOptionId)
        {
            return ValidateAndExecute(() => _quizService.DeleteAnswerOptionAsync(answerOptionId));
        }

        [HttpGet]
        [Route("user-quiz-results")]
        //[Authorize]
        public Task<IActionResult> GetAllUserQuizResults([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _quizService.GetAllUserQuizResultsAsync(paginationParameter));
        }

        [HttpGet]
        [Route("user-quiz-results/{id}")]
        //[Authorize]
        public Task<IActionResult> GetUserQuizResultById([FromRoute] Guid id)
        {
            return ValidateAndExecute(() => _quizService.GetUserQuizResultByIdAsync(id));
        }

        [HttpGet]
        [Route("user-quiz-results/user")]
        //[Authorize]
        public Task<IActionResult> GetUserQuizResultsByUserId([FromQuery] PaginationParameter paginationParameter)
        {
            return ValidateAndExecute(() => _quizService.GetUserQuizResultsByUserIdAsync(paginationParameter));
        }
    }
}
