namespace WebApplication4.Models
{
    public class PaymentConfirmationViewModel
    {
        public Invoice Invoice { get; set; }
        public int? SessionId { get; set; }
        public CompanyInfoViewModel CompanyInfo { get; set; }
    }

    public class CompanyInfoViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string TaxNumber { get; set; }
        public string LogoUrl { get; set; }
    }
}
