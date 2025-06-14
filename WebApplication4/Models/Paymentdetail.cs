﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebApplication4.Models;

public partial class Paymentdetail
{
    public int PaydetailId { get; set; }

    public int Payid { get; set; }

    public int UserId { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? IsPaid { get; set; }

    public DateOnly? PaidDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Payment Pay { get; set; }

    public virtual User User { get; set; }
}