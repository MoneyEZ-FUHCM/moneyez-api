using MoneyEz.Repositories.Enums;
using System.ComponentModel.DataAnnotations;


namespace MoneyEz.Services.BusinessModels.LiabilityModels
{
    public class UpdateLiabilityModel
    {
        [Required(ErrorMessage = "Id is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters.")]
        public string Name { get; set; }

        public string? NameUnsign { get; set; }

        [Required(ErrorMessage = "SubcategoryId is required.")]
        public Guid SubcategoryId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        public DateTime? RecognitionDate { get; set; } // Ngày xuất hiện ghi nợ

        public DateTime? DueDate { get; set; } // Hạn thanh toán

        public DateTime? InterestPaymentDate { get; set; } // Hạn thanh toán

        [Range(0, 100, ErrorMessage = "InterestRate must be between 0 and 100.")]
        public double? InterestRate { get; set; }

        public string? Description { get; set; }

        public OwnershipType? OwnershipType { get; set; }
    }
}
