using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.PostModels
{
    public class UpdatePostModel : CreatePostModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
