using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.FinancialReportModels;
using MoneyEz.Repositories.Commons;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void FinancialReportMapperConfig()
        {
            CreateMap<FinancialReport, FinancialReportModel>();

            CreateMap<CreateUserReportModel, FinancialReport>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.GroupId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalIncome, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalExpense, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.NetBalance, opt => opt.MapFrom(src => 0));

            CreateMap<CreateGroupReportModel, FinancialReport>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalIncome, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalExpense, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.NetBalance, opt => opt.MapFrom(src => 0));

            CreateMap<UpdateUserReportModel, FinancialReport>();
            CreateMap<UpdateGroupReportModel, FinancialReport>();

            CreateMap<Pagination<FinancialReport>, Pagination<FinancialReportModel>>()
                .ConvertUsing<PaginationConverter<FinancialReport, FinancialReportModel>>();
        }
    }
}
