using AutoMapper;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.SubcategoryModels;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void SubcategoryMapperConfig()
        {
            CreateMap<CreateSubcategoryModel, Subcategory>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<UpdateSubcategoryModel, Subcategory>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => src.NameUnsign));

            CreateMap<Subcategory, SubcategoryModel>();

            CreateMap<Pagination<Subcategory>, Pagination<SubcategoryModel>>()
              .ConvertUsing<PaginationConverter<Subcategory, SubcategoryModel>>();
        }
    }
}
