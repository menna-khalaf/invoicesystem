namespace WebApplication4.Models
{
    public class AddChildAccountViewModel
    {
        public string Name { get; set; }
        public int ParentAccountId { get; set; }
        public int? UserId { get; set; }
        public decimal InitialBalance { get; set; }
    }

    public class AccountHierarchyViewModel
    {
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public List<ParentAccountViewModel> ParentAccounts { get; set; }
    }

    public class ParentAccountViewModel
    {
        public int? ParentAccountId { get; set; }
        public string ParentAccountName { get; set; }
        public List<string> ChildAccounts { get; set; }
    }
}
