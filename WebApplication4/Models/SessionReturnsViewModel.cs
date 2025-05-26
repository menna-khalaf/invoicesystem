namespace WebApplication4.Models
{
    public class SessionReturnsViewModel
    {
        public int OriginalInvoiceId { get; set; }
        public int? SessionId { get; set; }
        public List<Invoice> Returns { get; set; }
        public decimal TotalReturns { get; set; }
    }
}
