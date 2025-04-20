using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Services.BusinessModels.ExternalServiceModels;
using MoneyEz.Services.BusinessModels.KnowledgeModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
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

        public async Task<BaseResultModel> CreateKnowledgeAsync(CreateKnowledgeModel model)
        {
            // Validate model and input file
            if (model == null)
            {
                throw new DefaultException("Knowledge model cannot be null", MessageConstants.KNOWLEDGE_DOCUMENT_INVALID_MODEL);
            }

            if (model.File == null || model.File.Length == 0)
            {
                throw new DefaultException("File is required and cannot be empty", MessageConstants.KNOWLEDGE_DOCUMENT_INVALID_FILE);
            }

            if (!model.ValidateFileType())
            {
                throw new DefaultException("Invalid file type. Allowed types are: PDF, DOC, DOCX, and TXT", 
                    MessageConstants.KNOWLEDGE_DOCUMENT_INVALID_FILE_TYPE);
            }

            // Call ExternalApiService to upload the document
            return await _externalApiService.ExecuteCreateKnownledgeDocument(model.File);
        }

        public async Task<BaseResultModel> DeleteKnowledgeAsync(Guid id)
        {
            return await _externalApiService.ExecuteDeleteKnownledgeDocument(id);
        }

        public async Task<BaseResultModel> GetKnowledgesAsync(PaginationParameter paginationParameter)
        {
            return await _externalApiService.GetKnowledgeDocuments(paginationParameter);
        }
    }
}
