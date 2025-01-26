using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void CategoryMapperConfig()
        {
            CreateMap<CreateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<UpdateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<Category, CategoryModel>();
        }
    }
}
