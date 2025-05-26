namespace WebApplication4.Models
{
    public class ProductSummary
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int TotalPurchasedQuantity { get; set; }
        public int TotalReturnedQuantity { get; set; }
    }
}
