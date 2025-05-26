using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class TransferCreateViewModel
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public int? InvoiceId { get; set; }
        public int? EmployeeId { get; set; }
        public string Notes { get; set; }

        public List<SelectListItem> ToAccounts { get; set; }
        public List<SelectListItem> Invoices { get; set; }
        public List<SelectListItem> Employees { get; set; }
    }

}