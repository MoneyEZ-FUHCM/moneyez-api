// C#
using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;

namespace MoneyEz.Services.Services.Implements
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public QuizService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> CreateQuizAsync(CreateQuizModel createQuizModel)
        {
            if (createQuizModel == null)
            {
                throw new ArgumentException("Quiz model is null.", nameof(createQuizModel));
            }
            if (string.IsNullOrWhiteSpace(createQuizModel.Title))
            {
                throw new ArgumentException("Quiz title is required.", nameof(createQuizModel.Title));
            }

            var quiz = _mapper.Map<Quiz>(createQuizModel);
            await _unitOfWork.QuizRepository.AddAsync(quiz);

            foreach (var createQuestionModel in createQuizModel.Questions)
            {
                var question = _mapper.Map<Question>(createQuestionModel);
                question.QuizId = quiz.Id;
                await _unitOfWork.QuestionRepository.AddAsync(question);

                foreach (var createAnswerOptionModel in createQuestionModel.AnswerOptions)
                {
                    var answerOption = _mapper.Map<AnswerOption>(createAnswerOptionModel);
                    answerOption.QuestionId = question.Id;
                    await _unitOfWork.AnswerOptionRepository.AddAsync(answerOption);
                }
            }

            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = _mapper.Map<QuizModel>(quiz),
                Message = "Quiz created successfully."
            };
        }

        public async Task<BaseResultModel> SubmitQuizAttemptAsync(CreateQuizAttemptModel quizAttemptModel)
        {
            if (quizAttemptModel == null || quizAttemptModel.QuizId == Guid.Empty)
            {
                throw new ArgumentException("Quiz attempt model or QuizId is invalid.");
            }

            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizAttemptModel.QuizId);
            if (quiz == null)
            {
                throw new NotExistException("", "Quiz not found.");
            }

            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail);
            if (user == null)
            {
                throw new NotExistException("", "User not found.");
            }

            var quizResult = new UserQuizResult
            {
                UserId = user.Id,
                QuizId = quizAttemptModel.QuizId,
                TakenAt = CommonUtils.GetCurrentTime(),
                RecommendedModel = string.Empty
            };

            await _unitOfWork.UserQuizResultRepository.AddAsync(quizResult);
            await _unitOfWork.SaveAsync();

            var answers = _mapper.Map<List<UserQuizAnswer>>(quizAttemptModel.Answers);
            foreach (var answer in answers)
            {
                answer.UserQuizResultId = quizResult.Id;
                await _unitOfWork.UserQuizAnswerRepository.AddAsync(answer);
            }
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<UserQuizResultModel>(quizResult),
                Message = "Quiz attempt submitted successfully."
            };
        }

        public async Task<BaseResultModel> GetQuizByIdAsync(Guid quizId)
        {
            if (quizId == Guid.Empty)
            {
                throw new ArgumentException("QuizId is invalid.", nameof(quizId));
            }
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsyncInclude(quizId);
            if (quiz == null)
            {
                throw new NotExistException("", "Quiz not found.");
            }
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(quiz),
                Message = "Quiz retrieved successfully."
            };
        }

        public async Task<BaseResultModel> GetQuizListAsync(PaginationParameter paginationParameter)
        {
            var pagedQuizzes = await _unitOfWork.QuizRepository.GetAllAsyncPagingInclude(paginationParameter);
            var resultData = _mapper.Map<Pagination<QuizModel>>(pagedQuizzes);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = resultData,
                Message = "Quiz list retrieved successfully."
            };
        }

        public async Task<BaseResultModel> UpdateQuizAsync(QuizModel quizModel)
        {
            if (quizModel == null || quizModel.Id == Guid.Empty)
            {
                throw new ArgumentException("Quiz model is null or invalid.", nameof(quizModel));
            }
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizModel.Id);
            if (quiz == null)
            {
                throw new NotExistException("", "Quiz not found.");
            }
            _mapper.Map(quizModel, quiz);
            _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Quiz updated successfully."
            };
        }

        public async Task<BaseResultModel> DeleteQuizAsync(Guid quizId)
        {
            if (quizId == Guid.Empty)
            {
                throw new ArgumentException("QuizId is invalid.", nameof(quizId));
            }
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null)
            {
                throw new NotExistException("", "Quiz not found.");
            }
            _unitOfWork.QuizRepository.SoftDeleteAsync(quiz);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Quiz deleted successfully."
            };
        }

        public async Task<BaseResultModel> GetQuestionsByQuizIdAsync(Guid quizId)
        {
            if (quizId == Guid.Empty)
            {
                throw new ArgumentException("QuizId is invalid.", nameof(quizId));
            }
            var questions = await _unitOfWork.QuestionRepository.GetByQuizIdAsync(quizId);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<List<QuestionModel>>(questions),
                Message = "Questions retrieved successfully."
            };
        }

        public async Task<BaseResultModel> CreateQuestionAsync(Guid quizId, CreateQuestionModel questionModel)
        {
            if (quizId == Guid.Empty)
            {
                throw new ArgumentException("QuizId is invalid.", nameof(quizId));
            }
            if (questionModel == null)
            {
                throw new ArgumentException("Question model is null.", nameof(questionModel));
            }
            if (string.IsNullOrWhiteSpace(questionModel.Content))
            {
                throw new ArgumentException("Question content is required.", nameof(questionModel.Content));
            }

            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizId);
            if (quiz == null)
            {
                throw new NotExistException("", "Quiz not found.");
            }

            var question = _mapper.Map<Question>(questionModel);
            question.QuizId = quizId;
            await _unitOfWork.QuestionRepository.AddAsync(question);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = _mapper.Map<QuestionModel>(question),
                Message = "Question created successfully."
            };
        }

        public async Task<BaseResultModel> UpdateQuestionAsync(Guid questionId, QuestionModel questionModel)
        {
            if (questionId == Guid.Empty)
            {
                throw new ArgumentException("QuestionId is invalid.", nameof(questionId));
            }
            if (questionModel == null)
            {
                throw new ArgumentException("Question model is null.", nameof(questionModel));
            }
            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                throw new NotExistException("", "Question not found.");
            }
            _mapper.Map(questionModel, question);
            _unitOfWork.QuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Question updated successfully."
            };
        }

        public async Task<BaseResultModel> DeleteQuestionAsync(Guid questionId)
        {
            if (questionId == Guid.Empty)
            {
                throw new ArgumentException("QuestionId is invalid.", nameof(questionId));
            }
            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                throw new NotExistException("", "Question not found.");
            }
            _unitOfWork.QuestionRepository.SoftDeleteAsync(question);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Question deleted successfully."
            };
        }

        public async Task<BaseResultModel> CreateAnswerOptionAsync(Guid questionId, CreateAnswerOptionModel answerOptionModel)
        {
            if (questionId == Guid.Empty)
            {
                throw new ArgumentException("QuestionId is invalid.", nameof(questionId));
            }
            if (answerOptionModel == null)
            {
                throw new ArgumentException("Answer option model is null.", nameof(answerOptionModel));
            }
            if (string.IsNullOrWhiteSpace(answerOptionModel.Content))
            {
                throw new ArgumentException("Answer option content is required.", nameof(answerOptionModel.Content));
            }

            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                throw new NotExistException("", "Question not found.");
            }

            var answerOption = _mapper.Map<AnswerOption>(answerOptionModel);
            answerOption.QuestionId = questionId;
            await _unitOfWork.AnswerOptionRepository.AddAsync(answerOption);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Data = _mapper.Map<AnswerOptionModel>(answerOption),
                Message = "Answer option created successfully."
            };
        }

        public async Task<BaseResultModel> UpdateAnswerOptionAsync(Guid answerOptionId, AnswerOptionModel answerOptionModel)
        {
            if (answerOptionId == Guid.Empty)
            {
                throw new ArgumentException("AnswerOptionId is invalid.", nameof(answerOptionId));
            }
            if (answerOptionModel == null)
            {
                throw new ArgumentException("Answer option model is null.", nameof(answerOptionModel));
            }
            var answerOption = await _unitOfWork.AnswerOptionRepository.GetByIdAsync(answerOptionId);
            if (answerOption == null)
            {
                throw new NotExistException("", "Answer option not found.");
            }
            _mapper.Map(answerOptionModel, answerOption);
            _unitOfWork.AnswerOptionRepository.UpdateAsync(answerOption);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Answer option updated successfully."
            };
        }

        public async Task<BaseResultModel> DeleteAnswerOptionAsync(Guid answerOptionId)
        {
            if (answerOptionId == Guid.Empty)
            {
                throw new ArgumentException("AnswerOptionId is invalid.", nameof(answerOptionId));
            }
            var answerOption = await _unitOfWork.AnswerOptionRepository.GetByIdAsync(answerOptionId);
            if (answerOption == null)
            {
                throw new NotExistException("", "Answer option not found.");
            }
            _unitOfWork.AnswerOptionRepository.SoftDeleteAsync(answerOption);
            await _unitOfWork.SaveAsync();
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = "Answer option deleted successfully."
            };
        }

        public async Task<BaseResultModel> GetAllUserQuizResultsAsync(PaginationParameter paginationParameter)
        {
            var pagedUserQuizResults = await _unitOfWork.UserQuizResultRepository.GetAllUserQuizResultsAsync(paginationParameter);
            var resultData = _mapper.Map<Pagination<UserQuizResultModel>>(pagedUserQuizResults);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = resultData,
                Message = "User quiz results retrieved successfully."
            };
        }

        public async Task<BaseResultModel> GetUserQuizResultByIdAsync(Guid id)
        {
            var userQuizResult = await _unitOfWork.UserQuizResultRepository.GetUserQuizResultByIdAsync(id);
            var resultData = _mapper.Map<UserQuizResultModel>(userQuizResult);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = resultData,
                Message = "User quiz results retrieved successfully."
            };
        }

        public async Task<BaseResultModel> GetUserQuizResultsByUserIdAsync(PaginationParameter paginationParameter)
        {
            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;
            var pagedUserQuizResults = await _unitOfWork.UserQuizResultRepository.GetUserQuizResultsByUserIdAsync(userId, paginationParameter);
            var resultData = _mapper.Map<Pagination<UserQuizResultModel>>(pagedUserQuizResults);
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = resultData,
                Message = "User quiz results retrieved successfully."
            };
        }
    }
}
