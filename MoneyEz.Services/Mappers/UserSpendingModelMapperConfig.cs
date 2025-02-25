using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.SpendingModelModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void UserSpendingModelMapperConfig()
        {
            CreateMap<UserSpendingModel, UserSpendingModelModel>()
                .ForMember(dest => dest.PeriodUnit, opt => opt.MapFrom(src => (PeriodUnit)src.PeriodUnit))
                .ForMember(dest => dest.SpendingModel, opt => opt.MapFrom(src => src.SpendingModel));

            CreateMap<UserSpendingModel, UserSpendingModelHistoryModel>()
                .ForMember(dest => dest.SpendingModelId, opt => opt.MapFrom(src => src.SpendingModelId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SpendingModel.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.SpendingModel.Description))
                .ForMember(dest => dest.IsTemplate, opt => opt.MapFrom(src => src.SpendingModel.IsTemplate))
                .ForMember(dest => dest.PeriodUnit, opt => opt.MapFrom(src => (PeriodUnit)src.PeriodUnit))
                .ForMember(dest => dest.PeriodValue, opt => opt.MapFrom(src => src.PeriodValue))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));

            CreateMap<Pagination<UserSpendingModel>, Pagination<UserSpendingModelModel>>()
              .ConvertUsing<PaginationConverter<UserSpendingModel, UserSpendingModelModel>>();
        }
    }
}
