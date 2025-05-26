using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class ConvertToInvoiceViewModel
    {
        [Required]
        public int QuotationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Display(Name = "اسم العميل")]
        public string CustomerName { get; set; }

        public int? EmployeeId { get; set; }

        [Display(Name = "اسم الموظف")]
        public string EmployeeName { get; set; }

        public int? BranchId { get; set; }

        [Display(Name = "اسم الفرع")]
        public string BranchName { get; set; }

        public int? StorehouseId { get; set; }

        [Display(Name = "اسم المستودع")]
        public string StorehouseName { get; set; }

        [Required]
        [Display(Name = "تاريخ عرض السعر")]
        [DataType(DataType.Date)]
        public DateTime QuotationDate { get; set; }

        [Required]
        [Display(Name = "الإجمالي")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "المجموع الفرعي")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Subtotal { get; set; }

        [Required]
        [Display(Name = "قيمة الضريبة")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal VatAmount { get; set; }

        [Required]
        [Display(Name = "تفاصيل عرض السعر")]
        public List<QuotationDetailViewModel> Details { get; set; } = new List<QuotationDetailViewModel>();

        // Additional fields for payment entries (to be captured in the view)
        public List<PaymentEntryViewModel> PaymentEntries { get; set; } = new List<PaymentEntryViewModel>();
    }
}
