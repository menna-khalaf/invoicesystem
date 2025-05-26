namespace WebApplication4.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }  // Changed from UnitPrice to match view
        public int Quantity { get; set; }
        public decimal TotalPrice =>Price * Quantity;
        public decimal DiscountPercent { get; set; }

        // Added VAT-related properties
        public decimal VatRate { get; set; }  // VAT rate as percentage (e.g., 15 for 15%)
        public decimal VatAmount => (Price * Quantity) * (VatRate / 100);
        public decimal PriceIncludingVat => Price * Quantity + VatAmount;

        // Added for detailed display
        //public string ProductCode { get; set; }
        //public string ProductDescription { get; set; }
    }
}