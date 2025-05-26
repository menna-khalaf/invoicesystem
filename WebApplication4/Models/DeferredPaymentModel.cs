namespace WebApplication4.Models
{
    public class DeferredPaymentModel
    {

        public int PayId { get; set; }
        public int? CustomerId { get; set; }
        public int? VendorId { get; set; }
        public int UserId { get; set; }
        public DateOnly PayDate { get; set; }
        public DateTime? PaymentUpdatedAt { get; set; }
        public int PayDetailId { get; set; }
        public int DetailUserId { get; set; }
        public string Description { get; set; }
        public decimal DetailAmount { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool? IsPaid { get; set; }
        public DateOnly? PaidDate { get; set; }
        public DateTime? DetailUpdatedAt { get; set; }
    }
}
