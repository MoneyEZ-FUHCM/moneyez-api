using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
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
        private readonly IExternalApiService _externalApiService;

        public AIKnowledgeService(IExternalApiService externalApiService) 
        {
            _externalApiService = externalApiService;
        }

        public Task<BaseResultModel> CreateKnowledgeAsync(CreateKnowledgeModel model)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResultModel> DeleteKnowledgeAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResultModel> GetKnowledgesAsync(PaginationParameter paginationParameter)
        {
            var model = new ExternalKnowledgeRequestModel
            {
                Command = "get_all_documents"
            };
            return await _externalApiService.ExecuteKnownledgeDocumentSerivce(model, paginationParameter);
        }
    }
}
