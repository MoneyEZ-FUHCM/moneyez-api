using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.FinancialGoalModels;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void FinancialGoalMapperConfig()
        {
            // Mapping cho Financial Goal cá nhân
            CreateMap<FinancialGoal, PersonalFinancialGoalModel>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<AddPersonalFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            CreateMap<UpdatePersonalFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            CreateMap<Pagination<FinancialGoal>, Pagination<PersonalFinancialGoalModel>>()
                .ConvertUsing<PaginationConverter<FinancialGoal, PersonalFinancialGoalModel>>();

            // Mapping cho Financial Goal nhóm
            CreateMap<FinancialGoal, GroupFinancialGoalModel>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<AddGroupFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            CreateMap<UpdateGroupFinancialGoalModel, FinancialGoal>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            CreateMap<Pagination<FinancialGoal>, Pagination<GroupFinancialGoalModel>>()
                .ConvertUsing<PaginationConverter<FinancialGoal, GroupFinancialGoalModel>>();
        }
    }
}
