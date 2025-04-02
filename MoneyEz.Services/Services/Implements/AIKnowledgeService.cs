using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class AIKnowledgeService : IAIKnowledgeService
    {
        public Task<BaseResultModel> CreateKnowledgeAsync(CreateKnowledgeModel model)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResultModel> DeleteKnowledgeAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResultModel> GetKnowledgesAsync(PaginationParameter paginationParameter)
        {
            throw new NotImplementedException();
        }
    }
}
