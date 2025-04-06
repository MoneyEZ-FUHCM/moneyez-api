using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.PostModels
{
    public class CreatePostModel
    {
        [Required(ErrorMessage = "Tiêu đề bài viết là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề bài viết không được vượt quá 200 ký tự.")]
        public required string Title { get; set; }


        [Required(ErrorMessage = "Nội dung bài viết là bắt buộc.")]
        public required string Content { get; set; }

        
        [StringLength(500, ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        public string? ShortContent { get; set; }
        
        public string? Thumbnail { get; set; }
    }
}
