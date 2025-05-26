namespace WebApplication4.Models
{
    public class ProcessPaymentViewModel
    {
        public int PayId { get; set; }
        public int PayDetailId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string CustomerName { get; set; }
        public decimal Amount { get; set; }
        public List<PaymentMethodModel> PaymentMethods { get; set; } = new List<PaymentMethodModel>();
        public List<CashAndBankAccount> Accounts { get; set; } = new List<CashAndBankAccount>();
    }

    public class PaymentMethodModel
    {
        public string PaymentMethod { get; set; }
        public int? AccountId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public DateTime? DueDate { get; set; } // Added to capture due date
    }

    public class CashAndBankAccount
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
    }
}