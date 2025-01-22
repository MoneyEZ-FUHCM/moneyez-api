
using MoneyEz.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.BusinessModels.GroupMember
{
    public class CreateGroupModel
    {

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm.")]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số dư hiện tại không hợp lệ.")]
        public decimal? CurrentBalance { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã trưởng nhóm.")]
        [Display(Name = "Leader")]
        public int Leader { get; set; }
    }
}
