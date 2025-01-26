using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void CategoryMapperConfig()
        {
            // Map từ CreateCategoryModel -> Category
            CreateMap<CreateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            // Map từ UpdateCategoryModel -> Category
            CreateMap<UpdateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            // Map từ Category -> CategoryModel
            CreateMap<Category, CategoryModel>();
        }
    }
}
