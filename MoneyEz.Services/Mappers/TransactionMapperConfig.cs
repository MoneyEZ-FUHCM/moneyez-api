using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.TransactionModels;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void TransactionMapperConfig()
        {
            CreateMap<Transaction, TransactionModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory.Name))
                .ForMember(dest => dest.SubcategoryIcon, opt => opt.MapFrom(src => src.Subcategory.Icon))
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            CreateMap<CreateTransactionModel, Transaction>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.APPROVED));

            CreateMap<UpdateTransactionModel, Transaction>();

            CreateMap<Pagination<Transaction>, Pagination<TransactionModel>>()
                .ConvertUsing<PaginationConverter<Transaction, TransactionModel>>();
        }
    }
}