using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace MoneyEz.Services.BusinessModels.KnowledgeModels
{
    public class CreateKnowledgeModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "File is required")]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = null!;

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
