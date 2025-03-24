using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.CategoryModels;
using MoneyEz.Services.BusinessModels.SubcategoryModels;
using System.Linq;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void SubcategoryMapperConfig()
        {
            // Map từ CreateSubcategoryModel -> Subcategory
            CreateMap<CreateSubcategoryModel, Subcategory>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            // Map từ UpdateSubcategoryModel -> Subcategory
            CreateMap<UpdateSubcategoryModel, Subcategory>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            // Map từ Subcategory -> SubcategoryModel
            CreateMap<Subcategory, SubcategoryModel>();

            // Map từ Pagination<Subcategory> -> Pagination<SubcategoryModel>
            CreateMap<Pagination<Subcategory>, Pagination<SubcategoryModel>>()
              .ConvertUsing<PaginationConverter<Subcategory, SubcategoryModel>>();
        }
    }
}
