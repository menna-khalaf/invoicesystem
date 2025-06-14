﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace WebApplication4.Models;

public partial class ParentAccount
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int AccountTypeId { get; set; }

    public int? UserId { get; set; }

    public virtual AccountType AccountType { get; set; }

    public virtual ICollection<ChildAccount> ChildAccounts { get; set; } = new List<ChildAccount>();

    public virtual User User { get; set; }
}