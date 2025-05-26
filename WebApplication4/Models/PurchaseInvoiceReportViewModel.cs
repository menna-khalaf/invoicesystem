namespace WebApplication4.Models
{
    /// <summary>
    /// View model for displaying purchase invoice reports, including return status.
    /// </summary>
    public class PurchaseInvoiceReportViewModel
    {
        /// <summary>
        /// The ID of the invoice.
        /// </summary>
        public int InvoiceID { get; set; }

        /// <summary>
        /// The date of the invoice.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// The name of the customer associated with the invoice.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// The name of the product purchased.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// The ID of the product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// The total quantity purchased.
        /// </summary>
        public int TotalPurchased { get; set; }

        /// <summary>
        /// The total quantity returned.
        /// </summary>
        public int TotalReturned { get; set; }

        /// <summary>
        /// The net quantity purchased (TotalPurchased - TotalReturned).
        /// </summary>
        public int NetPurchased { get; set; }

        /// <summary>
        /// The VAT rate applied to the product.
        /// </summary>
        public decimal VatRate { get; set; }

        /// <summary>
        /// The purchase price of the product.
        /// </summary>
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// The return status of the product ("تم الرجوع بالكامل", "مرتجع جزئي", or "لم يتم الرجوع").
        /// </summary>
        public string ReturnStatus { get; set; }
    }
}