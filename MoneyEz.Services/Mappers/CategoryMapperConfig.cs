using AutoMapper;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.Utils;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void CategoryMapperConfig()
        {
            // Mapping từ Category entity sang CategoryModel (trả về client)
            CreateMap<Category, CategoryModel>();

            // Mapping từ CreateCategoryModel sang Category entity (thêm mới)
            CreateMap<CreateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            // Mapping từ UpdateCategoryModel sang Category entity (cập nhật)
            CreateMap<UpdateCategoryModel, Category>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));
        }
    }
}
