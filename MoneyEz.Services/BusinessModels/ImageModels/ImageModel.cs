using MoneyEz.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.ImageModels
{
    public class ImageModel : BaseEntity
    {
        public Guid EntityId { get; set; }

        public string EntityName { get; set; } = "";

        public string ImageUrl { get; set; } = "";
    }
}
