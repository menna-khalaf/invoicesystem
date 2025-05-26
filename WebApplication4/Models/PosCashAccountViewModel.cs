namespace WebApplication4.Models
{
    public class PosCashAccountViewModel
    {
        public int PosId { get; set; }
        public string PosName { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }  // Required
        public string CashAccountName { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; }
        public int StorehouseId { get; set; }  // Required










        public int BankAccountId { get; set; } // Add this property
        public int CashAccountId { get; set; } // Add this property
    }
}