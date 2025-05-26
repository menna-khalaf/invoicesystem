using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http;

namespace WebApplication4.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public InvoicesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //// Get current user ID from session
            //var userId = HttpContext.Session.GetInt32("UserId");

            //if (userId == null)
            //{
            //    // Handle case where user is not logged in
            //    return RedirectToAction("Login", "Account");
            //}

            //var invoices = _context.Invoices
            //    .Where(i => i.UserId == userId) // Filter by logged-in user's ID
            //    .Include(i => i.Customer) // Include related Customer data
            //    .Select(i => new
            //    {
            //        Invoice = i,
            //        TotalAmount = _context.Invoicedetails
            //            .Where(d => d.InvoiceId == i.InvoiceId)
            //            .Sum(d => d.LineTotal), // Calculate TotalAmount from InvoiceDetails
            //        InvoiceType = _context.Invoicedetails
            //            .Where(d => d.InvoiceId == i.InvoiceId)
            //            .Select(d => d.InvoiceTyping)
            //            .FirstOrDefault() // Retrieve InvoiceType from Invoicedetails
            //    })
            //    .ToList()
            //    .Select(x => new Invoice
            //    {
            //        InvoiceId = x.Invoice.InvoiceId,
            //        Customer = x.Invoice.Customer,
            //        Status = x.Invoice.Status,
            //        CreatedAt = x.Invoice.CreatedAt,
            //        TotalAmount = x.TotalAmount,
            //        InvoiceType = x.InvoiceType // Set InvoiceType from Invoicedetails
            //    })
            //    .ToList();

            return View();
        }
        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
        // DTO for account data
        public class AccountDto
        {
            public int AccountId { get; set; }
            public string AccountName { get; set; }
        }

        // DTO for employee data
        public class EmployeeDto
        {
            public int EmployeeId { get; set; }
            public string FullName { get; set; }
        }

        // DTO for customer data
        public class CustomerDto
        {
            public int CustomerId { get; set; }
            public string CustomerName { get; set; }
        }

        // GET: Invoices/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var userStorehouses = _context.Storehouses
                    ?.Where(s => s.UserId == userId)
                    .ToList() ?? new List<Storehouse>();

                var userBranches = _context.Branches
                    ?.Where(b => b.UserId == userId)
                    .ToList() ?? new List<Branch>();

                var userProducts = _context.Products
                    ?.Where(p => p.UserId == userId)
                    .ToList() ?? new List<Product>();

                var cashAccounts = _context.Cashandbankaccounts
                    ?.Where(a => a.UserId == userId && a.AccountType == "خزينة" && a.Status == "نشط")
                    .Select(a => new AccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName ?? $"حساب بدون اسم (ID: {a.AccountId})"
                    })
                    .ToList() ?? new List<AccountDto>();

                var bankAccounts = _context.Cashandbankaccounts
                    ?.Where(a => a.UserId == userId && a.AccountType == "حساب بنكي" && a.Status == "نشط")
                    .Select(a => new AccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName ?? $"حساب بدون اسم (ID: {a.AccountId})"
                    })
                    .ToList() ?? new List<AccountDto>();

                var employees = _context.Employees
                    ?.Where(e => e.UserId == userId)
                    .Select(e => new EmployeeDto
                    {
                        EmployeeId = e.EmployeeId,
                        FullName = e.FullName ?? $"موظف بدون اسم (ID: {e.EmployeeId})"
                    })
                    .ToList() ?? new List<EmployeeDto>();

                var customers = _context.Customers
                    ?.Where(c => c.UserId == userId)
                    .Select(c => new CustomerDto
                    {
                        CustomerId = c.CustomerId,
                        CustomerName = c.CustomerName ?? $"عميل بدون اسم (ID: {c.CustomerId})"
                    })
                    .ToList() ?? new List<CustomerDto>();

                Console.WriteLine($"UserId: {userId}");
                Console.WriteLine($"Cash Accounts: {cashAccounts.Count}");
                cashAccounts.ForEach(a => Console.WriteLine($"Cash Account: Id={a.AccountId}, Name={a.AccountName}"));
                Console.WriteLine($"Bank Accounts: {bankAccounts.Count}");
                bankAccounts.ForEach(a => Console.WriteLine($"Bank Account: Id={a.AccountId}, Name={a.AccountName}"));
                Console.WriteLine($"Employees: {employees.Count}");
                employees.ForEach(e => Console.WriteLine($"Employee: Id={e.EmployeeId}, Name={e.FullName}"));
                Console.WriteLine($"Customers: {customers.Count}");
                customers.ForEach(c => Console.WriteLine($"Customer: Id={c.CustomerId}, Name={c.CustomerName}"));

                ViewData["CustomerId"] = new SelectList(customers, "CustomerId", "CustomerName");
                ViewData["EmployeeId"] = new SelectList(employees, "EmployeeId", "FullName");
                ViewData["StorehouseId"] = new SelectList(userStorehouses, "StorehouseId", "StorehouseName");
                ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName");
                ViewData["ProductId"] = new SelectList(userProducts, "ProductId", "ProductName");

                ViewBag.CashAccounts = cashAccounts;
                ViewBag.BankAccounts = bankAccounts;

                ViewBag.ProductPrices = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.Price);
                ViewBag.ProductBalances = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.Balance);
                ViewBag.ProductVatRates = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.Vatrate ?? 0m);

                ViewBag.VatRate = 15m;

                ViewData["CurrentUserId"] = userId;
                return View(new Invoice { InvoiceDate = DateTime.Now, Vatrate = ViewBag.VatRate });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Create GET: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل صفحة إنشاء الفاتورة." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(Invoice invoice, [FromForm] List<Invoicedetail> invoicedetails, [FromForm] List<PaymentEntry> paymentEntries)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "بيانات غير صالحة.", errors });
            }

            if (invoicedetails == null || !invoicedetails.Any())
            {
                return Json(new { success = false, message = "يجب إدخال تفاصيل الفاتورة." });
            }

            if (paymentEntries == null || !paymentEntries.Any())
            {
                return Json(new { success = false, message = "يجب إدخال طريقة دفع واحدة على الأقل." });
            }

            // Log raw invoicedetails
            Console.WriteLine("Raw Invoicedetails from client:");
            foreach (var detail in invoicedetails)
            {
                Console.WriteLine($"ProductId={detail.ProductId}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, VatAmount={detail.VatAmount}, LineTotal={detail.LineTotal}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Log invoice input data
                Console.WriteLine($"Received Invoice: EmployeeID={invoice.EmployeeId}, CustomerId={invoice.CustomerId}, StorehouseId={invoice.StorehouseId}, BranchId={invoice.BranchId}, Subtotal={invoice.Subtotal}, Vatamount={invoice.Vatamount}, TotalAmount={invoice.TotalAmount}");

                // Validate EmployeeId
                if (invoice.EmployeeId.HasValue)
                {
                    var employee = await _context.Employees
                        .AsNoTracking()
                        .Where(e => e.EmployeeId == invoice.EmployeeId.Value && e.UserId == userId)
                        .FirstOrDefaultAsync();
                    if (employee == null)
                    {
                        Console.WriteLine($"Invalid EmployeeId: {invoice.EmployeeId}");
                        return Json(new { success = false, message = $"الموظف برقم {invoice.EmployeeId.Value} غير موجود." });
                    }
                    Console.WriteLine($"EmployeeId: {invoice.EmployeeId} validated for UserId: {userId}");
                }

                // Validate CustomerId
                if (invoice.CustomerId.HasValue)
                {
                    var customer = await _context.Customers
                        .AsNoTracking()
                        .Where(c => c.CustomerId == invoice.CustomerId.Value && c.UserId == userId)
                        .FirstOrDefaultAsync();
                    if (customer == null)
                    {
                        Console.WriteLine($"Invalid CustomerId: {invoice.CustomerId}");
                        return Json(new { success = false, message = $"العميل برقم {invoice.CustomerId.Value} غير موجود." });
                    }
                    Console.WriteLine($"CustomerId: {invoice.CustomerId} validated for UserId: {userId}");
                }

                // Validate StorehouseId
                if (!invoice.StorehouseId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تحديد المخزن." });
                }
                var storehouse = await _context.Storehouses
                    .AsNoTracking()
                    .Where(s => s.StorehouseId == invoice.StorehouseId.Value && s.UserId == userId)
                    .FirstOrDefaultAsync();
                if (storehouse == null)
                {
                    Console.WriteLine($"Invalid StorehouseId: {invoice.StorehouseId}");
                    return Json(new { success = false, message = $"المخزن برقم {invoice.StorehouseId.Value} غير موجود." });
                }
                Console.WriteLine($"StorehouseId: {invoice.StorehouseId} validated for UserId: {userId}");

                // Validate BranchId
                if (invoice.BranchId.HasValue)
                {
                    var branch = await _context.Branches
                        .AsNoTracking()
                        .Where(b => b.BranchId == invoice.BranchId.Value && b.UserId == userId)
                        .FirstOrDefaultAsync();
                    if (branch == null)
                    {
                        Console.WriteLine($"Invalid BranchId: {invoice.BranchId}");
                        return Json(new { success = false, message = $"الفرع برقم {invoice.BranchId.Value} غير موجود." });
                    }
                    Console.WriteLine($"BranchId: {invoice.BranchId} validated for UserId: {userId}");
                }

                // Filter valid invoicedetails
                invoicedetails = invoicedetails
                    .Where(d => d.ProductId.HasValue && d.ProductId > 0 && d.Quantity > 0 && d.UnitPrice >= 0)
                    .ToList();
                if (!invoicedetails.Any())
                {
                    return Json(new { success = false, message = "يجب إدخال تفاصيل فاتورة صالحة (معرف المنتج والكمية مطلوبان)." });
                }

                // Check for duplicate products
                var receivedProductIds = invoicedetails.Select(d => d.ProductId.Value).ToList();
                if (receivedProductIds.Distinct().Count() != receivedProductIds.Count)
                {
                    var duplicates = receivedProductIds.GroupBy(id => id)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key);
                    return Json(new
                    {
                        success = false,
                        message = $"لا يمكن إدخال نفس المنتج أكثر من مرة: {string.Join(", ", duplicates)}"
                    });
                }

                // Validate invoicedetails and collect costs
                var productCosts = new Dictionary<int, decimal>();
                foreach (var detail in invoicedetails)
                {
                    var product = await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId.Value && p.UserId == userId);
                    if (product == null)
                    {
                        return Json(new { success = false, message = $"المنتج برقم {detail.ProductId.Value} غير موجود أو لا يخصك." });
                    }

                    if (detail.Quantity > product.Balance)
                    {
                        return Json(new { success = false, message = $"الكمية المطلوبة ({detail.Quantity}) تتجاوز الرصيد المتاح ({product.Balance}) للمنتج {product.ProductName}." });
                    }

                    if (product.PurchasePrice == null || product.Vatrate == null)
                    {
                        return Json(new { success = false, message = $"سعر الشراء أو معدل الضريبة للمنتج {product.ProductName} غير محدد." });
                    }

                    // Recalculate VAT and LineTotal
                    decimal vatRate = product.Vatrate ?? 0m;
                    decimal subtotal = detail.Quantity * detail.UnitPrice;
                    decimal vatAmount = Math.Round(subtotal * (vatRate / 100), 2, MidpointRounding.AwayFromZero);
                    decimal expectedLineTotal = subtotal + vatAmount;

                    // Validate client-provided UnitPrice and VatAmount
                    if (detail.LineTotal > 0 && detail.Quantity > 0)
                    {
                        decimal totalExclVat = detail.LineTotal / (1 + vatRate / 100);
                        decimal calculatedUnitPrice = Math.Round(totalExclVat / detail.Quantity, 2, MidpointRounding.AwayFromZero);
                        if (Math.Abs(detail.UnitPrice - calculatedUnitPrice) > 0.01m)
                        {
                            Console.WriteLine($"Correcting UnitPrice for ProductId={detail.ProductId}: Client={detail.UnitPrice}, Calculated={calculatedUnitPrice}");
                            detail.UnitPrice = calculatedUnitPrice;
                            subtotal = detail.Quantity * detail.UnitPrice;
                            vatAmount = Math.Round(subtotal * (vatRate / 100), 2, MidpointRounding.AwayFromZero);
                            expectedLineTotal = subtotal + vatAmount;
                        }
                    }

                    if (detail.VatAmount.HasValue && Math.Abs(detail.VatAmount.Value - vatAmount) > 0.01m)
                    {
                        Console.WriteLine($"Correcting VatAmount for ProductId={detail.ProductId}: Client={detail.VatAmount.Value}, Calculated={vatAmount}");
                        detail.VatAmount = vatAmount;
                    }
                    else
                    {
                        detail.VatAmount = vatAmount;
                    }

                    if (Math.Abs(detail.LineTotal - expectedLineTotal) > 0.01m)
                    {
                        Console.WriteLine($"Correcting LineTotal for ProductId={detail.ProductId}: Client={detail.LineTotal}, Calculated={expectedLineTotal}");
                        detail.LineTotal = expectedLineTotal;
                    }

                    // Log VAT calculation
                    Console.WriteLine($"VAT Calculation for ProductId={detail.ProductId}: Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, VatRate={vatRate}%, VatAmount={vatAmount}, LineTotal={expectedLineTotal}");

                    // Store cost as PurchasePrice only (excluding VAT)
                    productCosts[detail.ProductId.Value] = product.PurchasePrice;
                }

                // Set invoice properties
                invoice.UserId = userId;
                invoice.InvoiceType = "بيع";
                invoice.CreatedAt = DateTime.Now;
                Console.WriteLine($"Saving Invoice: UserId={invoice.UserId}, EmployeeId={invoice.EmployeeId}, CustomerId={invoice.CustomerId}, StorehouseId={invoice.StorehouseId}, BranchId={invoice.BranchId}, Subtotal={invoice.Subtotal}, Vatamount={invoice.Vatamount}, TotalAmount={invoice.TotalAmount}");
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Process invoicedetails
                foreach (var detail in invoicedetails)
                {
                    detail.InvoiceId = invoice.InvoiceId;
                    detail.InvoiceTyping = "بيع";
                    _context.Invoicedetails.Add(detail);

                    // Insert inventory record
                    var inventory = new Inventory
                    {
                        ProductId = detail.ProductId.Value,
                        OutgoingQuantity = detail.Quantity,
                        IncomingQuantity = 0,
                        LastUpdated = DateTime.Now,
                        UserId = userId.Value,
                        StorehouseId = invoice.StorehouseId.Value
                    };
                    _context.Inventories.Add(inventory);

                    // Update product balance
                    var product = await _context.Products.FindAsync(detail.ProductId.Value);
                    if (product == null)
                    {
                        return Json(new { success = false, message = $"المنتج برقم {detail.ProductId.Value} غير موجود." });
                    }
                    product.Balance -= detail.Quantity;
                    _context.Products.Update(product);

                    // Detach to prevent re-tracking
                    _context.Entry(detail).State = EntityState.Detached;
                }

                // Calculate invoice totals
                invoice.Subtotal = invoicedetails.Sum(d => d.Quantity * d.UnitPrice);
                invoice.Vatamount = invoicedetails.Sum(d => d.VatAmount ?? 0m);
                invoice.TotalAmount = invoice.Subtotal + invoice.Vatamount;
                _context.Invoices.Update(invoice);

                // Validate payment entries
                foreach (var entry in paymentEntries)
                {
                    if (string.IsNullOrEmpty(entry.PaymentMethod) || entry.Amount <= 0)
                    {
                        return Json(new { success = false, message = "يجب إدخال طريقة دفع صالحة مع مبلغ أكبر من 0." });
                    }
                    if (entry.PaymentMethod == "آجل" && !entry.DueDate.HasValue)
                    {
                        return Json(new { success = false, message = "يجب تحديد تاريخ الاستحقاق لطريقة الدفع 'آجل'." });
                    }
                    if (entry.PaymentMethod != "آجل" && !entry.PaidDate.HasValue)
                    {
                        return Json(new { success = false, message = $"يجب تحديد تاريخ الدفع لطريقة الدفع '{entry.PaymentMethod}'." });
                    }
                    if ((entry.PaymentMethod == "كاش" || entry.PaymentMethod == "تحويل بنكي") && !entry.AccountId.HasValue)
                    {
                        return Json(new { success = false, message = $"يجب تحديد حساب لطريقة الدفع '{entry.PaymentMethod}'." });
                    }
                    // Validate AccountId
                    if (entry.AccountId.HasValue)
                    {
                        var account = await _context.Cashandbankaccounts
                            .AsNoTracking()
                            .Where(a => a.AccountId == entry.AccountId.Value && a.UserId == userId && a.Status == "نشط")
                            .FirstOrDefaultAsync();
                        if (account == null)
                        {
                            Console.WriteLine($"Invalid AccountId: {entry.AccountId}");
                            return Json(new { success = false, message = $"الحساب برقم {entry.AccountId.Value} غير موجود أو غير نشط." });
                        }
                        if (entry.PaymentMethod == "كاش" && account.AccountType != "خزينة")
                        {
                            return Json(new { success = false, message = $"الحساب برقم {entry.AccountId.Value} ليس حساب خزينة لدفع كاش." });
                        }
                        if (entry.PaymentMethod == "تحويل بنكي" && account.AccountType != "حساب بنكي")
                        {
                            return Json(new { success = false, message = $"الحساب برقم {entry.AccountId.Value} ليس حساب بنكي لتحويل بنكي." });
                        }
                        Console.WriteLine($"AccountId: {entry.AccountId} validated for UserId: {userId}, PaymentMethod: {entry.PaymentMethod}");
                    }
                }

                // Validate payment totals
                var totalPaymentAmount = paymentEntries.Sum(p => p.Amount);
                if (totalPaymentAmount != invoice.TotalAmount)
                {
                    return Json(new { success = false, message = "إجمالي مبالغ الدفع لا يساوي إجمالي الفاتورة." });
                }

                // Assign cash and bank account IDs
                var firstCashEntry = paymentEntries.FirstOrDefault(p => p.PaymentMethod == "كاش" && p.AccountId.HasValue);
                var firstBankEntry = paymentEntries.FirstOrDefault(p => p.PaymentMethod == "تحويل بنكي" && p.AccountId.HasValue);
                invoice.InvoiceCashAccountId = firstCashEntry?.AccountId;
                invoice.InvoiceBankAccountId = firstBankEntry?.AccountId;
                _context.Invoices.Update(invoice);

                // Create payment record
                var payment = new Payment
                {
                    CustomerId = invoice.CustomerId,
                    UserId = userId.Value,
                    Paydate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = totalPaymentAmount,
                    PaymentStatus = "Paid",
                    InvoiceId = invoice.InvoiceId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Process payment details
                foreach (var entry in paymentEntries)
                {
                    var paymentDetail = new Paymentdetail
                    {
                        Payid = payment.Payid,
                        UserId = userId.Value,
                        Description = entry.PaymentMethod,
                        Amount = entry.Amount,
                        DueDate = entry.PaymentMethod == "آجل" && entry.DueDate.HasValue ? DateOnly.FromDateTime(entry.DueDate.Value) : null,
                        IsPaid = entry.PaymentMethod != "آجل",
                        PaidDate = entry.PaymentMethod != "آجل" && entry.PaidDate.HasValue ? DateOnly.FromDateTime(entry.PaidDate.Value) : null,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Paymentdetails.Add(paymentDetail);
                }

                // Update cashandbankaccounts balances
                if (invoice.InvoiceCashAccountId.HasValue)
                {
                    var cashAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceCashAccountId.Value);
                    if (cashAccount == null)
                    {
                        return Json(new { success = false, message = $"الحساب النقدي برقم {invoice.InvoiceCashAccountId.Value} غير موجود." });
                    }
                    if (cashAccount.ChildAccountId == null && cashAccount.AccountType == "خزينة")
                    {
                        cashAccount.ChildAccountId = 1; // النقدية بالصندوق
                    }
                    cashAccount.Balance += paymentEntries.Where(p => p.PaymentMethod == "كاش").Sum(p => p.Amount);
                    _context.Cashandbankaccounts.Update(cashAccount);
                    Console.WriteLine($"Updated cashandbankaccounts: AccountId={cashAccount.AccountId}, ChildAccountId={cashAccount.ChildAccountId}, NewBalance={cashAccount.Balance}");
                }

                if (invoice.InvoiceBankAccountId.HasValue)
                {
                    var bankAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceBankAccountId.Value);
                    if (bankAccount == null)
                    {
                        return Json(new { success = false, message = $"الحساب البنكي برقم {invoice.InvoiceBankAccountId.Value} غير موجود." });
                    }
                    if (bankAccount.ChildAccountId == null && bankAccount.AccountType == "حساب بنكي")
                    {
                        bankAccount.ChildAccountId = 2; // النقدية بالبنك
                    }
                    bankAccount.Balance += paymentEntries.Where(p => p.PaymentMethod == "تحويل بنكي").Sum(p => p.Amount);
                    _context.Cashandbankaccounts.Update(bankAccount);
                    Console.WriteLine($"Updated cashandbankaccounts: AccountId={bankAccount.AccountId}, ChildAccountId={bankAccount.ChildAccountId}, NewBalance={bankAccount.Balance}");
                }

                // Fetch ChildAccountIds using account_mappings
                var salesAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "إيرادات المبيعات")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (salesAccountId == 0)
                {
                    return Json(new { success = false, message = "حساب إيرادات المبيعات غير معرف في تعيينات الحسابات." });
                }

                var vatAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "ضريبة القيمة المضافة المستحقة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (vatAccountId == 0 && invoicedetails.Any(d => d.VatAmount > 0))
                {
                    return Json(new { success = false, message = "حساب ضريبة القيمة المضافة غير معرف في تعيينات الحسابات." });
                }
                if (vatAccountId != 6 && invoicedetails.Any(d => d.VatAmount > 0))
                {
                    return Json(new { success = false, message = $"حساب ضريبة القيمة المضافة يجب أن يكون child_account_id = 6، ولكن تم العثور على {vatAccountId}." });
                }

                var accountsReceivableAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "الحسابات المستحقة القبض")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (accountsReceivableAccountId == 0 && paymentEntries.Any(p => p.PaymentMethod == "آجل"))
                {
                    return Json(new { success = false, message = "حساب الحسابات المستحقة القبض غير معرف في تعيينات الحسابات." });
                }

                var cashAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "النقدية بالصندوق")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cashAccountId == 0 && paymentEntries.Any(p => p.PaymentMethod == "كاش"))
                {
                    return Json(new { success = false, message = "حساب النقدية بالصندوق غير معرف في تعيينات الحسابات." });
                }

                var bankAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "النقدية بالبنك")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (bankAccountId == 0 && paymentEntries.Any(p => p.PaymentMethod == "تحويل بنكي"))
                {
                    return Json(new { success = false, message = "حساب النقدية بالبنك غير معرف في تعيينات الحسابات." });
                }

                var inventoryAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "المخزون")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (inventoryAccountId == 0)
                {
                    return Json(new { success = false, message = "حساب المخزون غير معرف في تعيينات الحسابات." });
                }

                var cogsAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "تكلفة البضاعة المباعة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cogsAccountId == 0)
                {
                    return Json(new { success = false, message = "حساب تكلفة البضاعة المباعة غير معرف في تعيينات الحسابات." });
                }

                // Initialize journal entries list
                var journalEntries = new List<JournalEntry>();

                // Add journal entry for sales revenue (credit)
                journalEntries.Add(new JournalEntry
                {
                    EntryDate = DateOnly.FromDateTime(DateTime.Now),
                    Description = $"فاتورة مبيعات رقم {invoice.InvoiceId}",
                    DebitAmount = 0,
                    CreditAmount = invoice.Subtotal,
                    UserId = userId.Value,
                    CostCenterId = 0,
                    CreatedAt = DateTime.Now,
                    InvoiceId = invoice.InvoiceId,
                    InvoiceType = "بيع",
                    ChildAccountId = salesAccountId // e.g., 7
                });

                // Add journal entries for VAT per product (credit)
                foreach (var detail in invoicedetails)
                {
                    if (detail.VatAmount > 0)
                    {
                        // Check for existing conflicting VAT entries
                        var existingVatEntry = await _context.JournalEntries
                            .AnyAsync(je => je.InvoiceId == invoice.InvoiceId && je.InvoiceType == "بيع" && je.ChildAccountId == vatAccountId && je.Id == detail.ProductId);
                        if (existingVatEntry)
                        {
                            Console.WriteLine($"Warning: Duplicate VAT entry detected for InvoiceId={invoice.InvoiceId}, ProductId={detail.ProductId}, ChildAccountId={vatAccountId}");
                        }

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مبيعات - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                            DebitAmount = 0,
                            CreditAmount = detail.VatAmount ?? 0m,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = vatAccountId // e.g., 6
                        });
                        Console.WriteLine($"VAT Journal Entry: InvoiceId={invoice.InvoiceId}, ProductId={detail.ProductId}, VatAmount={detail.VatAmount}, ChildAccountId={vatAccountId}");
                    }
                }

                // Add journal entries for inventory and COGS
                decimal totalCogs = 0;
                foreach (var detail in invoicedetails)
                {
                    decimal cogsAmount = detail.Quantity * productCosts[detail.ProductId.Value];
                    totalCogs += cogsAmount;
                    Console.WriteLine($"COGS for ProductId={detail.ProductId}: Quantity={detail.Quantity}, PurchasePrice={productCosts[detail.ProductId.Value]}, COGSAmount={cogsAmount}");

                    // Debit COGS (Expense, increases by debit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تكلفة البضاعة المباعة - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                        DebitAmount = cogsAmount,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "بيع",
                        ChildAccountId = cogsAccountId // e.g., 9
                    });

                    // Credit Inventory (Asset, decreases by credit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تخفيض المخزون - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                        DebitAmount = 0,
                        CreditAmount = cogsAmount,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "بيع",
                        ChildAccountId = inventoryAccountId // e.g., 3
                    });
                }

                // Add journal entries for payments (debit)
                foreach (var entry in paymentEntries)
                {
                    if (entry.PaymentMethod == "كاش" && entry.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"دفع كاش - فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = entry.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = cashAccountId // e.g., 1
                        });
                        Console.WriteLine($"Cash Payment: AccountId={entry.AccountId}, ChildAccountId={cashAccountId}, Amount={entry.Amount}");
                    }
                    else if (entry.PaymentMethod == "تحويل بنكي" && entry.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"دفع بنكي - فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = entry.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = bankAccountId // e.g., 2
                        });
                        Console.WriteLine($"Bank Payment: AccountId={entry.AccountId}, ChildAccountId={bankAccountId}, Amount={entry.Amount}");
                    }
                    else if (entry.PaymentMethod == "آجل" && entry.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد آجل للعميل - فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = entry.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = accountsReceivableAccountId // e.g., 4
                        });
                        Console.WriteLine($"Credit Payment: ChildAccountId={accountsReceivableAccountId}, Amount={entry.Amount}");
                    }
                }

                // Log and save journal entries
                Console.WriteLine($"Saving {journalEntries.Count} JournalEntries");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }
                _context.JournalEntries.AddRange(journalEntries);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log saved journal entries
                var savedJournalEntries = await _context.JournalEntries
                    .Where(j => j.UserId == userId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now))
                    .ToListAsync();
                Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                foreach (var entry in savedJournalEntries)
                {
                    Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, EntryDate={entry.EntryDate}, Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }

                return Json(new { success = true, message = "تم حفظ الفاتورة وتفاصيلها والدفع وقيود اليومية بنجاح!" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DbUpdateException in Create POST: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.",
                    errorDetails = new
                    {
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in Create POST: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء الحفظ.",
                    errorDetails = new
                    {
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }


        public class PaymentEntry
        {
            public string PaymentMethod { get; set; }
            public decimal Amount { get; set; }
            public DateTime? DueDate { get; set; }
            public DateTime? PaidDate { get; set; }
            public int? AccountId { get; set; } // Changed to AccountId to match model
        }
        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", invoice.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Users, "UserId", "Email", invoice.EmployeeId);
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,CustomerId,EmployeeId,InvoiceDate,Status,PaymentMethod,SubscriptionPlan,Subtotal,Vatrate,Vatamount,TotalAmount,CreatedAt")] Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.InvoiceId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", invoice.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Users, "UserId", "Email", invoice.EmployeeId);
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invoices == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invoices == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Invoices'  is null.");
            }
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return (_context.Invoices?.Any(e => e.InvoiceId == id)).GetValueOrDefault();
        }









        public IActionResult StoreInvoiceId(int invoiceId)
        {
            // Store the invoiceId in the session
            HttpContext.Session.SetInt32("InvoiceId", invoiceId);

            // Redirect to the Details action in the Invoicedetails controller
            return RedirectToAction("Details", "Invoicedetails");
        }





        // GET: Invoices/CreatePurchase
        public IActionResult CreatePurshase()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var userStorehouses = _context.Storehouses?.Where(s => s.UserId == userId).ToList() ?? new List<Storehouse>();
                var userBranches = _context.Branches?.Where(b => b.UserId == userId).ToList() ?? new List<Branch>();
                var userProducts = _context.Products?.Where(p => p.UserId == userId).ToList() ?? new List<Product>();
                var userEmployees = _context.Employees
                    ?.Where(e => e.UserId == userId)
                    .Select(e => new EmployeeDto
                    {
                        EmployeeId = e.EmployeeId,
                        FullName = e.FullName ?? $"موظف بدون اسم (ID: {e.EmployeeId})"
                    })
                    .ToList() ?? new List<EmployeeDto>();

                var cashAccounts = _context.Cashandbankaccounts
                    ?.Where(a => a.UserId == userId && a.AccountType == "خزينة" && a.Status == "نشط")
                    .Select(a => new AccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName ?? $"حساب بدون اسم (ID: {a.AccountId})"
                    })
                    .ToList() ?? new List<AccountDto>();

                var bankAccounts = _context.Cashandbankaccounts
                    ?.Where(a => a.UserId == userId && a.AccountType == "حساب بنكي" && a.Status == "نشط")
                    .Select(a => new AccountDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName ?? $"حساب بدون اسم (ID: {a.AccountId})"
                    })
                    .ToList() ?? new List<AccountDto>();

                var vendors = _context.Vendors
                    ?.Where(v => v.UserId == userId)
                    .Select(v => new VendorDto
                    {
                        VendorId = v.VendorId,
                        VendorName = v.VendorName ?? $"مورد بدون اسم (ID: {v.VendorId})"
                    })
                    .ToList() ?? new List<VendorDto>();

                ViewData["VendorId"] = new SelectList(vendors, "VendorId", "VendorName");
                ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName");
                ViewData["StorehouseId"] = new SelectList(userStorehouses, "StorehouseId", "StorehouseName");
                ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName");
                ViewData["ProductId"] = new SelectList(userProducts, "ProductId", "ProductName");

                ViewBag.CashAccounts = cashAccounts;
                ViewBag.BankAccounts = bankAccounts;

                ViewBag.ProductPurchasePrices = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.PurchasePrice);

                ViewBag.ProductVatrates = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.Vatrate ?? 0m);

                ViewBag.ProductBalances = userProducts
                    .Where(p => p != null)
                    .ToDictionary(p => p.ProductId, p => p.Balance);

                ViewData["CurrentUserId"] = userId;

                return View(new Invoice { InvoiceDate = DateTime.Now, InvoiceType = "شراء" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreatePurchase GET: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل صفحة إنشاء فاتورة الشراء." });
            }
        }

        // POST: Invoices/CreatePurchase
        [HttpPost]
        public async Task<IActionResult> CreatePurchase(Invoice invoice, [FromForm] List<Invoicedetail> invoicedetails, [FromForm] List<PaymentEntry> paymentEntries)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine("ModelState Errors: " + string.Join("; ", errors));
                return Json(new { success = false, message = "بيانات غير صالحة.", errors });
            }

            if (invoicedetails == null || !invoicedetails.Any())
            {
                return Json(new { success = false, message = "يجب إدخال تفاصيل الفاتورة." });
            }

            // Log raw form data
            Console.WriteLine("Raw Form Data:");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"Key: {key}, Value: {string.Join(", ", Request.Form[key])}");
            }

            // Log raw invoicedetails
            Console.WriteLine($"Raw invoicedetails count: {invoicedetails.Count}");
            foreach (var detail in invoicedetails)
            {
                Console.WriteLine($"Raw Invoicedetail: ProductId={detail.ProductId}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, VatAmount={detail.VatAmount}, LineTotal={detail.LineTotal}, InvoiceTyping={detail.InvoiceTyping ?? "null"}");
            }

            // Remove exact duplicates from invoicedetails
            var distinctDetails = invoicedetails
                .GroupBy(d => new { d.ProductId, d.Quantity, d.UnitPrice, d.VatAmount, d.LineTotal, InvoiceTyping = d.InvoiceTyping ?? "null" })
                .Select(g => g.First())
                .ToList();

            if (distinctDetails.Count < invoicedetails.Count)
            {
                Console.WriteLine($"Exact duplicates detected in input. Reduced from {invoicedetails.Count} to {distinctDetails.Count} entries.");
                invoicedetails = distinctDetails;
            }

            // Check for duplicate ProductIds
            var duplicateProductIds = invoicedetails
                .GroupBy(d => d.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
                .ToList();

            if (duplicateProductIds.Any())
            {
                Console.WriteLine($"Duplicate ProductIds detected: {string.Join("; ", duplicateProductIds.Select(d => $"ProductId={d.ProductId}, Count={d.Count}"))}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate EmployeeId
                if (invoice.EmployeeId.HasValue)
                {
                    var employee = await _context.Employees
                        .AsNoTracking()
                        .Where(e => e.EmployeeId == invoice.EmployeeId.Value && e.UserId == userId)
                        .FirstOrDefaultAsync();
                    if (employee == null)
                    {
                        Console.WriteLine($"Invalid EmployeeId: {invoice.EmployeeId}");
                        return Json(new { success = false, message = $"الموظف برقم {invoice.EmployeeId.Value} غير موجود." });
                    }
                    Console.WriteLine($"EmployeeId: {invoice.EmployeeId} validated for UserId: {userId}");
                }

                // Filter and merge duplicates, ensuring InvoiceTyping == "شراء"
                var mergedDetails = invoicedetails
                    .Where(d => d.ProductId > 0 && d.Quantity > 0 && d.UnitPrice > 0 && d.LineTotal > 0 && d.InvoiceTyping == "شراء")
                    .GroupBy(d => d.ProductId)
                    .Select(g => new Invoicedetail
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(d => d.Quantity),
                        UnitPrice = g.First().UnitPrice,
                        VatAmount = g.Sum(d => d.VatAmount ?? 0m),
                        LineTotal = g.Sum(d => d.LineTotal),
                        InvoiceTyping = "شراء"
                    })
                    .ToList();

                if (!mergedDetails.Any())
                {
                    return Json(new { success = false, message = "يجب إدخال تفاصيل فاتورة صالحة (معرف المنتج والكمية مطلوبان)." });
                }

                // Log merged details before adding
                Console.WriteLine("Adding merged invoicedetails:");
                foreach (var d in mergedDetails)
                {
                    Console.WriteLine($"ProductId={d.ProductId}, Quantity={d.Quantity}, LineTotal={d.LineTotal}, InvoiceTyping={d.InvoiceTyping}");
                }

                // Detach all tracked Invoicedetail entities to prevent duplicates
                var trackedInvoicedetails = _context.ChangeTracker.Entries<Invoicedetail>().ToList();
                foreach (var entry in trackedInvoicedetails)
                {
                    entry.State = EntityState.Detached;
                }

                // Clear navigation property to prevent original invoicedetails from being included
                invoice.Invoicedetails = null;

                invoice.UserId = userId;
                invoice.CreatedAt = DateTime.Now;
                invoice.InvoiceType = "شراء";

                if (invoice.BranchId.HasValue)
                {
                    var branch = await _context.Branches
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.BranchId == invoice.BranchId && b.UserId == userId);
                    if (branch == null)
                    {
                        return Json(new { success = false, message = "الفرع المحدد غير صالح." });
                    }
                }

                if (!invoice.StorehouseId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تحديد المخزن." });
                }

                if (invoice.VendorId.HasValue)
                {
                    var vendor = await _context.Vendors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.VendorId == invoice.VendorId && v.UserId == userId);
                    if (vendor == null)
                    {
                        return Json(new { success = false, message = "المورد المحدد غير صالح." });
                    }
                }

                // Set CashAccountId and BankAccountId from payment entries
                var cashAccountId = paymentEntries?.FirstOrDefault(p => p.PaymentMethod == "كاش" && p.AccountId.HasValue)?.AccountId;
                var bankAccountId = paymentEntries?.FirstOrDefault(p => p.PaymentMethod == "تحويل بنكي" && p.AccountId.HasValue)?.AccountId;
                invoice.InvoiceCashAccountId = cashAccountId;
                invoice.InvoiceBankAccountId = bankAccountId;

                Console.WriteLine($"Saving Invoice: Id={invoice.InvoiceId}, UserId={invoice.UserId}, EmployeeId={invoice.EmployeeId}, StorehouseId={invoice.StorehouseId}, VendorId={invoice.VendorId}, BranchId={invoice.BranchId}, CashAccountId={invoice.InvoiceCashAccountId}, BankAccountId={invoice.InvoiceBankAccountId}");

                // Add and save invoice to generate InvoiceId
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                var inventories = new List<Inventory>();
                var productsToUpdate = new List<Product>();

                foreach (var detail in mergedDetails)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId && p.UserId == userId);
                    if (product == null)
                    {
                        return Json(new { success = false, message = $"المنتج برقم {detail.ProductId} غير موجود أو لا يخصك." });
                    }

                    if (detail.Quantity <= 0 || product.PurchasePrice <= 0)
                    {
                        return Json(new { success = false, message = "الكمية وسعر الشراء يجب أن يكونا أكبر من 0." });
                    }

                    detail.InvoiceId = invoice.InvoiceId;
                    detail.UnitPrice = product.PurchasePrice;
                    detail.InvoiceTyping = "شراء";

                    decimal vatRate = product.Vatrate ?? 0m;
                    decimal vatAmount = (detail.Quantity * product.PurchasePrice) * (vatRate / 100);
                    decimal expectedLineTotal = (detail.Quantity * product.PurchasePrice) + vatAmount;

                    if (Math.Abs(detail.LineTotal - expectedLineTotal) > 0.01m)
                    {
                        detail.LineTotal = expectedLineTotal;
                        detail.VatAmount = vatAmount;
                    }

                    // Add Inventory record
                    var inventory = new Inventory
                    {
                        ProductId = detail.ProductId,
                        OutgoingQuantity = 0,
                        IncomingQuantity = detail.Quantity,
                        UserId = userId.Value,
                        LastUpdated = DateTime.Now,
                        StorehouseId = invoice.StorehouseId.Value
                    };
                    inventories.Add(inventory);

                    Console.WriteLine($"Added Inventory: ProductId={inventory.ProductId}, IncomingQuantity={inventory.IncomingQuantity}, StorehouseId={inventory.StorehouseId}");

                    product.Balance += detail.Quantity;
                    productsToUpdate.Add(product);
                }

                invoice.Subtotal = mergedDetails.Sum(d => d.Quantity * d.UnitPrice);
                invoice.Vatamount = mergedDetails.Sum(d => d.VatAmount ?? 0m);
                invoice.TotalAmount = invoice.Subtotal + invoice.Vatamount;

                // Fetch ChildAccountIds using account_mappings
                var inventoryAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "المخزون")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (inventoryAccountId == 0)
                {
                    return Json(new { success = false, message = "حساب المخزون غير معرف في تعيينات الحسابات." });
                }

                var recoverableVatAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "ضريبة القيمة المضافة القابلة للاسترداد")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (recoverableVatAccountId == 0 && invoice.Vatamount > 0)
                {
                    return Json(new { success = false, message = "حساب ضريبة القيمة المضافة القابلة للاسترداد غير معرف في تعيينات الحسابات." });
                }

                var accountsPayableAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "الحسابات المستحقة الدفع")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (accountsPayableAccountId == 0 && paymentEntries.Any(p => p.PaymentMethod == "آجل"))
                {
                    return Json(new { success = false, message = "حساب الحسابات المستحقة الدفع غير معرف في تعيينات الحسابات." });
                }

                // Log account mappings
                Console.WriteLine($"Account Mappings: inventoryAccountId={inventoryAccountId}, recoverableVatAccountId={recoverableVatAccountId}, accountsPayableAccountId={accountsPayableAccountId}");

                // Initialize journal entries list
                var journalEntries = new List<JournalEntry>();

                // Add journal entry for inventory (debit)
                journalEntries.Add(new JournalEntry
                {
                    EntryDate = DateOnly.FromDateTime(DateTime.Now),
                    Description = $"فاتورة مشتريات رقم {invoice.InvoiceId} - مخزون",
                    DebitAmount = invoice.Subtotal,
                    CreditAmount = 0,
                    UserId = userId.Value,
                    CostCenterId = 0,
                    CreatedAt = DateTime.Now,
                    InvoiceId = invoice.InvoiceId,
                    InvoiceType = "شراء",
                    ChildAccountId = inventoryAccountId
                });

                // Add journal entry for VAT, if applicable (debit)
                if (invoice.Vatamount > 0)
                {
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"ضريبة مشتريات - فاتورة رقم {invoice.InvoiceId}",
                        DebitAmount = invoice.Vatamount ?? 0m,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "شراء",
                        ChildAccountId = recoverableVatAccountId
                    });
                }

                var payments = new List<Payment>();
                var paymentDetails = new List<Paymentdetail>();
                var accountsToUpdate = new List<Cashandbankaccount>();

                if (paymentEntries != null && paymentEntries.Any())
                {
                    var totalPaymentAmount = paymentEntries.Sum(p => p.Amount);
                    if (Math.Abs(totalPaymentAmount - invoice.TotalAmount.GetValueOrDefault()) > 0.01m)
                    {
                        return Json(new { success = false, message = "إجمالي مبالغ الدفع لا يساوي إجمالي الفاتورة." });
                    }

                    // Validate cash account exists and is active
                    if (cashAccountId.HasValue)
                    {
                        var cashAccount = await _context.Cashandbankaccounts
                            .FirstOrDefaultAsync(a => a.AccountId == cashAccountId && a.UserId == userId && a.AccountType == "خزينة" && a.Status == "نشط");
                        if (cashAccount == null)
                        {
                            return Json(new { success = false, message = "الحساب النقدي المحدد غير صالح." });
                        }
                    }

                    // Validate bank account exists and is active
                    if (bankAccountId.HasValue)
                    {
                        var bankAccount = await _context.Cashandbankaccounts
                            .FirstOrDefaultAsync(a => a.AccountId == bankAccountId && a.UserId == userId && a.AccountType == "حساب بنكي" && a.Status == "نشط");
                        if (bankAccount == null)
                        {
                            return Json(new { success = false, message = "الحساب البنكي المحدد غير صالح." });
                        }
                    }

                    Console.WriteLine($"Processing {paymentEntries.Count} PaymentEntries, TotalPaymentAmount={totalPaymentAmount}");

                    foreach (var entry in paymentEntries)
                    {
                        if (string.IsNullOrEmpty(entry.PaymentMethod) || entry.Amount <= 0)
                        {
                            return Json(new { success = false, message = "يجب إدخال طريقة دفع صالحة مع مبلغ أكبر من 0." });
                        }

                        if (entry.PaymentMethod == "آجل" && !entry.DueDate.HasValue)
                        {
                            return Json(new { success = false, message = "يجب تحديد تاريخ الاستحقاق لطريقة الدفع 'آجل'." });
                        }

                        if (entry.PaymentMethod != "آجل" && !entry.PaidDate.HasValue)
                        {
                            return Json(new { success = false, message = $"يجب تحديد تاريخ الدفع لطريقة الدفع '{entry.PaymentMethod}'." });
                        }

                        if ((entry.PaymentMethod == "كاش" || entry.PaymentMethod == "تحويل بنكي") && !entry.AccountId.HasValue)
                        {
                            return Json(new { success = false, message = $"يجب تحديد حساب لطريقة الدفع '{entry.PaymentMethod}'." });
                        }

                        // Add journal entries for payments (credit)
                        if (entry.PaymentMethod == "كاش" && entry.Amount > 0)
                        {
                            var cashChildAccountId = await _context.AccountMappings
                                .Where(m => m.TransactionType == "النقدية بالصندوق")
                                .Select(m => m.ChildAccountId)
                                .FirstOrDefaultAsync();
                            if (cashChildAccountId == 0)
                            {
                                return Json(new { success = false, message = "حساب النقدية بالصندوق غير معرف في تعيينات الحسابات." });
                            }

                            journalEntries.Add(new JournalEntry
                            {
                                EntryDate = DateOnly.FromDateTime(DateTime.Now),
                                Description = $"دفع كاش - فاتورة رقم {invoice.InvoiceId}",
                                DebitAmount = 0,
                                CreditAmount = entry.Amount,
                                UserId = userId.Value,
                                CostCenterId = 0,
                                CreatedAt = DateTime.Now,
                                InvoiceId = invoice.InvoiceId,
                                InvoiceType = "شراء",
                                ChildAccountId = cashChildAccountId
                            });
                            Console.WriteLine($"Cash Payment: AccountId={entry.AccountId}, child_account_id={cashChildAccountId}, Amount={entry.Amount}");
                            if (entry.AccountId == 11)
                            {
                                Console.WriteLine($"Processing cash payment for AccountId=11, child_account_id={cashChildAccountId}");
                            }
                        }
                        else if (entry.PaymentMethod == "تحويل بنكي" && entry.Amount > 0)
                        {
                            var bankChildAccountId = await _context.AccountMappings
                                .Where(m => m.TransactionType == "النقدية بالبنك")
                                .Select(m => m.ChildAccountId)
                                .FirstOrDefaultAsync();
                            if (bankChildAccountId == 0)
                            {
                                return Json(new { success = false, message = "حساب النقدية بالبنك غير معرف في تعيينات الحسابات." });
                            }

                            journalEntries.Add(new JournalEntry
                            {
                                EntryDate = DateOnly.FromDateTime(DateTime.Now),
                                Description = $"دفع بنكي - فاتورة رقم {invoice.InvoiceId}",
                                DebitAmount = 0,
                                CreditAmount = entry.Amount,
                                UserId = userId.Value,
                                CostCenterId = 0,
                                CreatedAt = DateTime.Now,
                                InvoiceId = invoice.InvoiceId,
                                InvoiceType = "شراء",
                                ChildAccountId = bankChildAccountId
                            });
                            Console.WriteLine($"Bank Payment: AccountId={entry.AccountId}, child_account_id={bankChildAccountId}, Amount={entry.Amount}");
                        }
                        else if (entry.PaymentMethod == "آجل" && entry.Amount > 0)
                        {
                            journalEntries.Add(new JournalEntry
                            {
                                EntryDate = DateOnly.FromDateTime(DateTime.Now),
                                Description = $"رصيد آجل للمورد - فاتورة رقم {invoice.InvoiceId}",
                                DebitAmount = 0,
                                CreditAmount = entry.Amount,
                                UserId = userId.Value,
                                CostCenterId = 0,
                                CreatedAt = DateTime.Now,
                                InvoiceId = invoice.InvoiceId,
                                InvoiceType = "شراء",
                                ChildAccountId = accountsPayableAccountId
                            });
                            Console.WriteLine($"Credit Payment: child_account_id={accountsPayableAccountId}, Amount={entry.Amount}");
                        }

                        var payment = new Payment
                        {
                            VendorId = invoice.VendorId,
                            UserId = userId.Value,
                            Paydate = DateOnly.FromDateTime(DateTime.Now),
                            TotalAmount = totalPaymentAmount,
                            PaymentStatus = "Paid",
                            InvoiceId = invoice.InvoiceId,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        payments.Add(payment);

                        var paymentDetail = new Paymentdetail
                        {
                            UserId = userId.Value,
                            Description = entry.PaymentMethod,
                            Amount = entry.Amount,
                            DueDate = entry.PaymentMethod == "آجل" && entry.DueDate.HasValue ? DateOnly.FromDateTime(entry.DueDate.Value) : null,
                            IsPaid = entry.PaymentMethod != "آجل",
                            PaidDate = entry.PaymentMethod != "آجل" && entry.PaidDate.HasValue ? DateOnly.FromDateTime(entry.PaidDate.Value) : null,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        paymentDetails.Add(paymentDetail);

                        // Update account balances for cash and bank transfers
                        if (entry.PaymentMethod == "كاش" && entry.AccountId.HasValue)
                        {
                            var cashAccount = await _context.Cashandbankaccounts
                                .FirstOrDefaultAsync(a => a.AccountId == entry.AccountId);
                            if (cashAccount != null)
                            {
                                if (cashAccount.ChildAccountId == null && cashAccount.AccountType == "خزينة")
                                {
                                    cashAccount.ChildAccountId = 1; // النقدية بالصندوق
                                    Console.WriteLine($"Assigned child_account_id=1 to cashandbankaccounts AccountId={cashAccount.AccountId}");
                                }
                                cashAccount.Balance -= entry.Amount;
                                accountsToUpdate.Add(cashAccount);
                                Console.WriteLine($"Updated cashandbankaccounts: AccountId={cashAccount.AccountId}, child_account_id={cashAccount.ChildAccountId}, NewBalance={cashAccount.Balance}");
                            }
                        }
                        else if (entry.PaymentMethod == "تحويل بنكي" && entry.AccountId.HasValue)
                        {
                            var bankAccount = await _context.Cashandbankaccounts
                                .FirstOrDefaultAsync(a => a.AccountId == entry.AccountId);
                            if (bankAccount != null)
                            {
                                if (bankAccount.ChildAccountId == null && bankAccount.AccountType == "حساب بنكي")
                                {
                                    bankAccount.ChildAccountId = 2; // النقدية بالبنك
                                    Console.WriteLine($"Assigned child_account_id=2 to cashandbankaccounts AccountId={bankAccount.AccountId}");
                                }
                                bankAccount.Balance -= entry.Amount;
                                accountsToUpdate.Add(bankAccount);
                                Console.WriteLine($"Updated cashandbankaccounts: AccountId={bankAccount.AccountId}, child_account_id={bankAccount.ChildAccountId}, NewBalance={bankAccount.Balance}");
                            }
                        }
                    }
                }

                // Log context state before saving
                var detailsInContext = _context.ChangeTracker.Entries<Invoicedetail>()
                    .Select(e => new { e.Entity.ProductId, e.Entity.Quantity, e.Entity.InvoiceId })
                    .ToList();
                Console.WriteLine($"Invoicedetails in context before SaveChanges: {detailsInContext.Count}");
                foreach (var d in detailsInContext)
                {
                    Console.WriteLine($"Context Invoicedetail: ProductId={d.ProductId}, Quantity={d.Quantity}, InvoiceId={d.InvoiceId}");
                }

                // Debug tracked Invoicedetails
                var allTrackedDetails = _context.ChangeTracker.Entries<Invoicedetail>()
                    .Select(e => new { e.Entity.ProductId, e.Entity.Quantity, e.State })
                    .ToList();
                Console.WriteLine("Tracked Invoicedetails before SaveChanges:");
                foreach (var d in allTrackedDetails)
                {
                    Console.WriteLine($"ProductId={d.ProductId}, Quantity={d.Quantity}, State={d.State}");
                }

                // Log journal entries before saving
                Console.WriteLine($"Saving {journalEntries.Count} JournalEntries");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"JournalEntry: Description={entry.Description}, debit_amount={entry.DebitAmount}, credit_amount={entry.CreditAmount}, child_account_id={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}");
                }

                // Add entities to context
                _context.Invoicedetails.AddRange(mergedDetails);
                _context.Inventories.AddRange(inventories);
                _context.Products.UpdateRange(productsToUpdate);
                _context.Invoices.Update(invoice);
                _context.JournalEntries.AddRange(journalEntries);

                if (payments.Any())
                {
                    _context.Payments.AddRange(payments);
                    await _context.SaveChangesAsync(); // Save Payments to generate Payid

                    foreach (var paymentDetail in paymentDetails)
                    {
                        paymentDetail.Payid = payments.First().Payid; // Assign Payid
                    }

                    _context.Paymentdetails.AddRange(paymentDetails);
                    _context.Cashandbankaccounts.UpdateRange(accountsToUpdate);
                }

                await _context.SaveChangesAsync();

                // Log Inventory state after SaveChanges
                var savedInventories = await _context.Inventories
                    .Where(i => i.StorehouseId == invoice.StorehouseId && mergedDetails.Select(d => d.ProductId).Contains(i.ProductId))
                    .ToListAsync();
                Console.WriteLine($"Saved Inventories count: {savedInventories.Count}");
                foreach (var inv in savedInventories)
                {
                    Console.WriteLine($"Saved Inventory: ProductId={inv.ProductId}, IncomingQuantity={inv.IncomingQuantity}, OutgoingQuantity={inv.OutgoingQuantity}, StorehouseId={inv.StorehouseId}, LastUpdated={inv.LastUpdated}");
                }

                // Log final database state for Invoicedetails
                var savedDetails = await _context.Invoicedetails
                    .Where(d => d.InvoiceId == invoice.InvoiceId)
                    .ToListAsync();
                Console.WriteLine($"Saved Invoicedetails count: {savedDetails.Count}");
                foreach (var detail in savedDetails)
                {
                    Console.WriteLine($"Saved Invoicedetail: DetailId={detail.DetailId}, InvoiceId={detail.InvoiceId}, ProductId={detail.ProductId}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, VatAmount={detail.VatAmount}, LineTotal={detail.LineTotal}, InvoiceTyping={detail.InvoiceTyping}");
                }

                // Log saved journal entries
                var savedJournalEntries = await _context.JournalEntries
                    .Where(j => j.UserId == userId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now))
                    .ToListAsync();
                Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                foreach (var entry in savedJournalEntries)
                {
                    Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, EntryDate={entry.EntryDate}, Description={entry.Description}, debit_amount={entry.DebitAmount}, credit_amount={entry.CreditAmount}, child_account_id={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}");
                }

                await transaction.CommitAsync();
                return Json(new { success = true, message = "تم حفظ فاتورة الشراء وتفاصيلها والدفع وقيود اليومية بنجاح!" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DbUpdateException in CreatePurchase POST: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.",
                    errorDetails = new
                    {
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in CreatePurchase POST: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء الحفظ.",
                    errorDetails = new
                    {
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }

        public class VendorDto
        {
            public int VendorId { get; set; }
            public string VendorName { get; set; }
        }








        public IActionResult NetIncome(int? year)
        {
            // Use current year if no year is provided
            int selectedYear = year ?? DateTime.Now.Year;

            // Fetch invoices for the selected year
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CreatedAt.HasValue && i.CreatedAt.Value.Year == selectedYear)
                .Select(i => new
                {
                    Invoice = i,
                    TotalAmount = _context.Invoicedetails
                        .Where(d => d.InvoiceId == i.InvoiceId)
                        .Sum(d => d.LineTotal),
                    InvoiceType = i.InvoiceType
                })
                .ToList();

            // Calculate total sales and purchases
            var totalSales = invoices.Where(i => i.InvoiceType == "بيع").Sum(i => i.TotalAmount);
            var totalPurchases = invoices.Where(i => i.InvoiceType == "شراء").Sum(i => i.TotalAmount);
            var netIncome = totalSales - totalPurchases;

            // Pass data to View using ViewBag
            ViewBag.Year = selectedYear;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalPurchases = totalPurchases;
            ViewBag.NetIncome = netIncome;

            return View();
        }











        public IActionResult ExportNetIncomeToExcel(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CreatedAt.HasValue && i.CreatedAt.Value.Year == selectedYear)
                .Select(i => new
                {
                    Invoice = i,
                    TotalAmount = _context.Invoicedetails
                        .Where(d => d.InvoiceId == i.InvoiceId)
                        .Sum(d => d.LineTotal),
                    InvoiceType = i.InvoiceType
                })
                .ToList();

            var totalSales = invoices.Where(i => i.InvoiceType == "بيع").Sum(i => i.TotalAmount);
            var totalPurchases = invoices.Where(i => i.InvoiceType == "شراء").Sum(i => i.TotalAmount);
            var netIncome = totalSales - totalPurchases;

            // ✅ Set the License Context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Net Income");

                worksheet.Cells[1, 1].Value = "السنة";
                worksheet.Cells[1, 2].Value = "إجمالي المبيعات (بيع)";
                worksheet.Cells[1, 3].Value = "إجمالي المشتريات (شراء)";
                worksheet.Cells[1, 4].Value = "صافي الدخل";

                worksheet.Cells[2, 1].Value = selectedYear;
                worksheet.Cells[2, 2].Value = totalSales;
                worksheet.Cells[2, 3].Value = totalPurchases;
                worksheet.Cells[2, 4].Value = netIncome;

                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"NetIncome_{selectedYear}.xlsx");
            }
        }







        public async Task<IActionResult> IndexPurshasing()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Session UserId: {userId}");

            if (!userId.HasValue)
            {
                Console.WriteLine("Error: UserId is null, redirecting to login.");
                return RedirectToAction("Login", "Usered");
            }

            // Check raw invoice count
            var userInvoicesCount = await _context.Invoices
                .CountAsync(i => i.UserId == userId.Value && i.InvoiceType == "شراء");
            Console.WriteLine($"Found {userInvoicesCount} purchase invoices for UserId: {userId}");

            if (userInvoicesCount == 0)
            {
                Console.WriteLine("No purchase invoices found. Checking all invoices for UserId.");
                var allInvoices = await _context.Invoices
                    .Where(i => i.UserId == userId)
                    .Select(i => new { i.InvoiceId, i.InvoiceType, i.UserId })
                    .ToListAsync();
                Console.WriteLine($"All invoices for UserId {userId}: {string.Join(", ", allInvoices.Select(i => $"InvoiceId={i.InvoiceId}, Type={i.InvoiceType}"))}");
            }

            // Get all return quantities grouped by InvoiceId and ProductId
            var returnQuantities = await _context.Invoicedetails
                .Where(d => d.InvoiceTyping == "مرتجع من الشراء")
                .GroupBy(d => new { d.InvoiceId, d.ProductId })
                .Select(g => new
                {
                    g.Key.InvoiceId,
                    g.Key.ProductId,
                    ReturnQuantity = g.Sum(d => d.Quantity)
                })
                .ToListAsync();

            // Main query using LINQ query syntax
            var purchaseData = await (from i in _context.Invoices
                                      where i.UserId == userId.Value && i.InvoiceType == "شراء"
                                      join id in _context.Invoicedetails on i.InvoiceId equals id.InvoiceId into invoiceDetailsGroup
                                      from id in invoiceDetailsGroup.DefaultIfEmpty()
                                      join p in _context.Products on (id != null ? id.ProductId : 0) equals p.ProductId into productGroup
                                      from p in productGroup.DefaultIfEmpty()
                                      join c in _context.Customers on i.CustomerId equals c.CustomerId into customerGroup
                                      from c in customerGroup.DefaultIfEmpty()
                                      select new
                                      {
                                          i.InvoiceId,
                                          i.InvoiceDate,
                                          CustomerName = c != null ? c.CustomerName : "Unknown",
                                          ProductName = p != null ? p.ProductName : "Unknown",
                                          ProductId = p != null ? p.ProductId : 0,
                                          VatRate = p != null ? p.Vatrate : 0m,
                                          PurchasePrice = p != null ? p.PurchasePrice : 0m,
                                          InvoiceTyping = id != null ? id.InvoiceTyping : null,
                                          Quantity = id != null ? id.Quantity : 0
                                      }).ToListAsync();

            var report = purchaseData
                .GroupBy(x => new
                {
                    x.InvoiceId,
                    x.InvoiceDate,
                    x.CustomerName,
                    x.ProductName,
                    x.ProductId,
                    x.VatRate,
                    x.PurchasePrice
                })
                .Select(g =>
                {
                    int totalPurchased = g.Where(x => x.InvoiceTyping == "شراء").Sum(x => x.Quantity);
                    int totalReturned = returnQuantities
                        .Where(r => r.InvoiceId == g.Key.InvoiceId && r.ProductId == g.Key.ProductId)
                        .Sum(r => r.ReturnQuantity);

                    string returnStatus;
                    if (totalReturned == totalPurchased && totalReturned > 0)
                    {
                        returnStatus = "تم الرجوع بالكامل";
                    }
                    else if (totalReturned > 0 && totalReturned < totalPurchased)
                    {
                        returnStatus = "مرتجع جزئي";
                    }
                    else
                    {
                        returnStatus = "لم يتم الرجوع";
                    }

                    return new PurchaseInvoiceReportViewModel
                    {
                        InvoiceID = g.Key.InvoiceId,
                        InvoiceDate = g.Key.InvoiceDate,
                        CustomerName = g.Key.CustomerName,
                        ProductName = g.Key.ProductName,
                        ProductId = g.Key.ProductId,
                        VatRate = g.Key.VatRate ?? 0m,
                        PurchasePrice = g.Key.PurchasePrice,
                        TotalPurchased = totalPurchased,
                        TotalReturned = totalReturned,
                        NetPurchased = totalPurchased - totalReturned,
                        ReturnStatus = returnStatus
                    };
                })
                .Where(r => r.TotalPurchased > 0 || r.TotalReturned > 0)
                .OrderByDescending(r => r.InvoiceDate)
                .ToList();

            Console.WriteLine($"Report contains {report.Count} items");
            if (report.Any())
            {
                var sample = report.First();
                Console.WriteLine($"Sample report item: InvoiceID={sample.InvoiceID}, ProductName={sample.ProductName}, TotalPurchased={sample.TotalPurchased}, ReturnStatus={sample.ReturnStatus}");
            }

            return View(report);
        }





        public async Task<IActionResult> IndexSales()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"Session UserId: {userId}");

            if (!userId.HasValue)
            {
                Console.WriteLine("Error: UserId is null, redirecting to login.");
                return RedirectToAction("Login", "Usered");
            }

            var userInvoicesCount = await _context.Invoices
                .CountAsync(i => i.UserId == userId.Value && (i.InvoiceType == "بيع" || i.InvoiceType == "مرتجع"));
            Console.WriteLine($"Found {userInvoicesCount} sales invoices for UserId: {userId}");

            if (userInvoicesCount == 0)
            {
                Console.WriteLine("No sales invoices found. Checking all invoices for UserId.");
                var allInvoices = await _context.Invoices
                    .Where(i => i.UserId == userId)
                    .Select(i => new { i.InvoiceId, i.InvoiceType, i.UserId })
                    .ToListAsync();
                Console.WriteLine($"All invoices for UserId {userId}: {string.Join(", ", allInvoices.Select(i => $"InvoiceId={i.InvoiceId}, Type={i.InvoiceType}"))}");
            }

            // Get all return quantities grouped by InvoiceId and ProductId
            var returnQuantities = await _context.Invoicedetails
                .Where(d => d.InvoiceTyping == "مرتجع من البيع")
                .GroupBy(d => new { d.InvoiceId, d.ProductId })
                .Select(g => new
                {
                    g.Key.InvoiceId,
                    g.Key.ProductId,
                    ReturnQuantity = g.Sum(d => d.Quantity)
                })
                .ToListAsync();

            // Aggregate sales data by InvoiceId, ProductId, and UnitPrice
            var salesData = await (from i in _context.Invoices
                                   where i.UserId == userId.Value && (i.InvoiceType == "بيع" || i.InvoiceType == "مرتجع")
                                   join id in _context.Invoicedetails on i.InvoiceId equals id.InvoiceId
                                   where id.InvoiceTyping == "بيع"
                                   join p in _context.Products on id.ProductId equals p.ProductId
                                   join c in _context.Customers on i.CustomerId equals c.CustomerId into customerGroup
                                   from c in customerGroup.DefaultIfEmpty()
                                   group new { i, id, p, c } by new
                                   {
                                       i.InvoiceId,
                                       i.InvoiceDate,
                                       CustomerName = c != null ? c.CustomerName : "Unknown",
                                       p.ProductName,
                                       p.ProductId,
                                       id.UnitPrice
                                   } into g
                                   select new
                                   {
                                       g.Key.InvoiceId,
                                       g.Key.InvoiceDate,
                                       g.Key.CustomerName,
                                       g.Key.ProductName,
                                       g.Key.ProductId,
                                       g.Key.UnitPrice,
                                       VatAmount = g.Sum(x => x.id.VatAmount.GetValueOrDefault(0m)),
                                       LineTotal = g.Sum(x => x.id.LineTotal),
                                       TotalSold = g.Sum(x => x.id.Quantity)
                                   }).ToListAsync();

            var report = salesData
                .Select(s =>
                {
                    int totalReturned = returnQuantities
                        .Where(r => r.InvoiceId == s.InvoiceId && r.ProductId == s.ProductId)
                        .Sum(r => r.ReturnQuantity);

                    string returnStatus;
                    if (totalReturned == s.TotalSold && totalReturned > 0)
                    {
                        returnStatus = "تم الرجوع بالكامل";
                    }
                    else if (totalReturned > 0 && totalReturned < s.TotalSold)
                    {
                        returnStatus = "مرتجع جزئي";
                    }
                    else
                    {
                        returnStatus = "لم يتم الرجوع";
                    }

                    decimal netTotalWithVat = s.TotalSold > 0 ? s.LineTotal * (s.TotalSold - totalReturned) / s.TotalSold : 0;
                    Console.WriteLine($"InvoiceID={s.InvoiceId}, ProductID={s.ProductId}, TotalSold={s.TotalSold}, TotalReturned={totalReturned}, LineTotal={s.LineTotal}, NetSold={s.TotalSold - totalReturned}, NetTotalWithVat={netTotalWithVat}");

                    return new SalesReportViewModel
                    {
                        InvoiceID = s.InvoiceId,
                        InvoiceDate = s.InvoiceDate,
                        CustomerName = s.CustomerName,
                        ProductName = s.ProductName,
                        ProductId = s.ProductId,
                        UnitPrice = s.UnitPrice,
                        VatAmount = s.VatAmount,
                        LineTotal = s.LineTotal,
                        TotalSold = s.TotalSold,
                        TotalReturned = totalReturned,
                        NetSold = s.TotalSold - totalReturned,
                        ReturnStatus = returnStatus
                    };
                })
                .Where(r => r.TotalSold > 0 || r.TotalReturned > 0)
                .OrderByDescending(r => r.InvoiceDate)
                .ToList();

             
            if (report.Any())
            {
                var sample = report.First();
                Console.WriteLine($"Sample report item: InvoiceID={sample.InvoiceID}, ProductName={sample.ProductName}, TotalSold={sample.TotalSold}, ReturnStatus={sample.ReturnStatus}, LineTotal={sample.LineTotal}");
            }

            return View(report);
        }





        public async Task<IActionResult> IndexReturns()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // Debug output
            Console.WriteLine($"Session UserId: {userId}");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Debug: Check if any return invoices exist for this user
            var userReturnsCount = await _context.Invoices
                .CountAsync(i => i.UserId == userId &&
                                _context.Invoicedetails.Any(id => id.InvoiceId == i.InvoiceId &&
                                                                id.InvoiceTyping == "مرتجع من الشراء"));
            Console.WriteLine($"Found {userReturnsCount} return invoices for user {userId}");

            var purchaseReturnsReport = await _context.Invoices
                .Where(i => i.UserId == userId) // Filter by current user
                .Join(_context.Invoicedetails,
                    i => i.InvoiceId,
                    id => id.InvoiceId,
                    (i, id) => new { i, id })
                .Where(x => x.id.InvoiceTyping == "مرتجع من الشراء") // Filter return purchases
                .Join(_context.Products,
                    temp => temp.id.ProductId,
                    p => p.ProductId,
                    (temp, p) => new { temp.i, temp.id, p })
                .Join(_context.Customers,
                    temp => temp.i.CustomerId,
                    c => c.CustomerId,
                    (temp, c) => new { temp.i, temp.id, temp.p, c })
                .GroupBy(x => new
                {
                    x.i.InvoiceId,
                    x.i.InvoiceDate,
                    x.c.CustomerName,
                    x.p.ProductName,
                    PurchasePrice = (decimal?)x.p.PurchasePrice, // Make nullable
                    Vatrate = (decimal?)x.p.Vatrate // Make nullable
                })
                .Select(g => new PurchaseReturnsReportViewModel
                {
                    InvoiceID = g.Key.InvoiceId,
                    InvoiceDate = g.Key.InvoiceDate,
                    CustomerName = g.Key.CustomerName,
                    ProductName = g.Key.ProductName,
                    TotalReturned = g.Sum(x => x.id.Quantity),
                    PurchasePrice = g.Key.PurchasePrice ?? 0m, // Handle nulls
                    Vatrate = g.Key.Vatrate ?? 0m // Handle nulls
                })
                .ToListAsync();

            // Debug: Check the final report count
            Console.WriteLine($"Report contains {purchaseReturnsReport.Count} items");

            return View(purchaseReturnsReport);
        }

        public async Task<IActionResult> IndexReturnSales()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var returnsReport = await _context.Invoices
                .Where(i => i.UserId == userId) // Filter by current user
                .Join(_context.Invoicedetails,
                    i => i.InvoiceId,
                    id => id.InvoiceId,
                    (i, id) => new { i, id })
                .Where(x => x.id.InvoiceTyping == "مرتجع من البيع") // Filter sales returns
                .Join(_context.Products,
                    temp => temp.id.ProductId,
                    p => p.ProductId,
                    (temp, p) => new { temp.i, temp.id, p })
                .Join(_context.Customers,
                    temp => temp.i.CustomerId,
                    c => c.CustomerId,
                    (temp, c) => new { temp.i, temp.id, temp.p, c })
                .GroupBy(x => new
                {
                    x.i.InvoiceId,
                    x.i.InvoiceDate,
                    x.c.CustomerName,
                    x.p.ProductName,
                    x.p.Price,
                    x.p.Vatrate,
                    x.p.PurchasePrice
                })
                .Select(g => new PurchaseReturnsReportViewModel
                {
                    InvoiceID = g.Key.InvoiceId,
                    InvoiceDate = g.Key.InvoiceDate,
                    CustomerName = g.Key.CustomerName,
                    ProductName = g.Key.ProductName,
                    TotalReturned = g.Sum(x => x.id.Quantity),
                    Price = g.Key.Price,
                    Vatrate = g.Key.Vatrate,
                    PurchasePrice = g.Key.PurchasePrice
                })
                .ToListAsync();

            return View(returnsReport);
        }


























































        public async Task<IActionResult> Indexdetaildsales()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"[Indexdetaildsales] Session UserId: {userId}");

            if (userId == null)
            {
                Console.WriteLine("[Indexdetaildsales] Error: UserId is null, redirecting to login.");
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                // Explicitly handle nullable int
                int userIdValue = userId.Value; // Already checked for null above

                // Log raw invoices
                var rawInvoices = await _context.Invoices
                    .Where(i => i.UserId == userIdValue && i.InvoiceType == "بيع")
                    .Select(i => new { i.InvoiceId, i.UserId, i.InvoiceType, i.CustomerId, i.EmployeeId, i.InvoiceDate })
                    .ToListAsync();
                Console.WriteLine($"[Indexdetaildsales] Found {rawInvoices.Count} raw sales invoices for UserId: {userIdValue}");
                foreach (var inv in rawInvoices)
                {
                    Console.WriteLine($"[Indexdetaildsales] Raw Invoice: InvoiceId={inv.InvoiceId}, UserId={inv.UserId}, InvoiceType={inv.InvoiceType}, CustomerId={inv.CustomerId}, EmployeeId={inv.EmployeeId}, InvoiceDate={inv.InvoiceDate}");
                }

                // Log unfiltered invoices
                var unfilteredInvoices = await _context.Invoices
                    .Where(i => i.InvoiceType == "بيع")
                    .Select(i => new { i.InvoiceId, i.UserId, i.InvoiceType, i.CustomerId, i.EmployeeId })
                    .Take(10)
                    .ToListAsync();
                Console.WriteLine($"[Indexdetaildsales] Found {unfilteredInvoices.Count} sales invoices without UserId filter:");
                foreach (var inv in unfilteredInvoices)
                {
                    Console.WriteLine($"[Indexdetaildsales] Unfiltered Invoice: InvoiceId={inv.InvoiceId}, UserId={inv.UserId}, InvoiceType={inv.InvoiceType}, CustomerId={inv.CustomerId}, EmployeeId={inv.EmployeeId}");
                }

                var invoices = await _context.Invoices
                    .Where(i => i.UserId == userIdValue && i.InvoiceType == "بيع")
                    .Join(
                        _context.Customers,
                        i => i.CustomerId,
                        c => c.CustomerId,
                        (i, c) => new { Invoice = i, CustomerName = c.CustomerName ?? $"عميل بدون اسم (ID: {c.CustomerId})" }
                    )
                    .Join(
                        _context.Employees,
                        ic => ic.Invoice.EmployeeId,
                        e => e.EmployeeId,
                        (ic, e) => new InvoiceViewModel
                        {
                            InvoiceId = ic.Invoice.InvoiceId,
                            CustomerName = ic.CustomerName,
                            EmployeeName = e.FullName ?? $"موظف بدون اسم (ID: {e.EmployeeId})",
                            InvoiceDate = ic.Invoice.InvoiceDate,
                            UserId = ic.Invoice.UserId??0 // For debugging
                        }
                    )
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                Console.WriteLine($"[Indexdetaildsales] Retrieved {invoices.Count} sales invoices for UserId: {userIdValue}");
                foreach (var inv in invoices)
                {
                    Console.WriteLine($"[Indexdetaildsales] Final Invoice: InvoiceId={inv.InvoiceId}, UserId={inv.UserId}, CustomerName={inv.CustomerName}, EmployeeName={inv.EmployeeName}, InvoiceDate={inv.InvoiceDate}");
                }

                // Fix CS1503: Call Distinct() explicitly
                Console.WriteLine($"[Indexdetaildsales] UserIds in result: {string.Join(", ", invoices.Select(i => i.UserId).Distinct().ToList())}");

                return View(invoices);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Indexdetaildsales] Error: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل قائمة الفواتير." });
            }
        }

        public async Task<IActionResult> PaymentDetails(int invoiceId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var paymentDetails = await _context.Payments
                    .Where(p => p.InvoiceId == invoiceId && p.UserId == userId)
                    .Join(
                        _context.Paymentdetails,
                        p => p.Payid,
                        pd => pd.Payid,
                        (p, pd) => new InvoiceViewModel
                        {
                            InvoiceId = p.InvoiceId, // Now int? in ViewModel, so no cast needed
                            TotalAmount = p.TotalAmount,
                            Description = pd.Description,
                            Amount = pd.Amount,
                            DueDate = pd.DueDate.HasValue ? pd.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                            PaidDate = pd.PaidDate.HasValue ? pd.PaidDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                            // Unused properties for this view
                            CustomerName = null,
                            EmployeeName = null,
                            InvoiceDate = default
                        }
                    )
                    .ToListAsync();

                ViewBag.InvoiceId = invoiceId;
                return View(paymentDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PaymentDetails: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل تفاصيل الدفع." });
            }
        }

        public async Task<IActionResult> JournalEntriess(int invoiceId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var journalEntries = await _context.JournalEntries
                    .Where(je => je.InvoiceId == invoiceId && je.UserId == userId)
                    .Select(je => new
                    {
                        je.EntryDate,
                        je.Description,
                        je.DebitAmount,
                        je.CreditAmount
                    })
                    .ToListAsync();

                ViewBag.InvoiceId = invoiceId;
                return View(journalEntries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JournalEntries: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل تقارير اليومية." });
            }
        }

        public async Task<IActionResult> JournalEntries(int invoiceId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var journalEntries = await _context.JournalEntries
                    .Where(je => je.InvoiceId == invoiceId && je.UserId == userId)
                    .Select(je => new InvoiceViewModel
                    {
                        InvoiceId = je.InvoiceId, // Fixed casing from invoiceId to InvoiceId
                        EntryDate = je.EntryDate.ToDateTime(TimeOnly.MinValue), // Convert DateOnly to DateTime?
                        Description = je.Description,
                        DebitAmount = je.DebitAmount,
                        CreditAmount = je.CreditAmount,
                        // Unused properties for this view
                        CustomerName = null,
                        EmployeeName = null,
                        InvoiceDate = default,
                        TotalAmount = default,
                        Amount = default,
                        DueDate = null,
                        PaidDate = null
                    })
                    .ToListAsync();

                ViewBag.InvoiceId = invoiceId;
                return View(journalEntries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JournalEntries: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل تقارير اليومية." });
            }
        }














        public async Task<IActionResult> InvoiceDetails(int invoiceId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                var invoiceDetails = await _context.Invoices
                    .Where(i => i.InvoiceId == invoiceId && i.UserId == userId)
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .Include(i => i.Vendor)
                    .GroupJoin(
                        _context.Invoicedetails,
                        i => i.InvoiceId,
                        id => id.InvoiceId,
                        (i, id) => new { Invoice = i, InvoiceDetails = id }
                    )
                    .SelectMany(
                        x => x.InvoiceDetails.DefaultIfEmpty(),
                        (i, id) => new InvoiceViewModel
                        {
                            InvoiceId = i.Invoice.InvoiceId,
                            CustomerName = i.Invoice.Customer != null ? i.Invoice.Customer.CustomerName : $"عميل بدون اسم (ID: {i.Invoice.CustomerId})",
                            EmployeeName = i.Invoice.Employee != null ? i.Invoice.Employee.FullName : "غير محدد",
                            VendorName = i.Invoice.Vendor != null ? i.Invoice.Vendor.VendorName : "غير محدد",
                            InvoiceDate = i.Invoice.InvoiceDate,
                            PaymentMethod = i.Invoice.PaymentMethod,
                            Subtotal = i.Invoice.Subtotal,
                            InvoiceType = i.Invoice.InvoiceType,
                            DetailID = id != null ? id.DetailId : null,
                            ProductID = id != null ? id.ProductId : null,
                            Quantity = id != null ? id.Quantity : null,
                            UnitPrice = id != null ? id.UnitPrice : 0,
                            LineTotal = id != null ? id.LineTotal : 0,
                            VatAmount = id != null ? id.VatAmount : 0,
                            InvoiceTyping = id != null ? id.InvoiceTyping : null,
                            // Unused properties
                            TotalAmount = default,
                            Description = null,
                            Amount = default,
                            DueDate = null,
                            PaidDate = null,
                            EntryDate = null,
                            DebitAmount = default,
                            CreditAmount = default
                        }
                    )
                    .ToListAsync();

                if (!invoiceDetails.Any())
                {
                    return View("Error", new { Message = "لم يتم العثور على الفاتورة أو تفاصيلها." });
                }

                ViewBag.InvoiceId = invoiceId;
                return View(invoiceDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InvoiceDetails: {ex.Message}\n{ex.StackTrace}");
                return View("Error", new { Message = "حدث خطأ أثناء تحميل تفاصيل الفاتورة." });
            }
        }























        public async Task<IActionResult> IndexDetaildPurshasing()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            Console.WriteLine($"[IndexDetailPurchasing] Session UserId: {userId}");

            if (!userId.HasValue)
            {
                Console.WriteLine("[IndexDetailPurchasing] Error: UserId is null, redirecting to login.");
                return RedirectToAction("Login", "Usered");
            }

            try
            {
                int userIdValue = userId.Value;

                // Log total purchase invoices for debugging
                var totalInvoices = await _context.Invoices
                    .Where(i => i.InvoiceType == "شراء")
                    .CountAsync();
                Console.WriteLine($"[IndexDetailPurchasing] Total purchase invoices in database: {totalInvoices}");

                // Log invoices for this user
                var userInvoices = await _context.Invoices
                    .Where(i => i.UserId == userIdValue && i.InvoiceType == "شراء")
                    .Select(i => new { i.InvoiceId, i.UserId, i.VendorId, i.EmployeeId, i.InvoiceDate })
                    .ToListAsync();
                Console.WriteLine($"[IndexDetailPurchasing] Found {userInvoices.Count} purchase invoices for UserId: {userIdValue}");
                foreach (var inv in userInvoices)
                {
                    Console.WriteLine($"[IndexDetailPurchasing] Raw Invoice: InvoiceId={inv.InvoiceId}, UserId={inv.UserId}, VendorId={inv.VendorId}, EmployeeId={inv.EmployeeId}, InvoiceDate={inv.InvoiceDate}");
                }

                // Check for null or invalid EmployeeId values
                var nullEmployeeIds = userInvoices
                    .Where(i => i.EmployeeId == null)
                    .Select(i => i.InvoiceId)
                    .ToList();
                var invalidEmployeeIds = userInvoices
                    .Where(i => i.EmployeeId != null && !_context.Employees.Any(e => e.EmployeeId == i.EmployeeId))
                    .Select(i => i.EmployeeId)
                    .Distinct()
                    .ToList();
                if (nullEmployeeIds.Any())
                {
                    Console.WriteLine($"[IndexDetailPurchasing] Warning: Found invoices with null EmployeeIds: {string.Join(", ", nullEmployeeIds)}");
                }
                if (invalidEmployeeIds.Any())
                {
                    Console.WriteLine($"[IndexDetailPurchasing] Warning: Found invoices with invalid EmployeeIds: {string.Join(", ", invalidEmployeeIds)}");
                }

                var invoices = await _context.Invoices
                    .Where(i => i.UserId == userIdValue && i.InvoiceType == "شراء")
                    .GroupJoin(
                        _context.Vendors,
                        i => i.VendorId,
                        v => v.VendorId,
                        (i, v) => new { Invoice = i, Vendors = v }
                    )
                    .SelectMany(
                        iv => iv.Vendors.DefaultIfEmpty(),
                        (iv, v) => new { iv.Invoice, VendorName = v != null ? v.VendorName ?? $"Vendor without name (ID: {iv.Invoice.VendorId})" : (iv.Invoice.VendorId != null ? $"No vendor (ID: {iv.Invoice.VendorId})" : "No vendor") }
                    )
                    .GroupJoin(
                        _context.Employees,
                        iv => iv.Invoice.EmployeeId,
                        e => e.EmployeeId,
                        (iv, e) => new { iv.Invoice, iv.VendorName, Employees = e }
                    )
                    .SelectMany(
                        ive => ive.Employees.DefaultIfEmpty(),
                        (ive, e) => new InvoiceViewModel
                        {
                            InvoiceId = ive.Invoice.InvoiceId,
                            VendorName = ive.VendorName,
                            CustomerName = null, // Not used for purchasing invoices
                            EmployeeName = e != null ? e.FullName ?? $"Employee without name (ID: {ive.Invoice.EmployeeId})" : (ive.Invoice.EmployeeId != null ? $"No employee (ID: {ive.Invoice.EmployeeId})" : "No employee"),
                            InvoiceDate = ive.Invoice.InvoiceDate,
                            UserId = ive.Invoice.UserId??0
                        }
                    )
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                Console.WriteLine($"[IndexDetailPurchasing] Retrieved {invoices.Count} purchase invoices for UserId: {userIdValue}");
                foreach (var inv in invoices)
                {
                    Console.WriteLine($"[IndexDetailPurchasing] Invoice: InvoiceId={inv.InvoiceId}, UserId={inv.UserId}, VendorName={inv.VendorName}, EmployeeName={inv.EmployeeName}, InvoiceDate={inv.InvoiceDate}");
                }

                // Log distinct UserIds for debugging
                var distinctUserIds = invoices.Select(i => i.UserId).Distinct().ToList();
                Console.WriteLine($"[IndexDetailPurchasing] Distinct UserIds in result: {string.Join(", ", distinctUserIds)}");

                return View(invoices);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error retrieving purchase invoices: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }
                Console.WriteLine($"[IndexDetailPurchasing] {errorMessage}\nStackTrace: {ex.StackTrace}");
                return View("Error", new { Message = "An error occurred while loading the purchase invoices list." });
            }
        }
    }
}