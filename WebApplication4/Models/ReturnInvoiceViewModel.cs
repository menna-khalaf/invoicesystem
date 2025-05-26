using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WebApplication4.Models
{
    public class ReturnInvoiceViewModel
    {
        public Invoice OriginalInvoice { get; set; }
        public List<Invoicedetail> InvoiceItems { get; set; }
        public List<ReturnItem> ReturnItems { get; set; }
        public decimal TaxRate { get; set; } = 0.05m; // 5% tax rate based on previous context (e.g., 5.00 VAT on 100.00)

        public Payment OriginalPayment { get; set; }
        public List<Paymentdetail> OriginalPaymentDetails { get; set; }

        public List<PaymentDetailViewModel> PaymentDetails { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "الكمية المرتجعة يجب أن تكون أكبر من الصفر")]
        public Dictionary<int, decimal> ReturnQuantities { get; set; } = new Dictionary<int, decimal>();
    }

    public class ReturnItem
    {
        public int InvoiceDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal Price { get; set; }
        public Product Product { get; set; }
        public decimal RemainingQuantity { get; set; } // Added property

        public decimal VatRate => Product?.Vatrate ?? 5m;
        public decimal SubTotal => ReturnedQuantity * Price;
        public decimal VatAmount => SubTotal * (VatRate / 100);
        public decimal Total => SubTotal + VatAmount;
    }

    public class PaymentDetailViewModel
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateOnly? DueDate { get; set; }
    }
}