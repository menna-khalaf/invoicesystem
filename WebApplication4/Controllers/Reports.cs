using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ReportsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ReportsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Reportincoming()
        {
            var reportData = _context.Products
                .Join(
                    _context.Invoicedetails,
                    p => p.ProductId,
                    id => id.ProductId,
                    (p, id) => new { p, id }
                )
                .Join(
                    _context.Invoices,
                    temp => temp.id.InvoiceId,
                    i => i.InvoiceId,
                    (temp, i) => new { temp.p, temp.id, i }
                )
                .Where(x => x.i.InvoiceType == "شراء" || x.i.InvoiceType == "مرتجع من الشراء")
                .GroupBy(x => new { x.p.ProductId, x.p.ProductName })
                .Select(g => new ProductSummary
                {
                    ProductID = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalPurchasedQuantity = g.Sum(x => x.id.InvoiceTyping == "شراء" ? x.id.Quantity : 0),
                    TotalReturnedQuantity = g.Sum(x => x.id.InvoiceTyping == "مرتجع من الشراء" ? x.id.Quantity : 0)
                })
                .ToList();

            return View(reportData);
        }

        public IActionResult Reportoutgoing()
        {
            var reportData = _context.Products
                .Join(
                    _context.Invoicedetails,
                    p => p.ProductId,
                    id => id.ProductId,
                    (p, id) => new { p, id }
                )
                .Join(
                    _context.Invoices,
                    temp => temp.id.InvoiceId,
                    i => i.InvoiceId,
                    (temp, i) => new { temp.p, temp.id, i }
                )
                .Where(x => x.i.InvoiceType == "بيع" || x.i.InvoiceType == "مرتجع من البيع")
                .GroupBy(x => new { x.p.ProductId, x.p.ProductName })
                .Select(g => new ProductSummary
                {
                    ProductID = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalPurchasedQuantity = g.Sum(x => x.id.InvoiceTyping == "بيع" ? x.id.Quantity : 0),
                    TotalReturnedQuantity = g.Sum(x => x.id.InvoiceTyping == "مرتجع من البيع" ? x.id.Quantity : 0)
                })
                .ToList();

            return View(reportData);
        }

        public async Task<IActionResult> BalanceSheet()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0; // Adjusted to match your query's UserID = 10

            try
            {
                // Fetch report items including all specified account types
                var reportItems = await _context.Childbalances
                    .Where(cb => cb.UserId == userId)
                    .Join(
                        _context.ChildAccounts,
                        cb => cb.ChildAccountId,
                        ca => ca.Id,
                        (cb, ca) => new { cb, ca }
                    )
                    .Join(
                        _context.ParentAccounts,
                        x => x.ca.ParentAccountId,
                        pa => pa.Id,
                        (x, pa) => new { x.cb, x.ca, pa }
                    )
                    .Join(
                        _context.AccountTypes,
                        x => x.pa.AccountTypeId,
                        at => at.Id,
                        (x, at) => new { x.cb, x.ca, x.pa, at }
                    )
                    .Where(x => new[] { "أصول", "خصوم", "حقوق ملكية", "مصروفات" }.Contains(x.at.Name))
                    .GroupBy(x => new { AccountTypeName = x.at.Name, ChildAccountName = x.ca.Name })
                    .Select(g => new BalanceSheetReportItem
                    {
                        AccountType = g.Key.AccountTypeName,
                        ChildAccountName = g.Key.ChildAccountName,
                        Balance = g.Sum(x => x.cb.Balance)
                    })
                    .OrderBy(x => x.AccountType == "أصول" ? 1 : x.AccountType == "خصوم" ? 2 :
                               x.AccountType == "حقوق ملكية" ? 3 : x.AccountType == "مصروفات" ? 4 : 5)
                    .ThenByDescending(x => x.Balance)
                    .ToListAsync();

                // Calculate balance sheet totals (excluding Expenses)
                var totalAssets = reportItems
                    .Where(x => x.AccountType == "أصول")
                    .Sum(x => x.Balance);

                var totalLiabilities = reportItems
                    .Where(x => x.AccountType == "خصوم")
                    .Sum(x => x.Balance);

                var totalEquity = reportItems
                    .Where(x => x.AccountType == "حقوق ملكية")
                    .Sum(x => x.Balance);

                // Check if the balance sheet balances (Assets = Liabilities + Equity)
                var isBalanced = Math.Abs(totalAssets - (totalLiabilities + totalEquity)) < 0.01m;

                // Prepare view model with current date
                var viewModel = new BalanceSheetReportViewModel
                {
                    ReportItems = reportItems,
                    UserId = userId,
                    AsOfDate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAssets = totalAssets,
                    TotalLiabilities = totalLiabilities,
                    TotalEquity = totalEquity,
                    TotalLiabilitiesAndEquity = totalLiabilities + totalEquity,
                    IsBalanced = isBalanced
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
        // GET: Reports/IncomeExpense
        public async Task<IActionResult> IncomeExpense()
        {
            // Get UserID from session, default to 2
            var userId = HttpContext.Session.GetInt32("UserId") ?? 2;

            // LINQ query to fetch income and expense report
            var reportItems = await _context.Childbalances
                .Where(cb => cb.UserId == userId)
                .Join(
                    _context.ChildAccounts
                        .Where(ca => ca.UserId == userId || ca.UserId == null),
                    cb => cb.ChildAccountId,
                    ca => ca.Id,
                    (cb, ca) => new { cb, ca }
                )
                .Join(
                    _context.ParentAccounts,
                    x => x.ca.ParentAccountId,
                    pa => pa.Id,
                    (x, pa) => new { x.cb, x.ca, pa }
                )
                .Join(
                    _context.AccountTypes
                        .Where(at => at.Name == "إيرادات" || at.Name == "مصروفات"),
                    x => x.pa.AccountTypeId,
                    at => at.Id,
                    (x, at) => new { x.cb, x.ca, x.pa, at }
                )
                .Join(
                    _context.AccountTypeMovements,
                    x => x.at.Id,
                    atm => atm.AccountTypeId,
                    (x, atm) => new
                    {
                        AccountType = x.at.Name,
                        ChildAccountName = x.ca.Name,
                        Balance = x.cb.Balance
                    }
                )
                .GroupBy(x => new { x.AccountType, x.ChildAccountName })
                .Select(g => new IncomeExpenseReportItem
                {
                    AccountType = g.Key.AccountType,
                    ChildAccountName = g.Key.ChildAccountName,
                    Total = g.Sum(x => x.Balance)
                })
                .OrderBy(x => x.AccountType)
                .ThenByDescending(x => x.Total)
                .ToListAsync();

            // Prepare view model
            var viewModel = new IncomeExpenseReportViewModel
            {
                ReportItems = reportItems,
                UserId = userId
            };

            return View(viewModel);
        }
    }

    // View model for the report
    public class IncomeExpenseReportViewModel
    {
        public List<IncomeExpenseReportItem> ReportItems { get; set; }
        public int UserId { get; set; }
    }

    // Data model for each report row
    public class IncomeExpenseReportItem
    {
        public string AccountType { get; set; }
        public string ChildAccountName { get; set; }
        public decimal Total { get; set; }
    }
}