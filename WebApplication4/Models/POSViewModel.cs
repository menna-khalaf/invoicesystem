namespace WebApplication4.Models
{
    public class POSViewModel
    {
        public PosSession Session { get; set; }

        // Rest of the properties remain the same
        public List<Product> Products { get; set; }
        public List<ProductCategory> Categories { get; set; }
        public List<PosPaymentMethod> PaymentMethods { get; set; }
        public string CartId { get; set; }
        public List<PosSession> ActiveSessions { get; set; }
        public int SessionId { get; set; }
        public string PosName { get; set; }
        public string EmployeeName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime DateTime { get; set; }

    }
}
