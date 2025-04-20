using MoneyEz.Repositories.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.KnowledgeModels
{
    public class KnowledgeModel : BaseEntity
    {
        public string? Name { get; set; }

        public int Size { get; set; }

        public string? ContentType { get; set; }
    }

    public class ResponseKnowledgeModel
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Size { get; set; }

        public string CreatedDate { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }

    public class DocumentsResponse
    {
        [JsonPropertyName("documents")]
        public List<ResponseKnowledgeModel> Documents { get; set; } = new List<ResponseKnowledgeModel>();
    }
}
