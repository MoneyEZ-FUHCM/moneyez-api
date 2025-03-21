﻿using MoneyEz.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace MoneyEz.Services.BusinessModels.FinancialReportModels
{
    public class CreateGroupReportModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}
