namespace WebApplication4.Models
{
    public class FinancialSummaryViewModel
    {


        public decimal CashAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CashNetAmount { get; set; }

        public decimal BankTransferAmount { get; set; }
        public decimal BankTransferRefundAmount { get; set; }
        public decimal BankTransferNetAmount { get; set; }

        public decimal DeferredAmount { get; set; }
        public decimal TotalNetAmount { get; set; }

        public decimal AdditionAmount { get; set; }
        public decimal DisbursementAmount { get; set; }

    }
}
