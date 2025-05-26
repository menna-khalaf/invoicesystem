namespace WebApplication4.Models
{
    public class ReturnedProductViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int InvoiceId { get; set; }
    }

    public class SessionReturnsReportViewModel
    {
        public PosSession Session { get; set; }
        public List<ReturnedProductViewModel> ReturnedProducts { get; set; }
        public dynamic Totals { get; set; } // Using dynamic for simplicity
    }
}
