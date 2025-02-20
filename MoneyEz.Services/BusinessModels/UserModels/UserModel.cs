using MoneyEz.Repositories.Entities;
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.UserModels
{
    public class UserModel : BaseEntity
    {
        public string? FullName { get; set; }

        public string? NameUnsign { get; set; }

        public required string Email { get; set; }

        public DateTime? Dob { get; set; }

        public Gender? Gender { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public string? GoogleId { get; set; }

        public bool? IsVerified { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }
    }
}
