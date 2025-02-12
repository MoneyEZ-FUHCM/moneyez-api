using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.AssetModels
{
    public class CreateAssetModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "SubcategoryId is required.")]
        public Guid SubcategoryId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        public DateTime? AcquisitionDate { get; set; }
        public DateTime? DepreciationDate { get; set; }
        public DateTime? RevaluationDate { get; set; }
        public DateTime? DisposalDate { get; set; }
        public DateTime? MaturityDate { get; set; }

        [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100.")]
        public double? Rate { get; set; }

        public string? Description { get; set; }

        public OwnershipType? OwnershipType { get; set; }
    }
}
