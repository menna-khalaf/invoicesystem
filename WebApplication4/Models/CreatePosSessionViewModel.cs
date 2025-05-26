namespace WebApplication4.Models
{
    public class CreatePosSessionViewModel
    {
        public PosSession PosSession { get; set; }
        public CartViewModel Cart { get; set; }
        public List<Product> Products { get; set; }

        public int InvoiceCount { get; set; }

        // Navigation properties
        public string BranchName { get; set; }
        public string PosName { get; set; }
        public string EmployeeName { get; set; }
        public string StorehouseName { get; set; }

        public int SessionId { get; set; }
        public int Posid { get; set; }
        public int EmployeeId { get; set; }
        public int StorehouseId { get; set; }
        public int BranchId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal StartingCash { get; set; }
        public decimal? EndingCash { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        // Add this property for the original invoice
        public Invoice OriginalInvoice { get; set; }

        // Account related properties
        public int? CashAccountId { get; set; }
        public decimal? CashAccountBalance { get; set; }

        // Navigation properties
        public virtual Branch Branch { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Po> Pos { get; set; } = new List<Po>();
        public virtual ICollection<PosCart> PosCarts { get; set; } = new List<PosCart>();
        public virtual Po PosNavigation { get; set; }
        public virtual Storehouse Storehouse { get; set; }
    }
}