using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.AssetModels;
using MoneyEz.Services.Utils;


namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void AssetMapperConfig()
        {
            CreateMap<Asset, AssetModel>();

            // Map from CreateAssetModel to Asset entity
            CreateMap<CreateAssetModel, Asset>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            // Map from UpdateAssetModel to Asset entity
            CreateMap<UpdateAssetModel, Asset>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));
        }
    }
}
