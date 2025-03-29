using MoneyEz.Repositories.Entities;
using MoneyEz.Services.BusinessModels.ImageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Mappers
{
    public partial class MapperConfig
    {
        partial void ImageMapperConfig()
        {
            CreateMap<Image, ImageModel>();
        }
    }
}
