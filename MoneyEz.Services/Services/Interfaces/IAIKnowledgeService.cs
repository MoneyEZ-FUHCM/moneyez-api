﻿using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IAIKnowledgeService
    {
        public Task<BaseResultModel> GetKnowledgesAsync(PaginationParameter paginationParameter);

        public Task<BaseResultModel> CreateKnowledgeAsync(CreateKnowledgeModel model);

        public Task<BaseResultModel> DeleteKnowledgeAsync(Guid id);
    }
}
