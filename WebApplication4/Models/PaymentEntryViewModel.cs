using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class PaymentEntryViewModel
    {
        [Required(ErrorMessage = "يجب اختيار طريقة الدفع")]
        [Display(Name = "طريقة الدفع")]
        public string PaymentMethod { get; set; }

        [Display(Name = "الحساب")]
        public int? AccountId { get; set; }

        [Required(ErrorMessage = "يجب إدخال المبلغ")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من 0")]
        [Display(Name = "المبلغ")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Amount { get; set; }

        [Display(Name = "تاريخ الاستحقاق")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "تاريخ الدفع")]
        [DataType(DataType.Date)]
        public DateTime? PaidDate { get; set; }
    }
}
