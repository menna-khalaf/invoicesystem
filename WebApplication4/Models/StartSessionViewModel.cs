// Models/StartSessionViewModel.cs
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace WebApplication4.Models
{
    public class StartSessionViewModel
    {
        public int EmployeeId { get; set; }
        public int POSId { get; set; }
        public decimal StartingCash { get; set; }
        public string Notes { get; set; }
        public DateTime StartTime { get; set; }
        [BindNever] // Add this attribute
        public List<SelectListItem> POSList { get; set; }
    }
}