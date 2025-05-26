namespace WebApplication4.Models
{
    public class ShiftReportViewModel
    {
        public PosSession Session { get; set; }
        public List<Invoice> Invoices { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalReturns { get; set; }
        public decimal NetSales { get; set; }
        public decimal TotalReturnsBySession { get; set; } // Added property for SQL query result
        public List<Invoicedetail> InvoiceDetails { get; set; }

        // Improved method to get the calculated total for an invoice
        public decimal GetInvoiceTotal(Invoice invoice)
        {
            if (invoice == null || InvoiceDetails == null)
                return 0;

            return InvoiceDetails
                .Where(d => d.InvoiceId == invoice.InvoiceId)
                .Sum(d => d.LineTotal + (d.VatAmount ?? 0));
        }

        // Additional helper method to get details for a specific invoice
        public List<Invoicedetail> GetInvoiceDetails(Invoice invoice)
        {
            if (invoice == null || InvoiceDetails == null)
                return new List<Invoicedetail>();

            return InvoiceDetails
                .Where(d => d.InvoiceId == invoice.InvoiceId)
                .ToList();
        }

        // Method to calculate returns for a specific invoice
        public decimal GetInvoiceReturns(Invoice invoice)
        {
            if (invoice == null || InvoiceDetails == null)
                return 0;

            return InvoiceDetails
                .Where(d => d.InvoiceId == invoice.InvoiceId && d.InvoiceTyping == "مرتجع من البيع")
                .Sum(d => d.LineTotal + (d.VatAmount ?? 0));
        }
    }
}