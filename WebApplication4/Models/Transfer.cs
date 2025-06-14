﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebApplication4.Models;

public partial class Transfer
{
    public int TransferId { get; set; }

    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public int? InvoiceId { get; set; }

    public DateTime? TransferDate { get; set; }

    public int? EmployeeId { get; set; }

    public string Notes { get; set; }

    public string Type { get; set; }

    public int UserId { get; set; }

    public virtual Employee Employee { get; set; }

    public virtual Cashandbankaccount FromAccount { get; set; }

    public virtual Invoice Invoice { get; set; }

    public virtual Cashandbankaccount ToAccount { get; set; }

    public virtual User User { get; set; }
}