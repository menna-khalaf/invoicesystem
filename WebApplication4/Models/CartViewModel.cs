namespace WebApplication4.Models
{
    public class CartViewModel
    {
        public string CartId { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}
