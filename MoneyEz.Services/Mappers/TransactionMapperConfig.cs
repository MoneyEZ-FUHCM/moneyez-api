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
            // Map từ Transaction entity sang TransactionModel
            CreateMap<Transaction, TransactionModel>();

            // Map từ CreateTransactionModel sang Transaction entity
            CreateMap<CreateTransactionModel, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.APPROVED)); // Mặc định approved

            // Map từ UpdateTransactionModel sang Transaction entity
            CreateMap<UpdateTransactionModel, Transaction>();

            // Map từ Pagination<Transaction> sang Pagination<TransactionModel>
            CreateMap<Pagination<Transaction>, Pagination<TransactionModel>>()
                .ConvertUsing<PaginationConverter<Transaction, TransactionModel>>();
        }
    }
}
