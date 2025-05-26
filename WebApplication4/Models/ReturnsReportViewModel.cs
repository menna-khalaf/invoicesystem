namespace WebApplication4.Models
{
    public class ReturnsReportViewModel
    {

        public int InvoiceID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int TotalReturned { get; set; }
        public decimal Vatrate { get; set; } // Added VATRate
        public decimal Price { get; set; } // Added Price (selling price)









        public int ProductId { get; set; }
    }
}
