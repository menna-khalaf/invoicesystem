namespace WebApplication4.Models
{
    public class SalesReportViewModel
    {
        public int InvoiceID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public int TotalReturned { get; set; }
        public int NetSold { get; set; }
        public decimal VATRate { get; set; }
        public decimal Price { get; set; }
        public int ProductId { get; set; }
        public string ReturnStatus { get; set; } // Added to store return status





        public decimal UnitPrice { get; set; } // Discounted unit price
        public decimal VatAmount { get; set; } // Total VAT for the line
        public decimal LineTotal { get; set; } // Total after discount plus VAT



        public string InvoiceSource { get; set; } // "Quotation" or "Ordinary"
    }
}