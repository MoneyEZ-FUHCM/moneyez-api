using AutoMapper;
using Microsoft.AspNetCore.Http;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.UnitOfWork;
using MoneyEz.Services.BusinessModels.PostModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using MoneyEz.Services.Constants;
using MoneyEz.Services.Exceptions;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClaimsService _claimsService;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<BaseResultModel> GetPostPaginationAsync(PaginationParameter paginationParameter, PostFilter postFilter)
        {
            var posts = await _unitOfWork.PostRepository.GetPostsByFilter(paginationParameter, postFilter);

            var postModels = _mapper.Map<Pagination<PostModel>>(posts);
            var result = PaginationHelper.GetPaginationResult(postModels, postModels);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.POST_LIST_FETCHED_SUCCESS,
                Data = result
            };
        }

        public async Task<BaseResultModel> GetPostByIdAsync(Guid id)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(id);

            if (post == null || post.IsDeleted)
            {
                throw new NotExistException(MessageConstants.POST_NOT_FOUND);
            }

            var postModel = _mapper.Map<PostModel>(post);

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.POST_FETCHED_SUCCESS,
                Data = postModel
            };
        }

        public async Task<BaseResultModel> AddPostAsync(CreatePostModel model)
        {
            var post = _mapper.Map<Post>(model);
            post.CreatedBy = _claimsService.GetCurrentUserEmail;
            
            await _unitOfWork.PostRepository.AddAsync(post);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status201Created,
                Message = MessageConstants.POST_CREATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> UpdatePostAsync(UpdatePostModel model)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(model.Id);
            
            if (post == null || post.IsDeleted)
            {
                throw new NotExistException(MessageConstants.POST_NOT_FOUND);
            }

            _mapper.Map(model, post);
            post.UpdatedBy = _claimsService.GetCurrentUserEmail;
            
            _unitOfWork.PostRepository.UpdateAsync(post);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.POST_UPDATED_SUCCESS
            };
        }

        public async Task<BaseResultModel> DeletePostAsync(Guid id)
        {
            var post = await _unitOfWork.PostRepository.GetByIdAsync(id);
            
            if (post == null || post.IsDeleted)
            {
                throw new NotExistException(MessageConstants.POST_NOT_FOUND);
            }

            post.UpdatedBy = _claimsService.GetCurrentUserEmail;
            _unitOfWork.PostRepository.SoftDeleteAsync(post);
            _unitOfWork.Save();

            return new BaseResultModel
            {
                Status = StatusCodes.Status200OK,
                Message = MessageConstants.POST_DELETED_SUCCESS
            };
        }
    }
}
