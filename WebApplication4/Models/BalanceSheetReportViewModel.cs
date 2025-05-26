namespace WebApplication4.Models
{
    public class BalanceSheetReportViewModel
    {
        public List<BalanceSheetReportItem> ReportItems { get; set; }
        public int UserId { get; set; }
        public DateOnly AsOfDate { get; set; } // Added to support BalanceSheet action

        // Added properties to support totals and balance check
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalLiabilitiesAndEquity { get; set; }
        public bool IsBalanced { get; set; }
    }

    public class BalanceSheetReportItem
    {
        public string AccountType { get; set; }
        public string ChildAccountName { get; set; }
        public decimal Balance { get; set; }
    }
}