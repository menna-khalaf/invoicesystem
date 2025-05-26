using System;
namespace WebApplication4.Models;

public class AdditionPermissionViewModel
{
    public int StorehouseID { get; set; }  // Storehouse where items are added
    public int ProductID { get; set; }  // Product being added
    public int IncomingQuantity { get; set; }  // Quantity added
    public DateTime? DateOfPermission { get; set; }  // Nullable in case of future scheduling
    public int? CustomerID { get; set; }  // Nullable if not always required
    public decimal PurchasePrice { get; set; }  // Cost per unit
    public int Balance { get; set; }  // Stock balance after addition
    public decimal Total => IncomingQuantity * PurchasePrice;


    // Navigation Properties (if using EF Core)
    public virtual Storehouse Storehouse { get; set; }
    public virtual Product Product { get; set; }
    public virtual Customer Customer { get; set; }
}
