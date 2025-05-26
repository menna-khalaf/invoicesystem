namespace WebApplication4.Models
{
    public class InvoiceSummaryViewModel
    {

        public decimal NetPurchases { get; set; }

        // VAT on Purchases: (Purchases * PurchasePrice * VATRate / 100) - (Returns * PurchasePrice * VATRate / 100)
        public decimal VATPurchases { get; set; }

        // Net Sales: (Sales - Returns) * Price
        public decimal NetSales { get; set; }

        // VAT on Sales: (Sales * Price * VATRate / 100) - (Returns * Price * VATRate / 100)
        public decimal VATSales { get; set; }

        // Net VAT: VATPurchases - VATSales
        public decimal NetVAT { get; set; }
    }
}
