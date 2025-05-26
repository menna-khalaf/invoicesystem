using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ReportingFull : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ReportingFull(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }






        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }
            return View();
        }































        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }
            return View();
        }

        // 1. Monthly Sales Analysis
        public async Task<IActionResult> GetMonthlySales()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .GroupBy(i => new { Year = i.InvoiceDate.Year, Month = i.InvoiceDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalSales = g.Sum(i => i.TotalAmount)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();
            return Json(result);
        }

        // 2. Sales by Branch
        public async Task<IActionResult> GetSalesByBranch()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .Join(_context.Branches.Where(b => b.UserId == userId.Value),
                    i => i.BranchId,
                    b => b.BranchId,
                    (i, b) => new { i, b })
                .GroupBy(x => x.b.BranchName)
                .Select(g => new
                {
                    BranchName = g.Key,
                    TotalSales = g.Sum(x => x.i.TotalAmount)
                })
                .ToListAsync();
            return Json(result);
        }

        // 3. Least Active Customers
        public async Task<IActionResult> GetLeastActiveCustomers()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .Join(_context.Customers.Where(c => c.UserId == userId.Value),
                    i => i.CustomerId,
                    c => c.CustomerId,
                    (i, c) => new { i, c })
                .GroupBy(x => new { x.c.CustomerId, x.c.CustomerName })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    TotalPurchases = g.Sum(x => x.i.TotalAmount)
                })
                .Where(g => g.TotalPurchases < 1000)
                .OrderBy(g => g.TotalPurchases)
                .ToListAsync();
            return Json(result);
        }

        // 4. Purchases by Vendor
        public async Task<IActionResult> GetPurchasesByVendor()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "شراء" && i.UserId == userId.Value)
                .Join(_context.Vendors.Where(v => v.UserId == userId.Value),
                    i => i.VendorId,
                    v => v.VendorId,
                    (i, v) => new { i, v })
                .GroupBy(x => new { x.v.VendorId, x.v.VendorName })
                .Select(g => new
                {
                    VendorName = g.Key.VendorName,
                    TotalPurchases = g.Sum(x => x.i.TotalAmount)
                })
                .OrderByDescending(g => g.TotalPurchases)
                .ToListAsync();
            return Json(result);
        }

        // 5. Products Below Reorder Point
        public async Task<IActionResult> GetProductsBelowReorder()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Products
                .Where(p => p.UserId == userId.Value && p.Balance <= p.RepurchasePoint)
                .Select(p => new
                {
                    p.ProductName,
                    p.Balance,
                    p.RepurchasePoint
                })
                .ToListAsync();
            return Json(result);
        }

        // 6. Purchases by Branch
        public async Task<IActionResult> GetPurchasesByBranch()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "شراء" && i.UserId == userId.Value)
                .Join(_context.Branches.Where(b => b.UserId == userId.Value),
                    i => i.BranchId,
                    b => b.BranchId,
                    (i, b) => new { i, b })
                .GroupBy(x => new { x.b.BranchId, x.b.BranchName })
                .Select(g => new
                {
                    BranchName = g.Key.BranchName,
                    TotalPurchases = g.Sum(x => x.i.TotalAmount)
                })
                .ToListAsync();
            return Json(result);
        }

        // 7. Stagnant Products
        public async Task<IActionResult> GetStagnantProducts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var result = await _context.Products
                .Where(p => p.UserId == userId.Value &&
                            p.LastRestocked.HasValue &&
                            p.LastRestocked.Value.ToDateTime(TimeOnly.MinValue) < sixMonthsAgo)
                .Select(p => new
                {
                    p.ProductName,
                    p.LastRestocked
                })
                .ToListAsync();
            return Json(result);
        }

        // 8. Current Inventory
        public async Task<IActionResult> GetCurrentInventory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Products
                .Where(p => p.UserId == userId.Value)
                .Select(p => new
                {
                    p.ProductName,
                    p.Balance
                })
                .OrderBy(p => p.Balance)
                .ToListAsync();
            return Json(result);
        }

        // 9. Inventory Discrepancies
        public async Task<IActionResult> GetInventoryDiscrepancies()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Adjustmentproducts
                
                .Join(_context.Products.Where(p => p.UserId == userId.Value),
                    ap => ap.ProductId,
                    p => p.ProductId,
                    (ap, p) => new
                    {
                        p.ProductName,
                        ap.ActualQuantity,
                        ap.Balanced,
                        Difference = ap.ActualQuantity - ap.Balanced
                    })
                .ToListAsync();
            return Json(result);
        }

        // 10. Aging Report
        public async Task<IActionResult> GetAgingReport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Paymentdetails
                .Where(pd => pd.IsPaid == false && pd.UserId == userId.Value)
                .Join(_context.Customers,
                    pd => pd.UserId,
                    c => c.UserId,
                    (pd, c) => new
                    {
                        c.CustomerName,
                        pd.Amount,
                        DaysOverdue = EF.Functions.DateDiffDay(
                            pd.DueDate,
                            DateOnly.FromDateTime(DateTime.Now))
                    })
                .ToListAsync();
            return Json(result);
        }

        // 11. Customer Behavior
        public async Task<IActionResult> GetCustomerBehavior()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .Join(_context.Customers.Where(c => c.UserId == userId.Value),
                    i => i.CustomerId,
                    c => c.CustomerId,
                    (i, c) => new { i, c })
                .GroupBy(x => new { x.c.CustomerId, x.c.CustomerName })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    InvoiceCount = g.Count(),
                    TotalPurchases = g.Sum(x => x.i.TotalAmount)
                })
                .OrderByDescending(g => g.InvoiceCount)
                .ToListAsync();
            return Json(result);
        }

        // 12. Inactive Vendors
        public async Task<IActionResult> GetInactiveVendors()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var oneYearAgo = DateTime.Now.AddYears(-1);
            var result = await _context.Vendors
                .Where(v => v.UserId == userId.Value &&
                            !_context.Invoices
                                .Where(i => i.InvoiceType == "شراء" &&
                                            i.InvoiceDate > oneYearAgo &&
                                            i.UserId == userId.Value)
                                .Select(i => i.VendorId)
                                .Contains(v.VendorId))
                .Select(v => new { v.VendorName })
                .ToListAsync();
            return Json(result);
        }

        // 13. Balance Sheet
        public async Task<IActionResult> GetBalanceSheet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Childbalances
                .Where(cb => cb.UserId == userId.Value)
                .Join(_context.ChildAccounts.Where(ca => ca.UserId == userId.Value),
                    cb => cb.ChildAccountId,
                    ca => ca.Id,
                    (cb, ca) => new
                    {
                        AccountName = ca.Name,
                        cb.Balance
                    })
                .ToListAsync();
            return Json(result);
        }

        // 14. Income Statement
        public async Task<IActionResult> GetIncomeStatement()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Childbalances
                .Where(cb => cb.UserId == userId.Value)
                .Join(_context.ChildAccounts.Where(ca => ca.UserId == userId.Value),
                    cb => cb.ChildAccountId,
                    ca => ca.Id,
                    (cb, ca) => new { cb, ca })
                .Join(_context.ParentAccounts.Where(pa => pa.UserId == userId.Value),
                    x => x.ca.ParentAccountId,
                    pa => pa.Id,
                    (x, pa) => new { x.cb, x.ca, pa })
                .Join(_context.AccountTypes,
                    x => x.pa.AccountTypeId,
                    at => at.Id,
                    (x, at) => new
                    {
                        x.cb.Balance,
                        AccountType = at.Name
                    })
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalRevenue = g.Where(x => x.AccountType == "إيرادات").Sum(x => x.Balance),
                    TotalExpenses = g.Where(x => x.AccountType == "مصروفات").Sum(x => x.Balance),
                    NetProfit = g.Where(x => x.AccountType == "إيرادات").Sum(x => x.Balance) -
                                g.Where(x => x.AccountType == "مصروفات").Sum(x => x.Balance)
                })
                .FirstOrDefaultAsync();
            return Json(result);
        }

        // 15. Journal Entries
        public async Task<IActionResult> GetJournalEntries()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.JournalEntries
                .Where(je => je.UserId == userId.Value)
                .Select(je => new
                {
                    je.EntryDate,
                    je.Description,
                    je.DebitAmount,
                    je.CreditAmount
                })
                .OrderByDescending(je => je.EntryDate)
                .ToListAsync();
            return Json(result);
        }

        // 16. Subledger Transactions
        public async Task<IActionResult> GetSubledgerTransactions()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var childAccountId = 1; // Replace with dynamic parameter if needed
            var result = await _context.JournalEntries
                .Where(je => je.ChildAccountId == childAccountId && je.UserId == userId.Value)
                .Select(je => new
                {
                    je.EntryDate,
                    je.Description,
                    je.DebitAmount,
                    je.CreditAmount
                })
                .OrderBy(je => je.EntryDate)
                .ToListAsync();
            return Json(result);
        }

        // 17. Top Selling Employee
        public async Task<IActionResult> GetTopSellingEmployee()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .Join(_context.Employees.Where(e => e.UserId == userId.Value),
                    i => i.EmployeeId,
                    e => e.EmployeeId,
                    (i, e) => new { i, e })
                .GroupBy(x => new { x.e.EmployeeId, x.e.FullName })
                .Select(g => new
                {
                    FullName = g.Key.FullName,
                    TotalSales = g.Sum(x => x.i.TotalAmount)
                })
                .OrderByDescending(g => g.TotalSales)
                .Take(1)
                .ToListAsync();
            return Json(result);
        }

        // 18. Unpaid Invoices
        public async Task<IActionResult> GetUnpaidInvoices()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.Status == "غير مدفوعة" && i.UserId == userId.Value)
                .Select(i => new
                {
                    i.InvoiceId,
                    i.CustomerId,
                    i.TotalAmount,
                    i.Status
                })
                .ToListAsync();
            return Json(result);
        }

        // 19. Cash vs Bank Comparison
        public async Task<IActionResult> GetCashVsBank()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId.Value)
                .GroupBy(a => a.AccountType)
                .Select(g => new
                {
                    AccountType = g.Key,
                    TotalBalance = g.Sum(a => a.Balance)
                })
                .ToListAsync();
            return Json(result);
        }

        // 20. Payment Methods Analysis
        public async Task<IActionResult> GetPaymentMethodsAnalysis()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .GroupBy(i => i.PaymentMethod)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(i => i.TotalAmount)
                })
                .ToListAsync();
            return Json(result);
        }

        // 21. Total Revenue
        public async Task<IActionResult> GetTotalRevenue()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.Status == "مدفوعة" && i.UserId == userId.Value)
                .SumAsync(i => i.TotalAmount);
            return Json(new { TotalRevenue = result });
        }

        // 22. Total Expenses
        public async Task<IActionResult> GetTotalExpenses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Exchanges
                .Where(e => e.ExchangeType == "صرف" && e.UserId == userId.Value)
                .SumAsync(e => e.Amount);
            return Json(new { TotalExpenses = result });
        }

        // 23. Total Sales Returns
        public async Task<IActionResult> GetTotalSalesReturns()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "مرتجع من البيع" && i.UserId == userId.Value)
                .SumAsync(i => i.TotalAmount);
            return Json(new { TotalSalesReturns = result });
        }

        // 24. Total Purchase Returns
        public async Task<IActionResult> GetTotalPurchaseReturns()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "مرتجع من الشراء" && i.UserId == userId.Value)
                .SumAsync(i => i.TotalAmount);
            return Json(new { TotalPurchaseReturns = result });
        }

        // 25. Top Purchasing Customer
        public async Task<IActionResult> GetTopPurchasingCustomer()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "بيع" && i.UserId == userId.Value)
                .Join(_context.Customers.Where(c => c.UserId == userId.Value),
                    i => i.CustomerId,
                    c => c.CustomerId,
                    (i, c) => new { i, c })
                .GroupBy(x => new { x.c.CustomerId, x.c.CustomerName })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    TotalPurchases = g.Sum(x => x.i.TotalAmount)
                })
                .OrderByDescending(g => g.TotalPurchases)
                .Take(1)
                .ToListAsync();
            return Json(result);
        }

        // 26. Most Frequent Vendor
        public async Task<IActionResult> GetMostFrequentVendor()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoices
                .Where(i => i.InvoiceType == "شراء" && i.UserId == userId.Value)
                .Join(_context.Vendors.Where(v => v.UserId == userId.Value),
                    i => i.VendorId,
                    v => v.VendorId,
                    (i, v) => new { i, v })
                .GroupBy(x => new { x.v.VendorId, x.v.VendorName })
                .Select(g => new
                {
                    VendorName = g.Key.VendorName,
                    OrderCount = g.Count()
                })
                .OrderByDescending(g => g.OrderCount)
                .Take(1)
                .ToListAsync();
            return Json(result);
        }

        // 27. Most Sold Product
        public async Task<IActionResult> GetMostSoldProduct()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoicedetails
                .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                    d => d.InvoiceId,
                    i => i.InvoiceId,
                    (d, i) => new { d, i })
                .Where(x => x.i.InvoiceType == "بيع")
                .Join(_context.Products.Where(p => p.UserId == userId.Value),
                    x => x.d.ProductId,
                    p => p.ProductId,
                    (x, p) => new { x.d, p })
                .GroupBy(x => new { x.p.ProductId, x.p.ProductName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantitySold = g.Sum(x => x.d.Quantity)
                })
                .OrderByDescending(g => g.TotalQuantitySold)
                .Take(1)
                .ToListAsync();
            return Json(result);
        }

        // 28. Most Purchased Product
        public async Task<IActionResult> GetMostPurchasedProduct()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = await _context.Invoicedetails
                .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                    d => d.InvoiceId,
                    i => i.InvoiceId,
                    (d, i) => new { d, i })
                .Where(x => x.i.InvoiceType == "شراء")
                .Join(_context.Products.Where(p => p.UserId == userId.Value),
                    x => x.d.ProductId,
                    p => p.ProductId,
                    (x, p) => new { x.d, p })
                .GroupBy(x => new { x.p.ProductId, x.p.ProductName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantityPurchased = g.Sum(x => x.d.Quantity)
                })
                .OrderByDescending(g => g.TotalQuantityPurchased)
                .Take(1)
                .ToListAsync();
            return Json(result);
        }

        // 29. Invoice Totals
        public async Task<IActionResult> GetInvoiceTotals()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            var result = new
            {
                إجمالي_البيع = await _context.Invoicedetails
                    .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                        d => d.InvoiceId,
                        i => i.InvoiceId,
                        (d, i) => new { d, i })
                    .Where(x => x.d.InvoiceTyping == "بيع")
                    .SumAsync(x => (decimal?)x.d.LineTotal) ?? 0,
                إجمالي_الشراء = await _context.Invoicedetails
                    .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                        d => d.InvoiceId,
                        i => i.InvoiceId,
                        (d, i) => new { d, i })
                    .Where(x => x.d.InvoiceTyping == "شراء")
                    .SumAsync(x => (decimal?)x.d.LineTotal) ?? 0,
                إجمالي_مرتجع_البيع = await _context.Invoicedetails
                    .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                        d => d.InvoiceId,
                        i => i.InvoiceId,
                        (d, i) => new { d, i })
                    .Where(x => x.d.InvoiceTyping == "مرتجع من البيع")
                    .SumAsync(x => (decimal?)x.d.LineTotal) ?? 0,
                إجمالي_مرتجع_الشراء = await _context.Invoicedetails
                    .Join(_context.Invoices.Where(i => i.UserId == userId.Value),
                        d => d.InvoiceId,
                        i => i.InvoiceId,
                        (d, i) => new { d, i })
                    .Where(x => x.d.InvoiceTyping == "مرتجع من الشراء")
                    .SumAsync(x => (decimal?)x.d.LineTotal) ?? 0
            };

            return Json(result);
        }
    }
}
