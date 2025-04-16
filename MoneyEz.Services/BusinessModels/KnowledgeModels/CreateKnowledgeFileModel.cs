using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.KnowledgeModels
{
    public class CreateKnowledgeFileModel
    {
        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;

        //[Range(0, 10 * 1024 * 1024)]
        //public long MaxFileSize { get; } = 10 * 1024 * 1024;

        //public string Size => File != null ? $"{File.Length / 1024.0:F2} KB" : "0 KB";

        // Allowed file types
        public static readonly string[] AllowedFileTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "text/plain"
        };

        public bool ValidateFileType()
        {
            return File != null && AllowedFileTypes.Contains(File.ContentType);
        }
    }
}
