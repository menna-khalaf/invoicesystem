using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication4.Models;

public partial class Exchange
{
    public int ExchangeId { get; set; }

    [Required(ErrorMessage = "تاريخ السند مطلوب.")]
    public DateTime ExchangeDate { get; set; }

    [Required(ErrorMessage = "المبلغ مطلوب.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "الوصف مطلوب.")]
    [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "طريقة الدفع مطلوبة.")]
    [StringLength(100, ErrorMessage = "طريقة الدفع يجب ألا تتجاوز 100 حرف.")]
    public string PaymentMethod { get; set; }

    [Required(ErrorMessage = "نوع السند مطلوب.")]
    [StringLength(50, ErrorMessage = "نوع السند يجب ألا يتجاوز 50 حرف.")]
    public string ExchangeType { get; set; }

    public int? VendorId { get; set; }

    public int? CustomerId { get; set; }

    public int? CostCenterId { get; set; }

    [Required(ErrorMessage = "معرف الحساب مطلوب.")]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "معرف المستخدم مطلوب.")]
    public int UserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public virtual Cashandbankaccount Account { get; set; }

    public virtual Costcenter? CostCenter { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Vendor? Vendor { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<ExchangeChildAccount> ExchangeChildAccounts { get; set; } = new List<ExchangeChildAccount>();

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}