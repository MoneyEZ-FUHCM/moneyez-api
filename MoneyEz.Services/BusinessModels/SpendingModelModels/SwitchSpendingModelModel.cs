using MoneyEz.Repositories.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.SpendingModelModels
{
    public class SwitchSpendingModelModel
    {
        [Required(ErrorMessage = "New Spending Model Id is required.")]
        public Guid SpendingModelId { get; set; }

        [Required(ErrorMessage = "Period Unit is required.")]
        public PeriodUnit PeriodUnit { get; set; }

        [Required(ErrorMessage = "Period Value is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Period Value must be greater than 0.")]
        public int PeriodValue { get; set; }
        public DateTime? StartDate { get; set; } // Optional, default = tomorrow

    }
}
