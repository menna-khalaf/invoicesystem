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
    public class VendorsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public VendorsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Vendors
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var invoiceSystemrahtkContext = _context.Vendors
                .Where(v => v.UserId == userId.Value)
                .Include(v => v.Employee)
                .Include(v => v.User);

            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Vendors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .Include(v => v.Employee)
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }
        // GET: Vendors/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Filter employees by the logged-in user's ID
            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName");

            ViewData["UserId"] = userId.Value;

            // Initialize a new Vendor with default TaxableCheck value
            var vendor = new Vendor { TaxableCheck = true };
            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VendorId,VendorName,PhoneNumber1,PhoneNumber2,Email,TaxNumber,TaxableCheck,Classification,CommercialRegister,Location,Currency,CreditLimit,EmployeeId,CreatedAt")] Vendor vendor)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "usered");
            }

            // Set the UserId from session to the vendor object
            vendor.UserId = userId.Value;

            // Set default value for TaxableCheck if not provided
            vendor.TaxableCheck ??= false;

            if (ModelState.IsValid)
            {
                _context.Add(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Filter employees by the logged-in user's ID when returning to view with errors
            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName", vendor.EmployeeId);
            ViewData["UserId"] = userId.Value;

            return View(vendor);
        }

        // GET: Vendors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            // Verify the vendor belongs to the logged-in user
            if (vendor.UserId != userId)
            {
                return Forbid(); // or return NotFound() if you prefer to hide its existence
            }

            // Filter employees by the logged-in user's ID
            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName", vendor.EmployeeId);
            ViewData["CurrentUserId"] = userId.Value;

            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VendorId,VendorName,PhoneNumber1,PhoneNumber2,Email,TaxNumber,TaxableCheck,Classification,CommercialRegister,Location,Currency,CreditLimit,EmployeeId,CreatedAt")] Vendor vendor)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id != vendor.VendorId)
            {
                return NotFound();
            }

            // Get the original vendor to verify ownership and preserve UserId
            var originalVendor = await _context.Vendors.AsNoTracking()
                .FirstOrDefaultAsync(v => v.VendorId == id);

            if (originalVendor == null)
            {
                return NotFound();
            }

            // Verify the vendor belongs to the logged-in user
            if (originalVendor.UserId != userId)
            {
                return Forbid(); // or return NotFound() if you prefer to hide its existence
            }

            // Set the UserId from the original record
            vendor.UserId = originalVendor.UserId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vendor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.VendorId))
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

            // Filter employees by the logged-in user's ID when returning to view with errors
            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName", vendor.EmployeeId);
            ViewData["CurrentUserId"] = userId.Value;
            return View(vendor);
        }

        // GET: Vendors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vendors == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .Include(v => v.Employee)
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.VendorId == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }


        // POST: Vendors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vendors == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Vendors'  is null.");
            }
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VendorExists(int id)
        {
            return (_context.Vendors?.Any(e => e.VendorId == id)).GetValueOrDefault();
        }
















        public async Task<IActionResult> VendorDeferredPayments(int? vendorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (!vendorId.HasValue)
            {
                return BadRequest("Vendor ID is required");
            }

            try
            {
                var query = _context.Payments
                    .Where(p => p.UserId == userId && p.InvoiceId != null)
                    .Join(_context.Invoices,
                        p => p.InvoiceId,
                        i => i.InvoiceId,
                        (p, i) => new { Payment = p, Invoice = i })
                    .Where(joined => joined.Invoice.VendorId == vendorId && joined.Invoice.CustomerId == null)
                    .Join(_context.Paymentdetails,
                        joined => joined.Payment.Payid,
                        pd => pd.Payid,
                        (joined, pd) => new { joined.Payment, joined.Invoice, PaymentDetail = pd })
                    .Where(joined => joined.PaymentDetail.Description.Contains("آجل"));

                var deferredPayments = await query
                    .Select(joined => new DeferredPaymentModel
                    {
                        PayId = joined.Payment.Payid,
                        VendorId = joined.Invoice.VendorId,
                        UserId = joined.Payment.UserId,
                        PayDate = joined.Payment.Paydate,
                        PaymentUpdatedAt = joined.Payment.UpdatedAt,
                        PayDetailId = joined.PaymentDetail.PaydetailId,
                        DetailUserId = joined.PaymentDetail.UserId,
                        Description = joined.PaymentDetail.Description,
                        DetailAmount = joined.PaymentDetail.Amount,
                        DueDate = joined.PaymentDetail.DueDate,
                        IsPaid = joined.PaymentDetail.IsPaid,
                        PaidDate = joined.PaymentDetail.PaidDate,
                        DetailUpdatedAt = joined.PaymentDetail.UpdatedAt
                    })
                    .OrderByDescending(p => p.PayDate)
                    .ThenBy(p => p.DueDate)
                    .ToListAsync();

                ViewBag.VendorId = vendorId;
                return View(deferredPayments);
            }
            catch (Exception ex)
            {
                return Problem($"Error retrieving vendor deferred payments: {ex.Message}");
            }
        }










        public async Task<IActionResult> ProcessVendorPayment(int payId, int payDetailId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Fetch payment detail first
            var paymentDetail = await _context.Paymentdetails
                .FirstOrDefaultAsync(pd => pd.PaydetailId == payDetailId);

            if (paymentDetail == null)
            {
                return NotFound("تفاصيل الدفع غير موجودة");
            }

            // Log for debugging
            if (paymentDetail.Amount == 0)
            {
                Console.WriteLine($"Warning: PaymentDetail ID {payDetailId} has Amount = 0");
            }

            // Fetch payment with related vendor data
            var payment = await _context.Payments
                .Join(_context.Paymentdetails,
                    p => p.Payid,
                    pd => pd.Payid,
                    (p, pd) => new { Payment = p, PaymentDetail = pd })
                .Join(_context.Invoices,
                    joined => joined.Payment.InvoiceId,
                    i => i.InvoiceId,
                    (joined, i) => new { joined.Payment, joined.PaymentDetail, Invoice = i })
                .Join(_context.Vendors,
                    joined => joined.Invoice.VendorId,
                    v => v.VendorId,
                    (joined, v) => new { joined.Payment, joined.PaymentDetail, joined.Invoice, Vendor = v })
                .Where(joined => joined.Payment.Payid == paymentDetail.Payid
                    && joined.PaymentDetail.PaydetailId == payDetailId
                    && joined.Invoice.VendorId != null
                    && joined.Invoice.CustomerId == null)
                .Select(joined => new ProcessPaymentViewModel
                {
                    PayId = joined.Payment.Payid,
                    PayDetailId = joined.PaymentDetail.PaydetailId,
                    CustomerName = joined.Vendor.VendorName,
                    PaymentDate = DateTime.Now,
                    Amount = joined.PaymentDetail.Amount != 0 ? joined.PaymentDetail.Amount : paymentDetail.Amount, // Fallback
                    PaymentMethods = new List<PaymentMethodModel>()
                })
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                Console.WriteLine($"No payment found for PayId {payId}, PayDetailId {payDetailId}");
                return NotFound("الدفع غير موجود");
            }

            // Fetch accounts
            payment.Accounts = await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId)
                .Select(a => new CashAndBankAccount
                {
                    AccountID = a.AccountId,
                    AccountName = a.AccountName
                })
                .ToListAsync();

            // Set ViewBag.VendorId for the cancel button
            ViewBag.VendorId = await _context.Payments
                .Where(p => p.Payid == payId)
                .Join(_context.Invoices, p => p.InvoiceId, i => i.InvoiceId, (p, i) => i.VendorId)
                .FirstOrDefaultAsync();

            return View(payment); // Pass the payment model to ProcessVendorPayment.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> ProcessVendorPayment(ProcessPaymentViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                Console.WriteLine("UserId is null. Session may have expired.");
                return Json(new { success = false, message = "جلسة المستخدم منتهية. يرجى تسجيل الدخول.", redirectUrl = Url.Action("Login", "Usered") });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("ModelState invalid. Errors: " + string.Join("; ", errors));
                return Json(new { success = false, message = "بيانات النموذج غير صالحة: " + string.Join("; ", errors) });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"Starting payment processing for PayDetailId: {model.PayDetailId}, PayId: {model.PayId}");
                var paymentDetail = await _context.Paymentdetails
                    .FirstOrDefaultAsync(pd => pd.PaydetailId == model.PayDetailId);
                if (paymentDetail == null)
                {
                    Console.WriteLine($"Payment detail not found for PayDetailId: {model.PayDetailId}");
                    return Json(new { success = false, message = "لم يتم العثور على تفاصيل الدفع" });
                }

                decimal totalPaymentAmount = model.PaymentMethods.Sum(pm => pm.PaymentAmount ?? 0);
                if (Math.Abs(totalPaymentAmount - paymentDetail.Amount) > 0.01m)
                {
                    Console.WriteLine($"Total payment amount {totalPaymentAmount} does not match PaymentDetail amount {paymentDetail.Amount}");
                    return Json(new { success = false, message = "مجموع مبالغ طرق الدفع لا يساوي المبلغ الأصلي" });
                }

                // Fetch payment info with invoice and vendor details
                var paymentInfo = await _context.Payments
                    .Where(p => p.Payid == model.PayId)
                    .Join(_context.Invoices,
                        p => p.InvoiceId,
                        i => i.InvoiceId,
                        (p, i) => new { p.InvoiceId, i.VendorId })
                    .Join(_context.Vendors,
                        joined => joined.VendorId,
                        v => v.VendorId,
                        (joined, v) => new { joined.InvoiceId, joined.VendorId, VendorName = v.VendorName })
                    .FirstOrDefaultAsync();

                if (paymentInfo == null)
                {
                    Console.WriteLine($"Vendor or invoice information not found for PayId: {model.PayId}");
                    return Json(new { success = false, message = "لم يتم العثور على معلومات الفاتورة أو المورد" });
                }

                // Update the original payment detail
                paymentDetail.IsPaid = true;
                paymentDetail.PaidDate = DateOnly.FromDateTime(model.PaymentDate);
                paymentDetail.UpdatedAt = DateTime.Now;

                foreach (var pm in model.PaymentMethods)
                {
                    if (string.IsNullOrEmpty(pm.PaymentMethod))
                    {
                        Console.WriteLine($"Invalid payment method for PayDetailId: {model.PayDetailId}");
                        return Json(new { success = false, message = "طريقة الدفع غير صالحة" });
                    }

                    // Create new payment detail
                    var newPaymentDetail = new Paymentdetail
                    {
                        Payid = model.PayId,
                        UserId = userId.Value,
                        Description = pm.PaymentMethod,
                        Amount = pm.PaymentAmount ?? 0,
                        DueDate = pm.PaymentMethod == "اجل (مدفوع)" && pm.DueDate.HasValue
                            ? DateOnly.FromDateTime(pm.DueDate.Value)
                            : null,
                        IsPaid = true,
                        PaidDate = DateOnly.FromDateTime(model.PaymentDate),
                        UpdatedAt = DateTime.Now
                    };
                    _context.Paymentdetails.Add(newPaymentDetail);

                    // Get child_account_id and account name for credit entry
                    int? creditChildAccountId = null;
                    string creditDescription = $"دفعة مورد: {paymentInfo.VendorName} - فاتورة #{paymentInfo.InvoiceId} - ";
                    if (pm.PaymentMethod == "كاش" || pm.PaymentMethod == "تحويل بنكي")
                    {
                        if (!pm.AccountId.HasValue)
                        {
                            Console.WriteLine($"No AccountId provided for PaymentMethod: {pm.PaymentMethod}");
                            return Json(new { success = false, message = $"يجب اختيار حساب لطريقة الدفع: {pm.PaymentMethod}" });
                        }

                        var account = await _context.Cashandbankaccounts
                            .FirstOrDefaultAsync(a => a.AccountId == pm.AccountId.Value && a.UserId == userId);
                        if (account == null)
                        {
                            Console.WriteLine($"Account not found for AccountId: {pm.AccountId}, PaymentMethod: {pm.PaymentMethod}");
                            return Json(new { success = false, message = $"الحساب المحدد غير موجود أو لا ينتمي إلى المستخدم لطريقة الدفع: {pm.PaymentMethod}" });
                        }

                        if (account.ChildAccountId == null)
                        {
                            Console.WriteLine($"No ChildAccountId for AccountId: {pm.AccountId}, PaymentMethod: {pm.PaymentMethod}");
                            return Json(new { success = false, message = $"الحساب المحدد ليس مرتبطًا بحساب فرعي صالح: {pm.PaymentMethod}" });
                        }

                        // Update account balance
                        decimal paymentAmount = pm.PaymentAmount ?? 0;
                        decimal newBalance = account.Balance - paymentAmount;
                        if (newBalance < 0)
                        {
                            Console.WriteLine($"Insufficient balance for AccountId: {account.AccountId}, PaymentMethod: {pm.PaymentMethod}, New Balance: {newBalance}");
                            return Json(new { success = false, message = $"رصيد الحساب غير كافٍ لطريقة الدفع: {pm.PaymentMethod}" });
                        }
                        account.Balance = newBalance;
                        creditChildAccountId = account.ChildAccountId;
                        creditDescription += pm.PaymentMethod == "كاش" ? $"دفع نقدي ({account.AccountName})" : $"تحويل بنكي ({account.AccountName})";
                    }
                    else if (pm.PaymentMethod == "اجل (مدفوع)")
                    {
                        creditChildAccountId = 5; // الحسابات المستحقة الدفع
                        creditDescription += "تسوية آجل";
                    }

                    // Create journal entries
                    var debitEntry = new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(model.PaymentDate),
                        Description = $"دفعة مورد: {paymentInfo.VendorName} - فاتورة #{paymentInfo.InvoiceId} - {pm.PaymentMethod}",
                        DebitAmount = pm.PaymentAmount ?? 0,
                        CreditAmount = 0,
                        ChildAccountId = 5, // الحسابات المستحقة الدفع
                        UserId = userId.Value,
                        CostCenterId = 1,
                        InvoiceId = paymentInfo.InvoiceId,
                        InvoiceType = "Vendor",
                        CreatedAt = DateTime.Now
                    };
                    _context.JournalEntries.Add(debitEntry);

                    var creditEntry = new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(model.PaymentDate),
                        Description = creditDescription,
                        DebitAmount = 0,
                        CreditAmount = pm.PaymentAmount ?? 0,
                        ChildAccountId = creditChildAccountId,
                        UserId = userId.Value,
                        CostCenterId = 1,
                        InvoiceId = paymentInfo.InvoiceId,
                        InvoiceType = "Vendor",
                        CreatedAt = DateTime.Now
                    };
                    _context.JournalEntries.Add(creditEntry);
                }

                _context.Paymentdetails.Remove(paymentDetail);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("VendorDeferredPayments", "Vendors", new { vendorId = paymentInfo.VendorId })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing payment: {ex.Message}, InnerException: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"خطأ أثناء معالجة الدفع: {ex.Message}. Inner: {ex.InnerException?.Message}" });
            }
        }

        private async Task<List<CashAndBankAccount>> GetUserAccounts(int userId)
        {
            return await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId && a.Status == "نشط")
                .Select(a => new CashAndBankAccount
                {
                    AccountID = a.AccountId,
                    AccountName = a.AccountName
                })
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts(string accountType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized();
            }

            var accounts = await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId && a.AccountType == accountType)
                .Select(a => new
                {
                    accountID = a.AccountId,
                    accountName = a.AccountName
                })
                .ToListAsync();

            return Json(accounts);
        }


















        public async Task<IActionResult> AgingReport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "جلسة المستخدم منتهية. يرجى تسجيل الدخول.", redirectUrl = Url.Action("Login", "Usered") });
            }

            var today = DateTime.Today;
            var report = new APAgingReportViewModel();

            // Fetch data matching the SQL query
            var invoices = await _context.Vendors
                .Where(v => v.UserId == userId.Value)
                .Join(_context.Invoices.Where(i => i.InvoiceType == "شراء"),
                    v => v.VendorId,
                    i => i.VendorId,
                    (v, i) => new { Vendor = v, Invoice = i })
                .Join(_context.Payments,
                    vi => vi.Invoice.InvoiceId,
                    p => p.InvoiceId,
                    (vi, p) => new { vi.Vendor, vi.Invoice, Payment = p })
                .Join(_context.Paymentdetails.Where(pd => pd.IsPaid == false),
                    vip => vip.Payment.Payid,
                    pd => pd.Payid,
                    (vip, pd) => new
                    {
                        VendorId = vip.Vendor.VendorId,
                        VendorName = vip.Vendor.VendorName,
                        InvoiceId = vip.Invoice.InvoiceId,
                        PayDetailId = pd.PaydetailId,
                        AmountDue = pd.Amount,
                        DueDate = pd.DueDate,
                        DaysPastDue = Microsoft.EntityFrameworkCore.MySqlDbFunctionsExtensions.DateDiffDay(
                            EF.Functions,
                            pd.DueDate.HasValue ? pd.DueDate.Value.ToDateTime(TimeOnly.MinValue) : vip.Invoice.InvoiceDate.AddDays(30),
                            today)
                    })
                .Select(x => new AgingInvoice
                {
                    VendorId = x.VendorId,
                    VendorName = x.VendorName,
                    InvoiceId = x.InvoiceId,
                    PayDetailId = x.PayDetailId,
                    AmountDue = x.AmountDue,
                    DueDate = x.DueDate.HasValue ? x.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                    DaysPastDue = x.DaysPastDue,
                    DelayCategory = x.DaysPastDue < 0 ? "غير متأخرة" :
                                    x.DaysPastDue == 0 ? "مستحقة اليوم" :
                                    x.DaysPastDue <= 30 ? "ديون متأخرة 30 يوم" :
                                    x.DaysPastDue <= 60 ? "ديون متأخرة 30-60 يوم" :
                                    x.DaysPastDue <= 90 ? "ديون متأخرة 60-90 يوم" :
                                    "ديون متأخرة لأكثر من 90 يوم",
                    Days1To30 = x.DaysPastDue >= 1 && x.DaysPastDue <= 30 ? x.AmountDue : 0,
                    Days31To60 = x.DaysPastDue >= 31 && x.DaysPastDue <= 60 ? x.AmountDue : 0,
                    Days61To90 = x.DaysPastDue >= 61 && x.DaysPastDue <= 90 ? x.AmountDue : 0,
                    DaysOver90 = x.DaysPastDue > 90 ? x.AmountDue : 0
                })
                .OrderBy(x => x.VendorName)
                .ThenByDescending(x => x.DaysPastDue < 0 ? 0 : x.DaysPastDue)
                .ToListAsync();

            report.Invoices = invoices;

            return View(report);
        }
    }
}

    