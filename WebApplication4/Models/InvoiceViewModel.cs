namespace WebApplication4.Models
{
    public class InvoiceViewModel
    {
        public int? InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public string InvoiceType { get; set; }
        public int? DetailID { get; set; }
        public int? ProductID { get; set; }
        public int? Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal? VatAmount { get; set; }
        public string InvoiceTyping { get; set; }
        public int UserId { get; set; } // Temporary for debugging








        //VendorId
        public string VendorName { get; set; }
    }
}