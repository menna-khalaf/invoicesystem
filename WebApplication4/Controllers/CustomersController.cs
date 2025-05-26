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
    public class CustomersController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public CustomersController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }
        // Existing Index action
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var userCustomers = await _context.Customers
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(userCustomers);
        }

        // Updated DeferredPayments action
        public async Task<IActionResult> DeferredPayments(int? customerId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var query = _context.Payments
                    .Where(p => p.UserId == userId)
                    .Join(_context.Paymentdetails,
                        p => p.Payid,
                        pd => pd.Payid,
                        (p, pd) => new { Payment = p, PaymentDetail = pd })
                    .Where(joined => joined.PaymentDetail.Description.Contains("آجل"));

                // Apply CustomerId filter if provided
                if (customerId.HasValue)
                {
                    query = query.Where(joined => joined.Payment.CustomerId == customerId.Value);
                }

                var deferredPayments = await query
                    .Select(joined => new DeferredPaymentModel
                    {
                        PayId = joined.Payment.Payid,
                        CustomerId = joined.Payment.CustomerId,
                        VendorId = joined.Payment.VendorId,
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

                // Pass CustomerId to view for display purposes
                ViewBag.CustomerId = customerId;
                return View(deferredPayments);
            }
            catch (Exception ex)
            {
                return Problem($"Error retrieving deferred payments: {ex.Message}");
            }
        }



























































        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }


        // GET: Customers/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerName,ContactInfo")] Customer customer)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Set additional properties
            customer.UserId = userId.Value;
            customer.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || customer.UserId != userId)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerName,ContactInfo,CreatedAt")] Customer customer)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            // Get the existing customer to verify ownership and preserve UserId
            var existingCustomer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (existingCustomer == null || existingCustomer.UserId != userId)
            {
                return NotFound();
            }

            // Preserve the original UserId and CreatedAt
            customer.UserId = existingCustomer.UserId;
            customer.CreatedAt = existingCustomer.CreatedAt;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Customers == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Customers'  is null.");
            }
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
          return (_context.Customers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }



















        public async Task<IActionResult> ProcessPayment(int payId, int payDetailId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Find the payment detail to get the correct PayId and Amount
            var paymentDetail = await _context.Paymentdetails
                .FirstOrDefaultAsync(pd => pd.PaydetailId == payDetailId);

            if (paymentDetail == null)
            {
                return NotFound("تفاصيل الدفع غير موجودة");
            }

            // Verify PayId matches and fetch payment details with customer name
            var payment = await _context.Payments
                .Join(_context.Paymentdetails,
                    p => p.Payid,
                    pd => pd.Payid,
                    (p, pd) => new { Payment = p, PaymentDetail = pd })
                .Join(_context.Customers,
                    joined => joined.Payment.CustomerId,
                    c => c.CustomerId,
                    (joined, c) => new { joined.Payment, joined.PaymentDetail, Customer = c })
                .Where(joined => joined.Payment.Payid == paymentDetail.Payid && joined.PaymentDetail.PaydetailId == payDetailId)
                .Select(joined => new ProcessPaymentViewModel
                {
                    PayId = joined.Payment.Payid,
                    PayDetailId = joined.PaymentDetail.PaydetailId,
                    CustomerName = joined.Customer.CustomerName, // Fetch actual customer name
                    PaymentDate = DateTime.Now,
                    Amount = joined.PaymentDetail.Amount, // Directly use the Amount from the specific PaymentDetail
                    PaymentMethods = new List<PaymentMethodModel>() // Fixed: Added parentheses
                })
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                return NotFound("الدفع غير موجود");
            }

            // Fetch available accounts for the dropdown
            payment.Accounts = await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId)
                .Select(a => new CashAndBankAccount
                {
                    AccountID = a.AccountId,
                    AccountName = a.AccountName
                })
                .ToListAsync();

            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentViewModel model)
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
                model.Accounts = await GetUserAccounts(userId.Value);
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
                    model.Accounts = await GetUserAccounts(userId.Value);
                    return Json(new { success = false, message = "لم يتم العثور على تفاصيل الدفع" });
                }

                decimal totalPaymentAmount = model.PaymentMethods.Sum(pm => pm.PaymentAmount ?? 0);
                if (Math.Abs(totalPaymentAmount - paymentDetail.Amount) > 0.01m)
                {
                    Console.WriteLine($"Total payment amount {totalPaymentAmount} does not match PaymentDetail amount {paymentDetail.Amount}");
                    model.Accounts = await GetUserAccounts(userId.Value);
                    return Json(new { success = false, message = "مجموع مبالغ طرق الدفع لا يساوي المبلغ الأصلي" });
                }

                // Fetch customer name and invoice ID
                var paymentInfo = await _context.Payments
                    .Where(p => p.Payid == model.PayId)
                    .Join(_context.Invoices,
                        p => p.InvoiceId,
                        i => i.InvoiceId,
                        (p, i) => new { p.InvoiceId, i.CustomerId })
                    .Join(_context.Customers,
                        joined => joined.CustomerId,
                        c => c.CustomerId,
                        (joined, c) => new { joined.InvoiceId,joined.CustomerId, CustomerName = c.CustomerName })
                    .FirstOrDefaultAsync();

                if (paymentInfo == null)
                {
                    Console.WriteLine($"Customer or invoice information not found for PayId: {model.PayId}");
                    model.Accounts = await GetUserAccounts(userId.Value);
                    return Json(new { success = false, message = "لم يتم العثور على معلومات الفاتورة أو العميل" });
                }

                // Update the original payment detail
                paymentDetail.IsPaid = true;
                paymentDetail.PaidDate = DateOnly.FromDateTime(model.PaymentDate);
                paymentDetail.UpdatedAt = DateTime.Now;

                // Process each payment method
                foreach (var pm in model.PaymentMethods)
                {
                    if (string.IsNullOrEmpty(pm.PaymentMethod))
                    {
                        Console.WriteLine($"Invalid payment method for PayDetailId: {model.PayDetailId}");
                        model.Accounts = await GetUserAccounts(userId.Value);
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

                    // Get child_account_id and description for debit entry
                    int? debitChildAccountId = null;
                    string debitDescription = $"دفعة عميل: {paymentInfo.CustomerName} - فاتورة #{paymentInfo.InvoiceId} - ";
                    if (pm.PaymentMethod == "كاش" || pm.PaymentMethod == "تحويل بنكي")
                    {
                        if (!pm.AccountId.HasValue)
                        {
                            Console.WriteLine($"No AccountId provided for PaymentMethod: {pm.PaymentMethod}");
                            model.Accounts = await GetUserAccounts(userId.Value);
                            return Json(new { success = false, message = $"يجب اختيار حساب لطريقة الدفع: {pm.PaymentMethod}" });
                        }

                        var account = await _context.Cashandbankaccounts
                            .FirstOrDefaultAsync(a => a.AccountId == pm.AccountId.Value && a.UserId == userId);
                        if (account == null)
                        {
                            Console.WriteLine($"Account not found for AccountId: {pm.AccountId}, PaymentMethod: {pm.PaymentMethod}");
                            model.Accounts = await GetUserAccounts(userId.Value);
                            return Json(new { success = false, message = $"الحساب المحدد غير موجود أو لا ينتمي إلى المستخدم لطريقة الدفع: {pm.PaymentMethod}" });
                        }

                        if (account.ChildAccountId == null)
                        {
                            Console.WriteLine($"No ChildAccountId for AccountId: {pm.AccountId}, PaymentMethod: {pm.PaymentMethod}");
                            model.Accounts = await GetUserAccounts(userId.Value);
                            return Json(new { success = false, message = $"الحساب المحدد ليس مرتبطًا بحساب فرعي صالح: {pm.PaymentMethod}" });
                        }

                        // Update account balance
                        decimal paymentAmount = pm.PaymentAmount ?? 0;
                        account.Balance += paymentAmount;
                        debitChildAccountId = account.ChildAccountId;
                        debitDescription += pm.PaymentMethod == "كاش" ? $"دفع نقدي ({account.AccountName})" : $"تحويل بنكي ({account.AccountName})";
                    }
                    else if (pm.PaymentMethod == "اجل (مدفوع)")
                    {
                        debitChildAccountId = 4; // الحسابات المستحقة القبض
                        debitDescription += "تسوية آجل";
                    }

                    // Create journal entries
                    var debitEntry = new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(model.PaymentDate),
                        Description = debitDescription,
                        DebitAmount = pm.PaymentAmount ?? 0,
                        CreditAmount = 0,
                        ChildAccountId = debitChildAccountId,
                        UserId = userId.Value,
                        CostCenterId = 1, // TODO: Replace with actual CostCenterId logic
                        InvoiceId = paymentInfo.InvoiceId,
                        InvoiceType = "Customer",
                        CreatedAt = DateTime.Now
                    };
                    _context.JournalEntries.Add(debitEntry);

                    var creditEntry = new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(model.PaymentDate),
                        Description = $"دفعة عميل: {paymentInfo.CustomerName} - فاتورة #{paymentInfo.InvoiceId} - {pm.PaymentMethod}",
                        DebitAmount = 0,
                        CreditAmount = pm.PaymentAmount ?? 0,
                        ChildAccountId = 4, // الحسابات المستحقة القبض
                        UserId = userId.Value,
                        CostCenterId = 1, // TODO: Replace with actual CostCenterId logic
                        InvoiceId = paymentInfo.InvoiceId,
                        InvoiceType = "Customer",
                        CreatedAt = DateTime.Now
                    };
                    _context.JournalEntries.Add(creditEntry);
                }

                // Remove the original payment detail
                _context.Paymentdetails.Remove(paymentDetail);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("DeferredPayments", "Customers", new { customerId = paymentInfo.CustomerId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing payment: {ex.Message}, InnerException: {ex.InnerException?.Message}, StackTrace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                model.Accounts = await GetUserAccounts(userId.Value);
                return Json(new { success = false, message = $"خطأ أثناء معالجة الدفع: {ex.Message}. Inner: {ex.InnerException?.Message}" });
            }
        }

        private async Task<List<CashAndBankAccount>> GetUserAccounts(int userId)
        {
            return await _context.Cashandbankaccounts
                .Where(a => a.UserId == userId)
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
                .Where(a => a.AccountType == accountType && a.Status == "نشط" && a.UserId == userId)
                .Select(a => new
                {
                    accountID = a.AccountId,
                    accountName = a.AccountName
                })
                .ToListAsync();

            return Json(accounts);
        }






























        public async Task<IActionResult> CustomerAgingReport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "جلسة المستخدم منتهية. يرجى تسجيل الدخول.", redirectUrl = Url.Action("Login", "Usered") });
            }

            var today = DateTime.Today;
            var report = new ARAgingReportViewModel();

            // Fetch data matching the SQL query
            var invoices = await _context.Customers
                .Where(c => c.UserId == userId.Value)
                .Join(_context.Invoices.Where(i => i.InvoiceType == "بيع"),
                    c => c.CustomerId,
                    i => i.CustomerId,
                    (c, i) => new { Customer = c, Invoice = i })
                .Join(_context.Payments,
                    ci => ci.Invoice.InvoiceId,
                    p => p.InvoiceId,
                    (ci, p) => new { ci.Customer, ci.Invoice, Payment = p })
                .Join(_context.Paymentdetails.Where(pd => pd.IsPaid == false),
                    cip => cip.Payment.Payid,
                    pd => pd.Payid,
                    (cip, pd) => new
                    {
                        CustomerId = cip.Customer.CustomerId,
                        CustomerName = cip.Customer.CustomerName,
                        InvoiceId = cip.Invoice.InvoiceId,
                        PayDetailId = pd.PaydetailId,
                        AmountDue = pd.Amount,
                        DueDate = pd.DueDate,
                        DaysPastDue = Microsoft.EntityFrameworkCore.MySqlDbFunctionsExtensions.DateDiffDay(
                            EF.Functions,
                            pd.DueDate.HasValue ? pd.DueDate.Value.ToDateTime(TimeOnly.MinValue) : cip.Invoice.InvoiceDate.AddDays(30),
                            today)
                    })
                .Select(x => new CustomerAgingInvoice
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
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
                .OrderBy(x => x.CustomerName)
                .ThenByDescending(x => x.DaysPastDue < 0 ? 0 : x.DaysPastDue)
                .ToListAsync();

            report.Invoices = invoices;

            return View(report);
        }
    }
}
