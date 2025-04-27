using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupFund
{
    public class UpdateGroupModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm.")]
        [Display(Name = "Name")]
        public required string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }


        [Required(ErrorMessage = "Vui lòng chọn tài khoản ngân hàng.")]
        public Guid AccountBankId { get; set; }

        public string? Image { get; set; }
    }
}
