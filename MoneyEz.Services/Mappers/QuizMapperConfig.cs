using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.BankAccountModels;
using MoneyEz.Services.BusinessModels.QuizModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public class QuizMapperConfig : Profile
    {
        public QuizMapperConfig()
        {
            // Quiz mappings
            CreateMap<CreateQuizModel, Quiz>()
                .ForMember(dest => dest.QuestionsJson, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    // Convert Questions to JSON format
                    var quizQuestions = src.Questions.Select(q => new QuizQuestion
                    {
                        Id = q.Id,
                        Content = q.Content,
                        AnswerOptions = q.AnswerOptions.Select(a => new QuizAnswerOption
                        {
                            Id = a.Id,
                            Content = a.Content
                        }).ToList()
                    }).ToList();

                    dest.SetQuestions(quizQuestions);
                });

            CreateMap<UpdateQuizModel, Quiz>()
                .ForMember(dest => dest.QuestionsJson, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    // Convert Questions to JSON format
                    var quizQuestions = src.Questions.Select(q => new QuizQuestion
                    {
                        Id = q.Id,
                        Content = q.Content,
                        AnswerOptions = q.AnswerOptions.Select(a => new QuizAnswerOption
                        {
                            Id = a.Id,
                            Content = a.Content
                        }).ToList()
                    }).ToList();

                    dest.SetQuestions(quizQuestions);
                });

            CreateMap<Quiz, QuizModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.GetQuestions()
                    .Select(q => new QuizQuestionModel
                    {
                        Id = q.Id,
                        Content = q.Content,
                        AnswerOptions = q.AnswerOptions.Select(a => new QuizAnswerOptionModel
                        {
                            Id = a.Id,
                            Content = a.Content
                        }).ToList()
                    }).ToList()));

            CreateMap<Quiz, UpdateQuizModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.GetQuestions()
                    .Select(q => new QuizQuestionModel
                    {
                        Id = q.Id,
                        Content = q.Content,
                        AnswerOptions = q.AnswerOptions.Select(a => new QuizAnswerOptionModel
                        {
                            Id = a.Id,
                            Content = a.Content
                        }).ToList()
                    }).ToList()));

            // User quiz result mappings
            CreateMap<CreateQuizAttemptModel, UserQuizResult>()
                .ForMember(dest => dest.RecommendedModel, opt => opt.Ignore())
                .ForMember(dest => dest.AnswersJson, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    var answers = src.Answers.Select(a => new UserAnswer
                    {
                        QuestionId = a.QuestionId ?? Guid.Empty,
                        AnswerOptionId = a.AnswerOptionId,
                        AnswerContent = a.AnswerContent
                    }).ToList();

                    dest.SetAnswers(answers);
                });

            CreateMap<UserQuizResult, QuizAttemptModel>()
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src =>
                    src.GetAnswers().Select(a => new UserAnswerModel
                    {
                        QuestionId = a.QuestionId == Guid.Empty ? null : a.QuestionId,
                        AnswerOptionId = a.AnswerOptionId,
                        AnswerContent = a.AnswerContent
                    }).ToList()
                 ));

            // Add mapping for UserQuizResult to UserQuizResultModel
            CreateMap<UserQuizResult, UserQuizResultModel>()
                .ForMember(dest => dest.RecommendedModel, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src =>
                    src.GetAnswers().Select(a => new UserAnswerModel
                    {
                        QuestionId = a.QuestionId == Guid.Empty ? null : a.QuestionId,
                        AnswerOptionId = a.AnswerOptionId,
                        AnswerContent = a.AnswerContent
                    }).ToList()
                ));

            CreateMap<Pagination<UserQuizResult>, Pagination<UserQuizResultModel>>().ConvertUsing<PaginationConverter<UserQuizResult, UserQuizResultModel>>();
        }
    }
}
