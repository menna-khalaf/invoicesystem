namespace WebApplication4.Models
{
    public class InvoiceDetailViewModel
    {
        public Invoice Invoice { get; set; }
        public List<Invoicedetail> SaleDetails { get; set; }
        public List<Invoicedetail> ReturnDetails { get; set; }
    }
}
