using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.SpendingModelModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void SpendingModelMapperConfig()
        {
            // Map từ CreateSpendingModelModel -> SpendingModel
            CreateMap<CreateSpendingModelModel, SpendingModel>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign))
                .ForMember(dest => dest.IsTemplate, opt => opt.MapFrom(src => src.IsTemplate));

            // Map từ UpdateSpendingModelModel -> SpendingModel
            CreateMap<UpdateSpendingModelModel, SpendingModel>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign))
                .ForMember(dest => dest.IsTemplate, opt => opt.MapFrom(src => src.IsTemplate));


            // Map từ SpendingModel -> SpendingModelModel
            CreateMap<SpendingModel, SpendingModelModel>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.SpendingModelCategories.Select(smc => new SpendingModelCategoryModel
                {
                    PercentageAmount = smc.PercentageAmount ?? 0, // Xử lý null bằng 0
                    Category = new CategoryModel
                    {
                        Id = smc.Category.Id,
                        Name = smc.Category.Name,
                        NameUnsign = smc.Category.NameUnsign,
                        Description = smc.Category.Description,
                        IsDeleted = smc.Category.IsDeleted,
                        CreatedDate = smc.Category.CreatedDate,
                        UpdatedDate = smc.Category.UpdatedDate,
                        CreatedBy = smc.Category.CreatedBy,
                        UpdatedBy = smc.Category.UpdatedBy
                    }
                })));

            // Map từ Pagination<SpendingModel> -> Pagination<SpendingModelModel>
            CreateMap<Pagination<SpendingModel>, Pagination<SpendingModelModel>>()
              .ConvertUsing<PaginationConverter<SpendingModel, SpendingModelModel>>();
        }
    }
}
