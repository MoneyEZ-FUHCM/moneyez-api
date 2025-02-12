using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.LiabilityModels;
using MoneyEz.Services.Utils;


namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void LiabilityMapperConfig()
        {
            CreateMap<Liability, LiabilityModel>();

            // Map from CreateLiabilityModel to Liability entity
            CreateMap<CreateLiabilityModel, Liability>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));

            // Map from UpdateLiabilityModel to Liability entity
            CreateMap<UpdateLiabilityModel, Liability>()
                .ForMember(dest => dest.NameUnsign, opt => opt.MapFrom(src => StringUtils.ConvertToUnSign(src.Name)));
        }
    }
}
