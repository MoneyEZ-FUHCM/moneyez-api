using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Repositories.Entities
{
    public partial class Post : BaseEntity
    {
        public required string Title { get; set; }

        public required string Content { get; set; }

        public string? ShortContent { get; set; }

        public string? Thumbnail { get; set; }
    }
}
