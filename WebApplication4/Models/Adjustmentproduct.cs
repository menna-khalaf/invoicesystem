﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebApplication4.Models;

public partial class Adjustmentproduct
{
    public int AdjustmentProductId { get; set; }

    public int? AdjustmentId { get; set; }

    public int? ProductId { get; set; }

    public int? ActualQuantity { get; set; }

    public int? Difference { get; set; }

    public int? Balanced { get; set; }

    public int? InventoryId { get; set; }

    public virtual Adjustment Adjustment { get; set; }

    public virtual Inventory Inventory { get; set; }

    public virtual Product Product { get; set; }
}