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
    public class TransfersController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public TransfersController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Transfers
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.Transfers.Include(t => t.Employee).Include(t => t.FromAccount).Include(t => t.Invoice).Include(t => t.ToAccount).Include(t => t.User);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Transfers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transfers == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers
                .Include(t => t.Employee)
                .Include(t => t.FromAccount)
                .Include(t => t.Invoice)
                .Include(t => t.ToAccount)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransferId == id);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }




        [HttpGet]
        public IActionResult GetAccountsByType(string type, int userId)
        {
            var accounts = _context.Cashandbankaccounts
                .Where(a => a.AccountType == type && a.UserId == userId)
                .Select(a => new
                {
                    accountId = a.AccountId,
                    accountName = a.AccountName
                })
                .ToList();

            return Json(accounts);
        }











        // GET: Transfers/Create
        public IActionResult Create()
        {
            // Try to get session ID from ASP.NET session
            int? sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");

            if (!sessionId.HasValue)
            {
                ModelState.AddModelError("", "Session ID is required.");
                return View();
            }

            // Retrieve UserId from session
            int? userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Retrieved UserId from session: {userId}");

            if (!userId.HasValue)
            {
                ModelState.AddModelError("", "User ID is required.");
                return View();
            }

            // Retrieve BankAccountId and CashAccountId from session
            int? bankAccountId = HttpContext.Session.GetInt32("BankAccountId");
            Console.WriteLine($"Retrieved BankAccountId from session: {bankAccountId}");

            int? cashAccountId = HttpContext.Session.GetInt32("CashAccountId");
            Console.WriteLine($"Retrieved CashAccountId from session: {cashAccountId}");

            if (!bankAccountId.HasValue || !cashAccountId.HasValue)
            {
                ModelState.AddModelError("", "Bank Account ID or Cash Account ID is missing from session.");
                return View();
            }

            // Retrieve invoices filtered by session ID
            var invoices = _context.Invoices
                .Where(i => i.SessionId == sessionId)
                .Select(i => new { i.InvoiceId })
                .ToList();

            if (!invoices.Any())
            {
                ModelState.AddModelError("", "No invoices found for the current session.");
                return View();
            }

            // Set default InvoiceId (e.g., first invoice)
            ViewBag.DefaultInvoiceId = invoices.First().InvoiceId;

            // Set default UserId from session
            ViewBag.DefaultUserId = userId;

            // Retrieve only the two accounts for FromAccountId
            var fromAccounts = _context.Cashandbankaccounts
                .Where(a => a.AccountId == bankAccountId || a.AccountId == cashAccountId)
                .Select(a => new { a.AccountId, a.AccountName })
                .ToList();

            if (!fromAccounts.Any())
            {
                ModelState.AddModelError("", "No matching bank or cash accounts found.");
                return View();
            }

            // Populate ViewData for dropdowns
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName");
            ViewData["FromAccountId"] = new SelectList(fromAccounts, "AccountId", "AccountName");
            ViewData["ToAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName");

            return View();
        }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("TransferId,FromAccountId,ToAccountId,Amount,InvoiceId,Type,TransferDate,EmployeeId,UserId")] Transfer transfer)
{
    // Try to get session ID from ASP.NET session
    int? sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");

    if (!sessionId.HasValue)
    {
        ModelState.AddModelError("", "Session ID is required.");
    }
    else
    {
        // Validate that the provided InvoiceId belongs to the session
        bool isValidInvoice = await _context.Invoices
            .AnyAsync(i => i.InvoiceId == transfer.InvoiceId && i.SessionId == sessionId);

        if (!isValidInvoice)
        {
            ModelState.AddModelError("InvoiceId", "The selected Invoice ID is not associated with the current session.");
        }
    }

    // Validate UserId matches session
    int? userId = HttpContext.Session.GetInt32("UserId");
    if (!userId.HasValue || transfer.UserId != userId)
    {
        ModelState.AddModelError("UserId", "The provided User ID does not match the current session.");
    }

    if (ModelState.IsValid)
    {
        var fromAccount = await _context.Cashandbankaccounts
            .FirstOrDefaultAsync(a => a.AccountId == transfer.FromAccountId);
        var toAccount = await _context.Cashandbankaccounts
            .FirstOrDefaultAsync(a => a.AccountId == transfer.ToAccountId);

        if (fromAccount == null || toAccount == null)
        {
            ModelState.AddModelError(string.Empty, "لم يتم العثور على أحد الحسابات المحددة.");
        }
        else
        {
            // تحديث الرصيد حسب نوع العملية
            if (transfer.Type == "اضافة")
            {
                fromAccount.Balance += transfer.Amount;
                toAccount.Balance -= transfer.Amount;
            }
            else if (transfer.Type == "صرف")
            {
                fromAccount.Balance -= transfer.Amount;
                toAccount.Balance += transfer.Amount;
            }
            else
            {
                ModelState.AddModelError("Type", "نوع التحويل غير صالح. يجب أن يكون 'إضافة' أو 'صرف'.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // تحديث الحسابات
                    _context.Cashandbankaccounts.Update(fromAccount);
                    _context.Cashandbankaccounts.Update(toAccount);

                    // إضافة التحويل
                    _context.Transfers.Add(transfer);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء حفظ البيانات: " + ex.Message);
                }
            }
        }
    }

    // تحميل القوائم المنسدلة مرة أخرى
    if (sessionId.HasValue)
    {
        var invoices = _context.Invoices
            .Where(i => i.SessionId == sessionId)
            .Select(i => new { i.InvoiceId })
            .ToList();
        ViewBag.DefaultInvoiceId = transfer.InvoiceId; // Preserve the submitted InvoiceId
    }
    else
    {
        ViewBag.DefaultInvoiceId = null;
    }

    ViewBag.DefaultUserId = userId; // Preserve the session UserId

    // Retrieve accounts for FromAccountId
    int? bankAccountId = HttpContext.Session.GetInt32("BankAccountId");
    int? cashAccountId = HttpContext.Session.GetInt32("CashAccountId");
    var fromAccounts = _context.Cashandbankaccounts
        .Where(a => a.AccountId == bankAccountId || a.AccountId == cashAccountId)
        .Select(a => new { a.AccountId, a.AccountName })
        .ToList();

    ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", transfer.EmployeeId);
    ViewData["FromAccountId"] = new SelectList(fromAccounts, "AccountId", "AccountName", transfer.FromAccountId);
    ViewData["ToAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", transfer.ToAccountId);

    return View(transfer);
}


        // GET: Transfers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Transfers == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", transfer.EmployeeId);
            ViewData["FromAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", transfer.FromAccountId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", transfer.InvoiceId);
            ViewData["ToAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", transfer.ToAccountId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", transfer.UserId);
            return View(transfer);
        }

        // POST: Transfers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransferId,FromAccountId,ToAccountId,Amount,InvoiceId,Type,TransferDate,EmployeeId,Notes,UserId")] Transfer transfer)
        {
            if (id != transfer.TransferId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transfer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransferExists(transfer.TransferId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", transfer.EmployeeId);
            ViewData["FromAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", transfer.FromAccountId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", transfer.InvoiceId);
            ViewData["ToAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", transfer.ToAccountId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", transfer.UserId);
            return View(transfer);
        }

        // GET: Transfers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transfers == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers
                .Include(t => t.Employee)
                .Include(t => t.FromAccount)
                .Include(t => t.Invoice)
                .Include(t => t.ToAccount)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransferId == id);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // POST: Transfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transfers == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Transfers'  is null.");
            }
            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer != null)
            {
                _context.Transfers.Remove(transfer);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransferExists(int id)
        {
          return (_context.Transfers?.Any(e => e.TransferId == id)).GetValueOrDefault();
        }
    }
}
