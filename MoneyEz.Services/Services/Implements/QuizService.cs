﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Repositories.Repositories.Interfaces;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Repositories.Utils;
using MoneyEz.Services.BusinessModels.QuizModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;
        private readonly IExternalApiService _externalApiService;

        public QuizService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService, IExternalApiService externalApiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
            _externalApiService = externalApiService;
        }

        // ADMIN FUNCTIONS

        public async Task<BaseResultModel> CreateQuizAsync(CreateQuizModel createQuizModel)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var quiz = _mapper.Map<Quiz>(createQuizModel);
            quiz.Version = CommonUtils.GetCurrentTime().ToString("yyyyMMddHHmm");
            quiz.CreatedBy = user.Email;


            if (createQuizModel.Status == CommonsStatus.ACTIVE)
            {
                var allQuizzes = await _unitOfWork.QuizRepository.GetAllAsync();
                foreach (var quiz1 in allQuizzes)
                {
                    quiz1.Status = CommonsStatus.INACTIVE;
                    quiz1.UpdatedBy = user.Email;
                    _unitOfWork.QuizRepository.UpdateAsync(quiz1);
                }
            }

            // Create a new quiz with versioning
            var createdQuiz = await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(createdQuiz),
                Message = "Tạo bộ câu hỏi thành công"
            };
        }

        public async Task<BaseResultModel> GetQuizByIdAsync(Guid id)
        {
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(id);
            if (quiz == null)
                throw new NotExistException($"Không tìm thấy bộ câu hỏi với ID: {id}");

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(quiz),
                Message = "Lấy bộ câu hỏi thành công"
            };
        }

        public async Task<BaseResultModel> GetAllQuizzesAsync(PaginationParameter paginationParameter, FilterBase filter)
        {
            var quizzesPagination = await _unitOfWork.QuizRepository.GetAllQuizzesPaginatedAsync(paginationParameter, filter);
            var quizModels = _mapper.Map<List<QuizModel>>(quizzesPagination);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = PaginationHelper.GetPaginationResult(quizzesPagination, quizModels),
                Message = "Lấy tất cả bộ câu hỏi thành công"
            };
        }

        public async Task<BaseResultModel> UpdateQuizAsync(UpdateQuizModel quizModel)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var existingQuiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizModel.Id);
            if (existingQuiz == null)
                throw new NotExistException($"Không tìm thấy bộ câu hỏi với ID: {quizModel.Id}");

            _mapper.Map(quizModel, existingQuiz);
            existingQuiz.Version = CommonUtils.GetCurrentTime().ToString("yyyyMMddHHmm");
            existingQuiz.UpdatedBy = user.Email;

            //if (quizModel.Status != null)
            //{
            //    existingQuiz.Status = quizModel.Status.Value;
            //}

            _unitOfWork.QuizRepository.UpdateAsync(existingQuiz);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(existingQuiz),
                Message = "Cập nhật bộ câu hỏi thành công"
            };
        }

        public async Task<BaseResultModel> DeleteQuizAsync(Guid id)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var existingQuiz = await _unitOfWork.QuizRepository.GetByIdAsync(id);
            if (existingQuiz == null)
                throw new NotExistException($"Không tìm thấy bộ câu hỏi với ID: {id}");

            if (existingQuiz.Status == CommonsStatus.ACTIVE)
            {
                throw new DefaultException("Không thể xóa bộ câu hỏi đang hoạt động");
            }

            existingQuiz.UpdatedBy = user.Email;
            _unitOfWork.QuizRepository.SoftDeleteAsync(existingQuiz);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(existingQuiz),
                Message = "Cập nhật bộ câu hỏi thành công"
            };
        }

        public async Task<BaseResultModel> ActivateQuizAsync(Guid id)
        {
            var user = await _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail)
                ?? throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);

            var allQuizzes = await _unitOfWork.QuizRepository.GetAllAsync();
            foreach (var quiz in allQuizzes)
            {
                quiz.Status = CommonsStatus.INACTIVE;
                quiz.UpdatedBy = user.Email;
                _unitOfWork.QuizRepository.UpdateAsync(quiz);
            }

            var quizToActivate = await _unitOfWork.QuizRepository.GetByIdAsync(id);
            if (quizToActivate == null)
                throw new NotExistException($"Không tìm thấy bộ câu hỏi với ID: {id}");

            quizToActivate.Status = CommonsStatus.ACTIVE;
            quizToActivate.UpdatedBy = user.Email;
            _unitOfWork.QuizRepository.UpdateAsync(quizToActivate);
            await _unitOfWork.SaveAsync();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = true,
                Message = "Kích hoạt bộ câu hỏi thành công"
            };
        }

        // USER FUNCTIONS

        public async Task<BaseResultModel> GetActiveQuizAsync()
        {
            var quiz = await _unitOfWork.QuizRepository.GetActiveQuizAsync();
            if (quiz == null)
                throw new NotExistException("Không có bộ câu hỏi nào đang hoạt động");

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = _mapper.Map<QuizModel>(quiz),
                Message = "Lấy bộ câu hỏi đang hoạt động thành công"
            };
        }

        public async Task<BaseResultModel> SubmitQuizAnswersAsync(CreateQuizAttemptModel quizAttempt)
        {
            var quiz = await _unitOfWork.QuizRepository.GetByIdAsync(quizAttempt.QuizId);
            if (quiz == null)
                throw new NotExistException($"Không tìm thấy bộ câu hỏi với ID: {quizAttempt.QuizId}");

            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var quizQuestions = quiz.GetQuestions();

            ValidateQuizAnswers(quizAttempt.Answers, quizQuestions);

            var userQuizResult = new UserQuizResult
            {
                UserId = userId,
                QuizId = quiz.Id,
                TakenAt = CommonUtils.GetCurrentTime(),
                QuizVersion = quiz.Version,
                CreatedBy = user.Email,
            };

            userQuizResult.SetAnswers(quizAttempt.Answers.Select(a => new UserAnswer
            {
                QuestionId = a.QuestionId ?? Guid.Empty,
                AnswerOptionId = a.AnswerOptionId,
                AnswerContent = a.AnswerContent
            }).ToList());

            // Calculate recommended spending model based on answers
            var summarizeQuizAnswers = SummarizeQuizAnswers(quiz, quizAttempt.Answers);
            var responseSuggest = await _externalApiService.SuggestionSpendingModelSerivce(summarizeQuizAnswers);
            if (responseSuggest != null)
            {
                userQuizResult.RecommendedModel = JsonConvert.SerializeObject(responseSuggest).ToString();
            }
            else
            {
                // If no recommendation is received, set it to an empty string or null
                userQuizResult.RecommendedModel = null;
            }

            // Save the result
            var savedResult = await _unitOfWork.UserQuizResultRepository.AddAsync(userQuizResult);
            await _unitOfWork.SaveAsync();

            // show result
            var quizResult = _mapper.Map<UserQuizResultModel>(savedResult);

            // Call external API to get recommended model
            if (responseSuggest != null)
            {
                quizResult.RecommendedModel = responseSuggest;
            }

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = quizResult,
                Message = "Nộp câu trả lời thành công"
            };
        }

        private void ValidateQuizAnswers(List<UserAnswerModel> answers, List<QuizQuestion> quizQuestions)
        {
            if (answers.Count < quizQuestions.Count)
            {
                throw new DefaultException($"Phải trả lời tất cả {quizQuestions.Count} câu hỏi, hiện tại chỉ có {answers.Count} câu trả lời");
            }

            var answeredQuestionIds = new HashSet<Guid>();

            foreach (var answer in answers)
            {
                if (!answer.QuestionId.HasValue)
                    continue;

                var matchingQuestion = quizQuestions.FirstOrDefault(q => q.Id == answer.QuestionId);

                if (matchingQuestion == null)
                {
                    throw new DefaultException($"Câu hỏi với ID {answer.QuestionId} không tồn tại trong bộ câu hỏi");
                }

                if (answer.AnswerOptionId.HasValue)
                {
                    var matchingOption = matchingQuestion.AnswerOptions.FirstOrDefault(o => o.Id == answer.AnswerOptionId);
                    if (matchingOption == null)
                    {
                        throw new DefaultException($"Lựa chọn với ID {answer.AnswerOptionId} không tồn tại trong câu hỏi {matchingQuestion.Content}");
                    }
                }

                answeredQuestionIds.Add(matchingQuestion.Id);
            }

            var unansweredQuestions = quizQuestions.Where(q => !answeredQuestionIds.Contains(q.Id)).ToList();
            if (unansweredQuestions.Any())
            {
                var missingQuestions = string.Join(", ", unansweredQuestions.Select(q => q.Content));
                throw new DefaultException($"Thiếu câu trả lời cho các câu hỏi sau: {missingQuestions}");
            }

            // Ensure all answers have content
            var emptyAnswers = answers.Where(a => string.IsNullOrWhiteSpace(a.AnswerContent)).ToList();
            if (emptyAnswers.Any())
            {
                throw new DefaultException("Tất cả các câu trả lời phải có nội dung");
            }
        }

        public async Task<BaseResultModel> GetUserQuizHistoryAsync(PaginationParameter paginationParameter)
        {
            var userId = _unitOfWork.UsersRepository.GetUserByEmailAsync(_claimsService.GetCurrentUserEmail).Result.Id;
            var user = await _unitOfWork.UsersRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotExistException(MessageConstants.ACCOUNT_NOT_EXIST);
            }

            var userQuizResultsPagination = await _unitOfWork.UserQuizResultRepository.GetUserQuizResultsByUserIdPaginatedAsync(userId, paginationParameter);
            var userQuizResultModels = _mapper.Map<List<UserQuizResultModel>>(userQuizResultsPagination);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = PaginationHelper.GetPaginationResult(userQuizResultsPagination, userQuizResultModels),
                Message = "Lấy lịch sử làm bài thành công"
            };
        }

        private List<QuestionAnswerPair> SummarizeQuizAnswers(Quiz quiz, List<UserAnswerModel> answers)
        {
            var questions = quiz.GetQuestions();

            var questionDict = questions.ToDictionary(q => q.Id, q => q.Content);

            var answerOptionsDict = new Dictionary<Guid, string>();
            foreach (var question in questions)
            {
                foreach (var option in question.AnswerOptions)
                {
                    answerOptionsDict[option.Id] = option.Content;
                }
            }

            var questionAnswerPairs = new List<QuestionAnswerPair>();

            foreach (var answer in answers)
            {
                if (answer.QuestionId == null || !questionDict.ContainsKey(answer.QuestionId.Value))
                    continue;

                string question = questionDict[answer.QuestionId.Value];

                string answerContent = answer.AnswerContent;
                if (string.IsNullOrEmpty(answerContent) && answer.AnswerOptionId.HasValue &&
                    answerOptionsDict.ContainsKey(answer.AnswerOptionId.Value))
                {
                    answerContent = answerOptionsDict[answer.AnswerOptionId.Value];
                }

                questionAnswerPairs.Add(new QuestionAnswerPair
                {
                    Question = question,
                    Answer = answerContent
                });
            }
            return questionAnswerPairs;
        }

        public async Task<BaseResultModel> GetUserQuizHistoryByIdAsync(Guid resultId)
        {
            var result = await _unitOfWork.UserQuizResultRepository.GetByIdAsync(resultId);
            if (result == null)
                throw new NotExistException($"Not found user quiz result id: {resultId}", MessageConstants.QUIZ_RESULT_NOT_FOUND);

            var quizResult = _mapper.Map<UserQuizResultModel>(result);
            var responseSuggest = JsonConvert.DeserializeObject<RecomendModelResponse>(result.RecommendedModel);

            if (responseSuggest != null)
            {
                quizResult.RecommendedModel = responseSuggest;
            }
            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Data = quizResult,
                Message = "Retrieved user quiz result successfully"
            };
        }
    }
}
