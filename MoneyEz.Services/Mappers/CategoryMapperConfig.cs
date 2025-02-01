using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void CategoryMapperConfig()
        {
            CreateMap<Category, CategoryModel>()
                .ForMember(dest => dest.Subcategories, opt => opt.MapFrom(src => src.Subcategories));

            // Map từ CreateCategoryModel sang Category entity
            CreateMap<CreateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            // Map từ UpdateCategoryModel sang Category entity
            CreateMap<UpdateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            // Map từ Pagination<Category> sang Pagination<CategoryModel>
            CreateMap<Pagination<Category>, Pagination<CategoryModel>>()
                .ConvertUsing<PaginationConverter<Category, CategoryModel>>();
        }
    }
}
