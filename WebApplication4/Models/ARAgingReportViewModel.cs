namespace WebApplication4.Models
{
    public class ARAgingReportViewModel
    {
        public List<CustomerAgingInvoice> Invoices { get; set; } = new List<CustomerAgingInvoice>();
    }

    public class CustomerAgingInvoice
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int InvoiceId { get; set; }
        public int? PayDetailId { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysPastDue { get; set; }
        public string DelayCategory { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal DaysOver90 { get; set; }
    }
}
