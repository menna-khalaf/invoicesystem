using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models
{
    public class PaymentViewModel
    {









        // Remove if not needed
        public List<PosPaymentMethod> PaymentMethods { get; set; }

        [Required(ErrorMessage = "Cart items are required")]
        public List<CartItemViewModel> CartItems { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be positive")]
        public decimal Subtotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tax must be positive or zero")]
        public decimal Tax { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be positive")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "At least one payment method is required")]
        [MinLength(1, ErrorMessage = "At least one payment method is required")]
        public List<PaymentItem> PaymentTypes { get; set; } = new List<PaymentItem>();

        public string CustomerName { get; set; } // Make optional if needed

        public int? CustomerId { get; set; }





        [Display(Name = "Cash Account ID")]
        public int? CashAccountId { get; set; }








        [Display(Name = "Cash Account ID")]
        public int? BankAccountId { get; set; }









        public int? SessionId { get; set; }
    }

    public class PaymentItem
    {
        [Required(ErrorMessage = "Payment type is required")]
        public string Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }


        public DateTime? DueDate { get; set; }
    }


}
