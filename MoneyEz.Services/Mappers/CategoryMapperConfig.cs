using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void CategoryMapperConfig()
        {
            CreateMap<Category, CategoryModel>()
                .ForMember(dest => dest.Subcategories, opt => opt.MapFrom(src => src.CategorySubcategories.Select(cs => cs.Subcategory).ToList()));

            CreateMap<CategorySubcategory, SubcategoryModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Subcategory.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Subcategory.Name))
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.Subcategory.NameUnsign))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Subcategory.Description));

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
