using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ExchangesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ExchangesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Exchanges
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only expense transactions for the logged-in user
            var userExpenses = await _context.Exchanges
                .Where(e => e.ExchangeType == "صرف" && e.UserId == userId)
                .Include(e => e.Account)
                .Include(e => e.CostCenter)
                .Include(e => e.Customer)
                .Include(e => e.User)
                .Include(e => e.Vendor)
                .OrderByDescending(e => e.ExchangeDate) // Optional: order by date
                .ToListAsync();

            return View(userExpenses);
        }

        // GET: Exchanges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Exchanges == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchanges
                .Include(e => e.Account)
                .Include(e => e.CostCenter)
                .Include(e => e.Customer)
                .Include(e => e.User)
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.ExchangeId == id);
            if (exchange == null)
            {
                return NotFound();
            }

            return View(exchange);
        }
        // GET: Exchanges/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Fetch child accounts with their parent account names for grouping
            var childAccounts = _context.ChildAccounts
                .Join(_context.ParentAccounts,
                    ca => ca.ParentAccountId,
                    pa => pa.Id,
                    (ca, pa) => new { ca, pa })
                .Join(_context.AccountTypeMovements,
                    x => x.pa.AccountTypeId,
                    atm => atm.AccountTypeId,
                    (x, atm) => new { x.ca, x.pa, atm })
                .Where(x => (x.pa.AccountTypeId == 1 || x.pa.AccountTypeId == 5 || x.pa.AccountTypeId == 2) && // Assets, Expenses, Liabilities
                           x.ca.Id != 1 && x.ca.Id != 2 && // Exclude cash/bank accounts
                           (!_context.Childbalances.Any(cb => cb.ChildAccountId == x.ca.Id) ||
                            _context.Childbalances.Any(cb => cb.ChildAccountId == x.ca.Id && cb.UserId == userId)))
                .Select(x => new { x.ca.Id, x.ca.Name, ParentName = x.pa.Name })
                .ToList();

            var cashBankAccounts = _context.Cashandbankaccounts
                .Where(a => a.UserId == userId)
                .Select(a => new { a.AccountId, a.AccountName, a.AccountType })
                .ToList();

            // Use SelectList with grouping for child accounts
            ViewData["ChildAccountId"] = new SelectList(childAccounts.Any() ? childAccounts : new List<object>(), "Id", "Name", null, "ParentName");
            ViewData["AccountId"] = new SelectList(cashBankAccounts.Any() ? cashBankAccounts : new List<object>(), "AccountId", "AccountName");
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters
                .Where(c => c.UserId == userId), "CostCenterId", "CenterName");

            ViewBag.ExchangeType = "صرف";
            ViewBag.ExchangeDate = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }

        // POST: Exchanges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ExchangeDate,Amount,Description,PaymentMethod,CostCenterId,AccountId")] Exchange exchange, int[] ChildAccountIds, decimal[] ChildAccountAmounts)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            exchange.ExchangeType = "صرف";
            exchange.UserId = userId.Value;
            exchange.CreatedAt = DateTime.Now;

            // Make CostCenterId optional
            if (ModelState.ContainsKey("CostCenterId"))
            {
                ModelState["CostCenterId"].Errors.Clear();
                ModelState["CostCenterId"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            // Validate ChildAccountIds and ChildAccountAmounts
            if (ChildAccountIds == null || !ChildAccountIds.Any())
            {
                ModelState.AddModelError("", "يجب اختيار حساب فرعي واحد على الأقل.");
            }
            else if (ChildAccountIds.Length != ChildAccountAmounts.Length || Math.Abs(ChildAccountAmounts.Sum() - exchange.Amount) > 0.01m)
            {
                ModelState.AddModelError("", "يجب أن يتطابق مجموع مبالغ الحسابات الفرعية مع المبلغ الإجمالي.");
            }
            else if (ChildAccountAmounts.Any(a => a <= 0))
            {
                ModelState.AddModelError("", "جميع مبالغ الحسابات الفرعية يجب أن تكون أكبر من صفر.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Add(exchange);
                    await _context.SaveChangesAsync();

                    // Save child accounts to exchange_child_accounts
                    foreach (var (childAccountId, amount) in ChildAccountIds.Zip(ChildAccountAmounts, (id, amt) => (id, amt)))
                    {
                        var childAccountEntry = new ExchangeChildAccount
                        {
                            ExchangeId = exchange.ExchangeId,
                            ChildAccountId = childAccountId,
                            Amount = amount
                        };
                        _context.ExchangeChildAccounts.Add(childAccountEntry);
                    }

                    // Create credit journal entry for cash/bank account
                    var creditEntry = new JournalEntry
                    {
                        UserId = userId.Value,
                        ExchangeId = exchange.ExchangeId,
                        ChildAccountId = _context.Cashandbankaccounts
                            .Where(a => a.AccountId == exchange.AccountId)
                            .Select(a => a.ChildAccountId)
                            .FirstOrDefault(),
                        DebitAmount = 0,
                        CreditAmount = exchange.Amount,
                        Description = $"سند صرف: {exchange.Description}",
                        CreatedAt = DateTime.Now,
                        EntryDate = DateOnly.FromDateTime(exchange.ExchangeDate),
                        CostCenterId = exchange.CostCenterId
                    };
                    _context.JournalEntries.Add(creditEntry);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "حدث خطأ أثناء حفظ السند. حاول مرة أخرى.");
                }
            }

            var childAccounts = _context.ChildAccounts
                .Join(_context.ParentAccounts,
                    ca => ca.ParentAccountId,
                    pa => pa.Id,
                    (ca, pa) => new { ca, pa })
                .Join(_context.AccountTypeMovements,
                    x => x.pa.AccountTypeId,
                    atm => atm.AccountTypeId,
                    (x, atm) => new { x.ca, x.pa, atm })
                .Where(x => (x.pa.AccountTypeId == 1 || x.pa.AccountTypeId == 5 || x.pa.AccountTypeId == 2) && // Assets, Expenses, Liabilities
                           x.ca.Id != 1 && x.ca.Id != 2 && // Exclude cash/bank accounts
                           (!_context.Childbalances.Any(cb => cb.ChildAccountId == x.ca.Id) ||
                            _context.Childbalances.Any(cb => cb.ChildAccountId == x.ca.Id && cb.UserId == userId)))
                .Select(x => new { x.ca.Id, x.ca.Name, ParentName = x.pa.Name })
                .ToList();

            var cashBankAccounts = _context.Cashandbankaccounts
                .Where(a => a.UserId == userId)
                .Select(a => new { a.AccountId, a.AccountName, a.AccountType })
                .ToList();

            ViewData["ChildAccountId"] = new SelectList(childAccounts.Any() ? childAccounts : new List<object>(), "Id", "Name", null, "ParentName");
            ViewData["AccountId"] = new SelectList(cashBankAccounts.Any() ? cashBankAccounts : new List<object>(), "AccountId", "AccountName", exchange.AccountId);
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters
                .Where(c => c.UserId == userId), "CostCenterId", "CenterName", exchange.CostCenterId);

            return View(exchange);
        }

        // API to get cash/bank accounts based on PaymentMethod
        [HttpGet]
        public IActionResult GetCashBankAccounts(string paymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            var accountType = paymentMethod == "نقدي" ? "خزينة" : "حساب بنكي";
            var accounts = _context.Cashandbankaccounts
                .Where(a => a.UserId == userId && a.AccountType == accountType)
                .Select(a => new { a.AccountId, a.AccountName })
                .ToList();

            return Json(new { success = true, accounts });
        }
        // GET: Exchanges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Exchanges == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", exchange.AccountId);
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters, "CostCenterId", "CenterName", exchange.CostCenterId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", exchange.CustomerId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", exchange.UserId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "PhoneNumber1", exchange.VendorId);
            return View(exchange);
        }

        // POST: Exchanges/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ExchangeId,ExchangeDate,Amount,Description,PaymentMethod,ExchangeType,VendorId,CustomerId,CostCenterId,AccountId,UserId,CreatedAt")] Exchange exchange)
        {
            if (id != exchange.ExchangeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exchange);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExchangeExists(exchange.ExchangeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", exchange.AccountId);
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters, "CostCenterId", "CenterName", exchange.CostCenterId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", exchange.CustomerId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", exchange.UserId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "PhoneNumber1", exchange.VendorId);
            return View(exchange);
        }

        // GET: Exchanges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Exchanges == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchanges
                .Include(e => e.Account)
                .Include(e => e.CostCenter)
                .Include(e => e.Customer)
                .Include(e => e.User)
                .Include(e => e.Vendor)
                .FirstOrDefaultAsync(m => m.ExchangeId == id);
            if (exchange == null)
            {
                return NotFound();
            }

            return View(exchange);
        }

        // POST: Exchanges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Exchanges == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Exchanges'  is null.");
            }
            var exchange = await _context.Exchanges.FindAsync(id);
            if (exchange != null)
            {
                _context.Exchanges.Remove(exchange);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExchangeExists(int id)
        {
          return (_context.Exchanges?.Any(e => e.ExchangeId == id)).GetValueOrDefault();
        }









        // GET: Exchanges/CreateReceipt
        public IActionResult CreateReceipt()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            ViewData["AccountId"] = new SelectList(_context.Cashandbankaccounts.Where(a => a.UserId == userId), "AccountId", "AccountName");
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters.Where(c => c.UserId == userId), "CostCenterId", "CenterName");
            ViewData["VendorId"] = new SelectList(_context.Vendors.Where(v => v.UserId == userId), "VendorId", "VendorName");

            // Set default values for receipt
            ViewBag.ExchangeType = "قبض";
            ViewBag.ExchangeDate = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }



        //سند القبض
        // POST: Exchanges/CreateReceipt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceipt([Bind("ExchangeDate,Amount,Description,PaymentMethod,VendorId,CostCenterId,AccountId")] Exchange exchange)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Set receipt-specific values
            exchange.ExchangeType = "قبض";
            exchange.UserId = userId.Value;
            exchange.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(exchange);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            ViewData["AccountId"] = new SelectList(_context.Cashandbankaccounts.Where(a => a.UserId == userId), "AccountId", "AccountName", exchange.AccountId);
            ViewData["CostCenterId"] = new SelectList(_context.Costcenters.Where(c => c.UserId == userId), "CostCenterId", "CenterName", exchange.CostCenterId);
            ViewData["VendorId"] = new SelectList(_context.Vendors.Where(v => v.UserId == userId), "VendorId", "VendorName", exchange.VendorId);

            return View(exchange);
        }





        // GET: Exchanges
        public async Task<IActionResult> IndexReceipt()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only expense transactions for the logged-in user
            var userExpenses = await _context.Exchanges
                .Where(e => e.ExchangeType == "قبض" && e.UserId == userId)
                .Include(e => e.Account)
                .Include(e => e.CostCenter)
                .Include(e => e.Customer)
                .Include(e => e.User)
                .Include(e => e.Vendor)
                .OrderByDescending(e => e.ExchangeDate) // Optional: order by date
                .ToListAsync();

            return View(userExpenses);
        }
    }
}
