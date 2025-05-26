using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "نوع الطلب مطلوب")]
        public string OrderType { get; set; }

        public DateTime? OrderDate { get; set; }

        [Required(ErrorMessage = "العميل مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار عميل")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "المستودع مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار مستودع")]
        public int StorehouseId { get; set; }

        [Required(ErrorMessage = "الموظف مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار موظف")]
        public int EmployeeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار مستخدم")]
        public int? UserId { get; set; } // Added UserId, nullable to match database

        public string Notes { get; set; }

        [Required(ErrorMessage = "يجب إضافة منتج واحد على الأقل")]
        public List<OrderProductingViewModel> Products { get; set; } = new List<OrderProductingViewModel>();
    }

    public class OrderProductingViewModel
    {
        [Required(ErrorMessage = "المنتج مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار منتج")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من 0")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }
    }
}