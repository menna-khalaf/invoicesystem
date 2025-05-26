namespace WebApplication4.Models
{
    public class PurchaseReturnsReportViewModel
    {
        public int InvoiceID { get; set; }

        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int TotalReturned { get; set; }
        public decimal? Vatrate { get; set; } // Added VATRate
        public decimal PurchasePrice { get; set; } // Added PurchasePrice
        public decimal Price { get; set; } // Added PurchasePrice
    }
}

