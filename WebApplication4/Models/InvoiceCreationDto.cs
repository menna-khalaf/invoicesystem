namespace WebApplication4.Models
{
    public class InvoiceCreationDto
    {

        public Invoice Invoice { get; set; }
        public List<Invoicedetail> InvoiceDetails { get; set; }
        public List<PaymentDetailDto> PaymentDetails { get; set; }
    }
}
