namespace WebApplication4.Models
{
    public class PaymentDetailDto
    {

        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string DueDate { get; set; } // String to match client-side data
        public bool IsPaid { get; set; }
    }
}
