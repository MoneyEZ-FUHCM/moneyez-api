using AutoMapper;
using MoneyEz.Repositories.Entities;
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
            CreateMap<CreateQuizModel, Quiz>()
                .ForMember(dest => dest.Questions, opt => opt.Ignore());

            CreateMap<CreateQuestionModel, Question>()
                .ForMember(dest => dest.AnswerOptions, opt => opt.Ignore());

            CreateMap<CreateAnswerOptionModel, AnswerOption>();

            CreateMap<CreateUserQuizAnswerModel, UserQuizAnswer>();

            CreateMap<Quiz, QuizModel>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<Question, QuestionModel>()
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions));

            CreateMap<AnswerOption, AnswerOptionModel>();

            CreateMap<UserQuizAnswer, UserQuizAnswerModel>();

            CreateMap<UserQuizResult, UserQuizResultModel>()
                .ForMember(dest => dest.UserAnswers, opt => opt.MapFrom(src => src.UserQuizAnswers));

            CreateMap<CreateUserQuizResultModel, UserQuizResult>()
                .ForMember(dest => dest.UserQuizAnswers, opt => opt.MapFrom(src => src.UserQuizAnswers));
        }
    }
}
