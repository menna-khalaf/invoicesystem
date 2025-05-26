using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class QuotationDetailViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Display(Name = "اسم المنتج")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "الكمية")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "سعر الوحدة")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "قيمة الضريبة")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal VatAmount { get; set; }

        [Display(Name = "الإجمالي")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal LineTotal { get; set; }

        [Display(Name = "الوصف")]
        public string ProductDescription { get; set; }

        [Display(Name = "نوع الخصم")]
        public string DiscountType { get; set; }

        [Display(Name = "قيمة الخصم")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Discount { get; set; }
    }
}
