using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Services.BusinessModels.PostModels;
using MoneyEz.Services.BusinessModels.ResultModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Interfaces
{
    public interface IPostService
    {
        Task<BaseResultModel> GetPostPaginationAsync(PaginationParameter paginationParameter, PostFilter postFilter);
        Task<BaseResultModel> GetPostByIdAsync(Guid id);
        Task<BaseResultModel> AddPostAsync(CreatePostModel model);
        Task<BaseResultModel> UpdatePostAsync(UpdatePostModel model);
        Task<BaseResultModel> DeletePostAsync(Guid id);
    }
}
