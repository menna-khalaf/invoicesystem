namespace WebApplication4.Models
{
    public class QuotationViewModel
    {
        public Quotation Quotation { get; set; }
        public List<Quotationdetail> QuotationDetails { get; set; } = new List<Quotationdetail>();
    }
}
