using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyEz.Repositories.Commons;
using MoneyEz.Repositories.Commons.Filters;
using MoneyEz.Repositories.Enums;
using MoneyEz.Services.BusinessModels.PostModels;
using MoneyEz.Services.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyEz.API.Controllers
{
    [Route("api/v1/posts")]
    [ApiController]
    [Authorize]
    public class PostsController : BaseController
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public Task<IActionResult> GetPosts([FromQuery] PaginationParameter paginationParameter, [FromQuery] PostFilter postFilter)
        {
            return ValidateAndExecute(() => _postService.GetPostPaginationAsync(paginationParameter, postFilter));
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetPostById(Guid id)
        {
            return ValidateAndExecute(() => _postService.GetPostByIdAsync(id));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPost]
        public Task<IActionResult> AddPost([FromBody] CreatePostModel model)
        {
            return ValidateAndExecute(() => _postService.AddPostAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpPut]
        public Task<IActionResult> UpdatePost([FromBody] UpdatePostModel model)
        {
            return ValidateAndExecute(() => _postService.UpdatePostAsync(model));
        }

        [Authorize(Roles = nameof(RolesEnum.ADMIN))]
        [HttpDelete("{id}")]
        public Task<IActionResult> DeletePost(Guid id)
        {
            return ValidateAndExecute(() => _postService.DeletePostAsync(id));
        }
    }
}
