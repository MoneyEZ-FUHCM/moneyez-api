using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.TransactionModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void TransactionMapperConfig()
        {
            // Map Transaction -> TransactionModel
            CreateMap<Transaction, TransactionModel>()
                .ForMember(dest => dest.Subcategory, opt => opt.MapFrom(src => src.Subcategory));

            // Map CreateTransactionModel -> Transaction
            CreateMap<CreateTransactionModel, Transaction>();

            // Map UpdateTransactionModel -> Transaction
            CreateMap<UpdateTransactionModel, Transaction>();
        }
    }
}
