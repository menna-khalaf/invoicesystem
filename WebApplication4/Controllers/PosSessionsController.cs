using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class PosSessionsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public PosSessionsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: PosSessions
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.PosSessions.Include(p => p.Branch).Include(p => p.Employee).Include(p => p.PosNavigation).Include(p => p.Storehouse);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: PosSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PosSessions == null)
            {
                return NotFound();
            }

            var posSession = await _context.PosSessions
                .Include(p => p.Branch)
                .Include(p => p.Employee)
                .Include(p => p.PosNavigation)
                .Include(p => p.Storehouse)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (posSession == null)
            {
                return NotFound();
            }

            return View(posSession);
        }
        [HttpGet]
        public IActionResult Create(int? posId, int? sessionId)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Try to get session ID from ASP.NET session if not provided
            if (!sessionId.HasValue)
            {
                sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");
            }

            // Retrieve the CashAccountId from session
            var cashAccountId = HttpContext.Session.GetInt32("CashAccountId");

            // Fetch the CashAccount balance
            var cashAccount = _context.Cashandbankaccounts
                                    .FirstOrDefault(ca => ca.AccountId == cashAccountId);

            // Prepare ViewModel with user-specific products
            var viewModel = new CreatePosSessionViewModel
            {
                PosSession = new PosSession(),
                Cart = new CartViewModel
                {
                    CartId = Guid.NewGuid().ToString(),
                    Items = new List<CartItemViewModel>()
                },
                Products = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Storehouse)
                    .Where(p => p.UserId == userId)  // Filter by current user's ID
                    .ToList(),
                InvoiceCount = 0,
                CashAccountId = cashAccountId,
                CashAccountBalance = cashAccount?.Balance
            };

            if (sessionId.HasValue)
            {
                var session = _context.PosSessions
                    .Include(s => s.Branch)
                    .Include(s => s.PosNavigation)
                    .Include(s => s.Employee)
                    .Include(s => s.Storehouse)
                    .Include(s => s.Invoices)
                    .FirstOrDefault(s => s.SessionId == sessionId.Value);

                if (session != null)
                {
                    viewModel.PosSession = session;
                    viewModel.StartingCash = session.StartingCash;
                    viewModel.InvoiceCount = session.Invoices?.Count ?? 0;

                    viewModel.BranchName = session.Branch?.BranchName;
                    viewModel.PosName = session.PosNavigation?.Posname;
                    viewModel.EmployeeName = session.Employee?.FullName;
                    viewModel.StorehouseName = session.Storehouse?.StorehouseName;
                }
            }

            ViewBag.SessionId = sessionId;

            if (viewModel.PosSession.SessionId == 0)
            {
                ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
                ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName");
                ViewData["Posid"] = new SelectList(_context.Pos, "Posid", "Posname");
                ViewData["StorehouseId"] = new SelectList(
                    _context.Storehouses.Where(s => s.UserId == userId),
                    "StorehouseId",
                    "StorehouseName");
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionInvoiceCount(int sessionId)
        {
            try
            {
                // Verify session exists first
                var sessionExists = await _context.PosSessions
                    .AnyAsync(s => s.SessionId == sessionId);

                if (!sessionExists)
                {
                    return Json(new { success = false, error = "Session not found" });
                }

                var invoiceCount = await _context.Invoices
                    .Where(i => i.SessionId == sessionId)
                    .CountAsync();

                return Json(new { success = true, count = invoiceCount });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    detailedError = ex.InnerException?.Message
                });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePosSessionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Retrieve session ID from ASP.NET session if not set in viewModel
                if (viewModel.PosSession.SessionId == 0)
                {
                    var sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");
                    if (sessionId.HasValue)
                    {
                        viewModel.PosSession.SessionId = sessionId.Value;
                    }
                }

                if (viewModel.PosSession.SessionId == 0)
                {
                    // Creating a new session
                    viewModel.PosSession.StartTime = DateTime.Now;
                    viewModel.PosSession.Status = "Active";
                    _context.Add(viewModel.PosSession);

                    await _context.SaveChangesAsync();

                    // Store the new session ID in ASP.NET session
                    HttpContext.Session.SetInt32("CurrentPosSessionId", viewModel.PosSession.SessionId);
                }
                else
                {
                    // Updating an existing session
                    var existingSession = await _context.PosSessions
                        .FirstOrDefaultAsync(s => s.SessionId == viewModel.PosSession.SessionId);

                    if (existingSession != null)
                    {
                        existingSession.StartingCash = viewModel.PosSession.StartingCash;
                        _context.Update(existingSession);
                    }
                }

                await _context.SaveChangesAsync();

                // Handle cart items
                if (viewModel.PosSession.SessionId != 0)
                {
                    var existingCartItems = _context.PosCarts
                        .Where(c => c.SessionId == viewModel.PosSession.SessionId);
                    _context.PosCarts.RemoveRange(existingCartItems);
                }

                // Add new cart items
                foreach (var item in viewModel.Cart.Items)
                {
                    var cartItem = new PosCart
                    {
                        CartId = viewModel.Cart.CartId,
                        SessionId = viewModel.PosSession.SessionId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        AddedAt = DateTime.Now,
                        Status = "Active"
                    };
                    _context.PosCarts.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // Pass the session ID to the next page
                return RedirectToAction("NextPage", new { sessionId = viewModel.PosSession.SessionId });
            }

            // If we got this far, something failed, redisplay form
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", viewModel.PosSession.BranchId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", viewModel.PosSession.EmployeeId);
            ViewData["Posid"] = new SelectList(_context.Pos, "Posid", "Posname", viewModel.PosSession.Posid);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", viewModel.PosSession.StorehouseId);

            return View(viewModel);
        }

        // POST: Remove from Cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            var cartItem = await _context.PosCarts
                .FirstOrDefaultAsync(c => c.CartId == request.CartId && c.ProductId == request.ProductId);

            if (cartItem != null)
            {
                _context.PosCarts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        public class AddToCartRequest
        {
            public string CartId { get; set; }
            public int SessionId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class RemoveFromCartRequest
        {
            public string CartId { get; set; }
            public int ProductId { get; set; }
        }

        // GET: PosSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PosSessions == null)
            {
                return NotFound();
            }

            var posSession = await _context.PosSessions.FindAsync(id);
            if (posSession == null)
            {
                return NotFound();
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", posSession.BranchId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", posSession.EmployeeId);
            ViewData["Posid"] = new SelectList(_context.Pos, "Posid", "Posname", posSession.Posid);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", posSession.StorehouseId);
            return View(posSession);
        }

        // POST: PosSessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SessionId,Posid,EmployeeId,StorehouseId,BranchId,StartTime,EndTime,StartingCash,EndingCash,Status,Notes")] PosSession posSession)
        {
            if (id != posSession.SessionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosSessionExists(posSession.SessionId))
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
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", posSession.BranchId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", posSession.EmployeeId);
            ViewData["Posid"] = new SelectList(_context.Pos, "Posid", "Posname", posSession.Posid);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", posSession.StorehouseId);
            return View(posSession);
        }

        // GET: PosSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PosSessions == null)
            {
                return NotFound();
            }

            var posSession = await _context.PosSessions
                .Include(p => p.Branch)
                .Include(p => p.Employee)
                .Include(p => p.PosNavigation)
                .Include(p => p.Storehouse)
                .FirstOrDefaultAsync(m => m.SessionId == id);
            if (posSession == null)
            {
                return NotFound();
            }

            return View(posSession);
        }

        // POST: PosSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PosSessions == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.PosSessions'  is null.");
            }
            var posSession = await _context.PosSessions.FindAsync(id);
            if (posSession != null)
            {
                _context.PosSessions.Remove(posSession);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PosSessionExists(int id)
        {
          return (_context.PosSessions?.Any(e => e.SessionId == id)).GetValueOrDefault();
        }












        [HttpGet]
        public IActionResult Payment(int? sessionId)
        {


            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }
            try
            {
                Console.WriteLine($"Entering Payment GET action with sessionId: {sessionId}");

                // If sessionId wasn't provided in URL, try to get it from ASP.NET session
                if (!sessionId.HasValue)
                {
                    sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");
                    Console.WriteLine($"Retrieved sessionId from ASP.NET session: {sessionId}");
                }

                var BankAccountId = HttpContext.Session.GetInt32("BankAccountId");
                Console.WriteLine($"Retrieved CashAccountId from session: {BankAccountId}");



                // Retrieve CashAccountId from session
                var cashAccountId = HttpContext.Session.GetInt32("CashAccountId");
                Console.WriteLine($"Retrieved CashAccountId from session: {cashAccountId}");

                var cartJson = HttpContext.Session.GetString("Cart");
                if (string.IsNullOrEmpty(cartJson))
                {
                    Console.WriteLine("Cart is empty - redirecting to Create");
                    return RedirectToAction("Create");
                }

                var model = new PaymentViewModel
                {
                    SessionId = sessionId,
                    CashAccountId = cashAccountId, // Add this to your view model
                    BankAccountId=BankAccountId,
                    PaymentMethods = _context.PosPaymentMethods
                        .Where(p => p.IsActive == true)
                        .OrderBy(p => p.DisplayOrder)
                        .ToList(),
                    CartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson),
                    Subtotal = decimal.Parse(HttpContext.Session.GetString("Subtotal") ?? "0"),
                    Tax = decimal.Parse(HttpContext.Session.GetString("Tax") ?? "0"),
                    Total = decimal.Parse(HttpContext.Session.GetString("Total") ?? "0")
                };

                Console.WriteLine($"Loaded payment view with {model.CartItems?.Count} items for session {sessionId}");
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Payment GET: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment([FromForm] PaymentViewModel model, bool isInvoicePayment = true)
        {
            try
            {
                // 1. Retrieve UserId from session
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
                }
                Console.WriteLine($"Retrieved UserId from session: {userId}");

                // 2. Log the incoming model for debugging
                Console.WriteLine($"Received PaymentViewModel: CustomerId={model.CustomerId}, SessionId={model.SessionId}, Total={model.Total}");
                Console.WriteLine($"PaymentTypes count: {model.PaymentTypes?.Count ?? 0}");
                if (model.PaymentTypes != null)
                {
                    for (int i = 0; i < model.PaymentTypes.Count; i++)
                    {
                        var pt = model.PaymentTypes[i];
                        Console.WriteLine($"PaymentTypes[{i}]: Type={pt.Type}, Amount={pt.Amount}, DueDate={pt.DueDate?.ToString() ?? "null"}");
                    }
                }

                // 3. Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    Console.WriteLine("ModelState Errors: " + string.Join(", ", errors));
                    return Json(new { success = false, message = "بيانات غير صالحة", errors = errors });
                }

                // 4. Validate session ID
                if (!model.SessionId.HasValue)
                {
                    return Json(new { success = false, message = "معرف الجلسة غير موجود" });
                }

                // 5. Verify session exists
                var sessionExists = await _context.PosSessions
                    .AnyAsync(s => s.SessionId == model.SessionId.Value);
                if (!sessionExists)
                {
                    return Json(new { success = false, message = "الجلسة غير صالحة أو منتهية" });
                }

                // 6. Validate customer
                if (!model.CustomerId.HasValue)
                {
                    return Json(new { success = false, message = "الرجاء اختيار عميل" });
                }

                // 7. Validate PaymentTypes
                if (model.PaymentTypes == null || !model.PaymentTypes.Any())
                {
                    return Json(new { success = false, message = "يجب اختيار طريقة دفع واحدة على الأقل" });
                }

                // 8. Calculate totals based on whether this is for an invoice or direct payment
                decimal totalAmount;
                decimal subtotal = 0;
                decimal vatAmount = 0;
                List<CartItemViewModel> cartItems = null; // Store cartItems for reuse

                if (isInvoicePayment)
                {
                    // Get cart from session for invoice payment
                    var cartJson = HttpContext.Session.GetString("Cart");
                    if (string.IsNullOrEmpty(cartJson))
                    {
                        return Json(new { success = false, message = "سلة التسوق فارغة" });
                    }

                    // Deserialize cart
                    cartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
                    if (cartItems == null || !cartItems.Any())
                    {
                        return Json(new { success = false, message = "لا توجد عناصر في السلة" });
                    }

                    subtotal = cartItems.Sum(i => i.Price * i.Quantity);
                    vatAmount = cartItems.Sum(i => i.VatAmount);
                    totalAmount = subtotal + vatAmount;
                }
                else
                {
                    // For direct payment, use the provided total
                    totalAmount = model.Total;
                }

                // 9. Validate PaymentTypes total
                var paymentTypesTotal = model.PaymentTypes.Sum(pt => pt.Amount);
                if (Math.Abs(paymentTypesTotal - totalAmount) > 0.01m)
                {
                    return Json(new { success = false, message = "مجموع مبالغ طرق الدفع لا يساوي المبلغ الإجمالي" });
                }

                // 10. Start transaction
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 11. Create invoice first if this is an invoice payment
                Invoice? invoice = null;
                if (isInvoicePayment)
                {
                    // Create invoice
                    var paymentMethodString = string.Join(", ", model.PaymentTypes.Select(p => p.Type));
                    if (paymentMethodString.Length > 100)
                    {
                        paymentMethodString = paymentMethodString.Substring(0, 97) + "...";
                    }

                    invoice = new Invoice
                    {
                        CustomerId = model.CustomerId.Value,
                        Status = "مدفوعة",
                        Subtotal = subtotal,
                        Vatrate = 15.00m,
                        Vatamount = vatAmount,
                        InvoiceType = "بيع",
                        ItemsCount = cartItems.Count,
                        EmployeeId = userId.Value,
                        UserId = userId.Value,
                        SessionId = model.SessionId.Value,
                        TotalAmount = totalAmount,
                        InvoiceDate = DateTime.Now
                    };
                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();
                }

                // 12. Create Payment record
                var payment = new Payment
                {
                    CustomerId = model.CustomerId.Value,
                    VendorId = null,
                    UserId = userId.Value,
                    Paydate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = totalAmount,
                    PaymentStatus = "Paid",
                    Notes = null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    InvoiceId = isInvoicePayment ? invoice?.InvoiceId : null
                };
                Console.WriteLine($"Creating Payment: CustomerId={payment.CustomerId}, TotalAmount={payment.TotalAmount}, InvoiceId={payment.InvoiceId}, UserId={payment.UserId}");
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Payment saved with Payid: {payment.Payid}");

                // 13. Create PaymentDetails records
                var paymentDetails = model.PaymentTypes.Select(paymentType => new Paymentdetail
                {
                    Payid = payment.Payid,
                    UserId = userId.Value,
                    Description = paymentType.Type,
                    Amount = paymentType.Amount,
                    DueDate = paymentType.Type == "آجل"
                        ? DateOnly.FromDateTime(paymentType.DueDate ?? DateTime.Now.AddDays(30))
                        : null,
                    IsPaid = paymentType.Type != "آجل",
                    PaidDate = paymentType.Type != "آجل" ? DateOnly.FromDateTime(DateTime.Now) : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                if (!paymentDetails.Any())
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "لا توجد تفاصيل دفع للحفظ" });
                }
                Console.WriteLine($"Saving {paymentDetails.Count} Paymentdetails rows");
                await _context.Paymentdetails.AddRangeAsync(paymentDetails);
                await _context.SaveChangesAsync();

                // 14. Handle invoice details and inventory if this is an invoice payment
                Dictionary<int, decimal> productCosts = new Dictionary<int, decimal>();
                if (isInvoicePayment && invoice != null)
                {
                    // Add invoice details
                    var details = cartItems.Select(item => new Invoicedetail
                    {
                        InvoiceId = invoice.InvoiceId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        LineTotal = item.Price * item.Quantity,
                        VatAmount = item.VatAmount,
                        InvoiceTyping = "بيع"
                    }).ToList();
                    await _context.Invoicedetails.AddRangeAsync(details);

                    // Fetch product costs and validate
                    foreach (var item in cartItems)
                    {
                        var product = await _context.Products
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId && p.UserId == userId);
                        if (product == null)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = $"المنتج برقم {item.ProductId} غير موجود أو لا يخصك." });
                        }
                        if (product.PurchasePrice == null)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = $"سعر الشراء للمنتج {product.ProductName} غير محدد." });
                        }
                        if (item.Quantity > product.Balance)
                        {
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = $"الكمية المطلوبة ({item.Quantity}) تتجاوز الرصيد المتاح ({product.Balance}) للمنتج {product.ProductName}." });
                        }
                        productCosts[item.ProductId] = product.PurchasePrice; // Use .Value since null is checked; change to product.PurchasePrice if decimal

                        // Update product balance
                        product.Balance -= item.Quantity;
                        _context.Products.Update(product);
                    }

                    // Fetch StorehouseId from PosSessions or use default
                    var storehouseId = await _context.PosSessions
                        .Where(s => s.SessionId == model.SessionId.Value)
                        .Select(s => s.StorehouseId)
                        .FirstOrDefaultAsync();
                    if (storehouseId == null || storehouseId == 0)
                    {
                        Console.WriteLine($"Warning: StorehouseId is null or 0 for SessionId={model.SessionId.Value}. Using default StorehouseId=1.");
                        storehouseId = 1; // Default to 1; adjust based on your valid StorehouseId
                    }
                    Console.WriteLine($"Using StorehouseId={storehouseId} for Inventory records");

                    // Update inventory
                    var inventoryRecords = cartItems.Select(item => new Inventory
                    {
                        ProductId = item.ProductId,
                        OutgoingQuantity = item.Quantity,
                        IncomingQuantity = 0,
                        LastUpdated = DateTime.Now,
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.InvoiceId,
                        UserId = userId.Value,
                        EmployeeId = userId.Value,
                        SessionId = model.SessionId.Value,
                        StorehouseId = storehouseId // Use .Value since null is handled above
                    }).ToList();
                    await _context.Inventories.AddRangeAsync(inventoryRecords);
                    await _context.SaveChangesAsync(); // Explicit save to catch errors here

                    // Clear the cart session
                    HttpContext.Session.Remove("Cart");
                    HttpContext.Session.Remove("Subtotal");
                    HttpContext.Session.Remove("Tax");
                    HttpContext.Session.Remove("Total");

                    // Store InvoiceId in session for confirmation
                    HttpContext.Session.SetInt32("CurrentInvoiceId", invoice.InvoiceId);
                }

                // 15. Update cash and bank account balances
                int? cashAccountId = HttpContext.Session.GetInt32("CashAccountId");
                int? bankAccountId = HttpContext.Session.GetInt32("BankAccountId");

                Console.WriteLine($"Session - CashAccountId: {cashAccountId}, BankAccountId: {bankAccountId}");

                if (!cashAccountId.HasValue)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "معرف حساب النقدية غير متوفر في الجلسة" });
                }

                if (!bankAccountId.HasValue)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "معرف حساب البنك غير متوفر في الجلسة" });
                }

                var cashAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == cashAccountId.Value);
                if (cashAccount == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"حساب النقدية بمعرف {cashAccountId.Value} غير موجود في جدول Cashandbankaccounts" });
                }

                var bankAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == bankAccountId.Value);
                if (bankAccount == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"حساب البنك بمعرف {bankAccountId.Value} غير موجود في جدول Cashandbankaccounts" });
                }

                Console.WriteLine($"Cash Account - ID: {cashAccount.AccountId}, Balance: {cashAccount.Balance}");
                Console.WriteLine($"Bank Account - ID: {bankAccount.AccountId}, Balance: {bankAccount.Balance}");

                foreach (var paymentType in model.PaymentTypes)
                {
                    var type = paymentType.Type.Trim().ToLower();
                    if (type == "كاش")
                    {
                        cashAccount.Balance += paymentType.Amount;
                        _context.Cashandbankaccounts.Update(cashAccount);
                        Console.WriteLine($"Cash account {cashAccount.AccountId} balance updated to {cashAccount.Balance} (increased by {paymentType.Amount})");
                    }
                    else if (type == "تحويل بنكي")
                    {
                        bankAccount.Balance += paymentType.Amount;
                        _context.Cashandbankaccounts.Update(bankAccount);
                        Console.WriteLine($"Bank account {bankAccount.AccountId} balance updated to {bankAccount.Balance} (increased by {paymentType.Amount})");
                    }
                    else
                    {
                        Console.WriteLine($"Payment type skipped for balance update: {paymentType.Type}");
                    }
                }

                // 16. Fetch ChildAccountIds from account_mappings for journal entries
                var salesAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "إيرادات المبيعات")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (salesAccountId == 0 && isInvoicePayment)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب إيرادات المبيعات غير معرف في تعيينات الحسابات." });
                }

                var vatAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "ضريبة القيمة المضافة المستحقة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (vatAccountId == 0 && isInvoicePayment && vatAmount > 0)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب ضريبة القيمة المضافة غير معرف في تعيينات الحسابات." });
                }
                if (vatAccountId != 6 && isInvoicePayment && vatAmount > 0)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"حساب ضريبة القيمة المضافة يجب أن يكون child_account_id = 6، ولكن تم العثور على {vatAccountId}." });
                }

                var accountsReceivableAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "الحسابات المستحقة القبض")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (accountsReceivableAccountId == 0 && model.PaymentTypes.Any(p => p.Type == "آجل"))
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب الحسابات المستحقة القبض غير معرف في تعيينات الحسابات." });
                }

                var cashAccountMappingId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "النقدية بالصندوق")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cashAccountMappingId == 0 && model.PaymentTypes.Any(p => p.Type == "كاش"))
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب النقدية بالصندوق غير معرف في تعيينات الحسابات." });
                }

                var bankAccountMappingId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "النقدية بالبنك")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (bankAccountMappingId == 0 && model.PaymentTypes.Any(p => p.Type == "تحويل بنكي"))
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب النقدية بالبنك غير معرف في تعيينات الحسابات." });
                }

                var inventoryAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "المخزون")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (inventoryAccountId == 0 && isInvoicePayment)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب المخزون غير معرف في تعيينات الحسابات." });
                }

                var cogsAccountId = await _context.AccountMappings
                    .Where(m => m.TransactionType == "تكلفة البضاعة المباعة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cogsAccountId == 0 && isInvoicePayment)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "حساب تكلفة البضاعة المباعة غير معرف في تعيينات الحسابات." });
                }

                // 17. Initialize journal entries list
                var journalEntries = new List<JournalEntry>();

                // 18. Add journal entries for invoice payment
                if (isInvoicePayment && invoice != null)
                {
                    // Credit Sales Revenue
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"فاتورة مبيعات رقم {invoice.InvoiceId}",
                        DebitAmount = 0,
                        CreditAmount = subtotal,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "بيع",
                        ChildAccountId = salesAccountId
                    });

                    // Credit VAT Payable (if applicable)
                    if (vatAmount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مبيعات - فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = 0,
                            CreditAmount = vatAmount,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = vatAccountId
                        });
                    }

                    // Add journal entries for inventory and COGS
                    decimal totalCogs = 0;
                    foreach (var item in cartItems)
                    {
                        decimal cogsAmount = item.Quantity * productCosts[item.ProductId];
                        totalCogs += cogsAmount;
                        Console.WriteLine($"COGS for ProductId={item.ProductId}: Quantity={item.Quantity}, PurchasePrice={productCosts[item.ProductId]}, COGSAmount={cogsAmount}");

                        // Debit COGS (Expense, increases by debit)
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"تكلفة البضاعة المباعة - فاتورة رقم {invoice.InvoiceId}, منتج {item.ProductId}",
                            DebitAmount = cogsAmount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = cogsAccountId
                        });

                        // Credit Inventory (Asset, decreases by credit)
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"تخفيض المخزون - فاتورة رقم {invoice.InvoiceId}, منتج {item.ProductId}",
                            DebitAmount = 0,
                            CreditAmount = cogsAmount,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = inventoryAccountId
                        });
                    }
                    Console.WriteLine($"Total COGS for InvoiceId={invoice.InvoiceId}: {totalCogs}");
                }

                // 19. Add journal entries for payments
                foreach (var paymentType in model.PaymentTypes)
                {
                    var type = paymentType.Type.Trim().ToLower();
                    if (type == "كاش" && paymentType.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"دفع كاش - {(isInvoicePayment && invoice != null ? $"فاتورة رقم {invoice.InvoiceId}" : $"دفع مباشر Payid {payment.Payid}")}",
                            DebitAmount = paymentType.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = isInvoicePayment ? invoice?.InvoiceId : null,
                            InvoiceType = isInvoicePayment ? "بيع" : null,
                            ChildAccountId = cashAccountMappingId
                        });
                    }
                    else if (type == "تحويل بنكي" && paymentType.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"دفع بنكي - {(isInvoicePayment && invoice != null ? $"فاتورة رقم {invoice.InvoiceId}" : $"دفع مباشر Payid {payment.Payid}")}",
                            DebitAmount = paymentType.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = isInvoicePayment ? invoice?.InvoiceId : null,
                            InvoiceType = isInvoicePayment ? "بيع" : null,
                            ChildAccountId = bankAccountMappingId
                        });
                    }
                    else if (type == "آجل" && paymentType.Amount > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد آجل للعميل - {(isInvoicePayment && invoice != null ? $"فاتورة رقم {invoice.InvoiceId}" : $"دفع مباشر Payid {payment.Payid}")}",
                            DebitAmount = paymentType.Amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = isInvoicePayment ? invoice?.InvoiceId : null,
                            InvoiceType = isInvoicePayment ? "بيع" : null,
                            ChildAccountId = accountsReceivableAccountId
                        });
                    }
                }

                // 20. Log and save journal entries
                Console.WriteLine($"Saving {journalEntries.Count} JournalEntries");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}");
                }
                _context.JournalEntries.AddRange(journalEntries);
                await _context.SaveChangesAsync();

                // 21. Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Verify balances after save
                var updatedCashAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == cashAccountId.Value);
                var updatedBankAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == bankAccountId.Value);
                Console.WriteLine($"After save - Cash account {updatedCashAccount.AccountId} balance: {updatedCashAccount.Balance}");
                Console.WriteLine($"After save - Bank account {updatedBankAccount.AccountId} balance: {updatedBankAccount.Balance}");

                // 22. Return success
                return Json(new
                {
                    success = true,
                    invoiceId = invoice?.InvoiceId,
                    paymentId = payment.Payid,
                    sessionId = model.SessionId.Value,
                    message = isInvoicePayment ? "تم حفظ الفاتورة وتحديث الرصيد وقيود اليومية بنجاح" : "تم حفظ الدفع وقيود اليومية بنجاح",
                    cashBalance = updatedCashAccount.Balance,
                    bankBalance = updatedBankAccount?.Balance
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = isInvoicePayment ? "حدث خطأ أثناء حفظ الفاتورة" : "حدث خطأ أثناء حفظ الدفع",
                    error = ex.Message,
                    detailedError = ex.InnerException?.Message ?? ex.Message
                });
            }
        }












        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string term = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                Console.WriteLine("User not logged in - returning unauthorized");
                return Unauthorized(new { error = "User not authenticated" });
            }

            try
            {
                var query = _context.Customers.AsQueryable();

                // Filter by UserId from session
                query = query.Where(c => c.UserId == userId);

                // Apply search term filter if provided
                if (!string.IsNullOrWhiteSpace(term))
                {
                    query = query.Where(c => c.CustomerName.Contains(term));
                }

                var customers = await query
                    .OrderBy(c => c.CustomerName)
                    .Select(c => new
                    {
                        customerId = c.CustomerId,
                        customerName = c.CustomerName
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {customers.Count} customers for UserId: {userId} with term: {term}");
                return Json(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SearchCustomers: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while searching customers" });
            }
        }
        [HttpPost]
        public IActionResult StoreInvoiceIdInSession(int invoiceId)
        {
            HttpContext.Session.SetInt32("CurrentInvoiceId", invoiceId);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetInvoiceIdFromSession()
        {
            var invoiceId = HttpContext.Session.GetInt32("CurrentInvoiceId");
            return Json(new
            {
                success = invoiceId.HasValue,
                invoiceId = invoiceId ?? 0
            });
        }
        [HttpGet]
        public IActionResult PaymentConfirmation(int invoiceId, int? sessionId)
        {
            try
            {
                // Get the invoice with all related data
                var invoice = _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .Include(i => i.Invoicedetails)
                        .ThenInclude(d => d.Product)
                    .FirstOrDefault(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Create view model
                var model = new PaymentConfirmationViewModel
                {
                    Invoice = invoice,
                    SessionId = sessionId,
                    CompanyInfo = new CompanyInfoViewModel
                    {
                        Name = "Your Company Name",
                        Address = "123 Business Street, City",
                        Phone = "+966 12 345 6789",
                        TaxNumber = "123456789",
                        LogoUrl = "/images/logo.png"
                    }
                };

                return View(model);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }
        }












        [HttpPost]
        public IActionResult SaveCartToSession([FromBody] CartSessionData data)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(data.cartItems));
            HttpContext.Session.SetString("Subtotal", data.subtotal.ToString());
            HttpContext.Session.SetString("Tax", data.tax.ToString());
            HttpContext.Session.SetString("Total", data.total.ToString());
            return Ok();
        }

        public class CartSessionData
        {
            public List<CartItemViewModel> cartItems { get; set; }
            public decimal subtotal { get; set; }
            public decimal tax { get; set; }
            public decimal total { get; set; }
        }



















        [HttpGet]
        public IActionResult ShiftReport(int sessionId)
        {
            // Get the session details
            var session = _context.PosSessions
                .Include(s => s.Branch)
                .Include(s => s.PosNavigation)
                .Include(s => s.Employee)
                .Include(s => s.Storehouse)
                .FirstOrDefault(s => s.SessionId == sessionId);

            if (session == null)
            {
                return NotFound();
            }

            // Get all invoices for this session
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Include(i => i.Pos)
                .Where(i => i.SessionId == sessionId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();

            // Get all invoice details for these invoices
            var invoiceDetails = _context.Invoicedetails
                .Where(d => d.InvoiceId.HasValue && invoices.Select(i => i.InvoiceId).Contains(d.InvoiceId.Value))
                .ToList();

            // Get the invoice IDs for this session
            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            // Total Sales (بيع): LineTotal + VatAmount
            var totalSales = invoiceDetails
                .Where(d => d.InvoiceTyping == "بيع")
                .Sum(d => (decimal?)(d.LineTotal + (d.VatAmount ?? 0))) ?? 0;

            // Total Returns (مرتجع من البيع): LineTotal + VatAmount
            var totalReturns = invoiceDetails
                .Where(d => d.InvoiceTyping == "مرتجع من البيع")
                .Sum(d => (decimal?)(d.LineTotal + (d.VatAmount ?? 0))) ?? 0;

            // New: Get the total returns grouped by SessionID (equivalent to your SQL query)
            var totalReturnsBySession = invoiceDetails
                .Where(d => d.InvoiceTyping == "مرتجع من البيع")
                .GroupBy(d => invoices.First(i => i.InvoiceId == d.InvoiceId).SessionId)
                .Select(g => new {
                    SessionID = g.Key,
                    TotalSalesAmount = g.Sum(d => d.LineTotal + (d.VatAmount ?? 0))
                })
                .FirstOrDefault();

            // Final ViewModel
            var viewModel = new ShiftReportViewModel
            {
                Session = session,
                Invoices = invoices,
                InvoiceDetails = invoiceDetails,
                TotalSales = totalSales,
                TotalReturns = totalReturns,
                NetSales = totalSales - totalReturns,
                TotalReturnsBySession = totalReturnsBySession?.TotalSalesAmount ?? 0 // Add this to your view model
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ReturnSalesCashier(int originalInvoiceId)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get the original invoice with its details
            var originalInvoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Invoicedetails)
                .ThenInclude(id => id.Product)
                .FirstOrDefault(i => i.InvoiceId == originalInvoiceId);

            if (originalInvoice == null)
            {
                return NotFound();
            }

            // Get the original payment with payment details
            var originalPayment = _context.Payments
                .Include(p => p.Paymentdetails)
                .FirstOrDefault(p => p.InvoiceId == originalInvoiceId);

            // Get payment details for the invoice
            var paymentDetails = (from p in _context.Payments
                                  join pd in _context.Paymentdetails on p.Payid equals pd.Payid
                                  where p.InvoiceId == originalInvoiceId
                                  select new PaymentDetailViewModel
                                  {
                                      Description = pd.Description,
                                      Amount = pd.Amount,
                                      DueDate = pd.DueDate
                                  }).ToList();

            // Get returned quantities for each product in the invoice
            var returnedQuantities = _context.Invoicedetails
                .Where(id => id.InvoiceId == originalInvoiceId && id.InvoiceTyping == "مرتجع من البيع")
                .GroupBy(id => id.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalReturnedQuantity = g.Sum(x => x.Quantity)
                })
                .ToDictionary(x => x.ProductId, x => x.TotalReturnedQuantity);

            var viewModel = new ReturnInvoiceViewModel
            {
                OriginalInvoice = originalInvoice,
                InvoiceItems = originalInvoice.Invoicedetails
                    .Where(id => id.InvoiceTyping != "مرتجع من البيع")
                    .ToList(),
                OriginalPayment = originalPayment,
                OriginalPaymentDetails = originalPayment?.Paymentdetails.ToList() ?? new List<Paymentdetail>(),
                PaymentDetails = paymentDetails,
                ReturnItems = originalInvoice.Invoicedetails
                    .Where(id => id.InvoiceTyping != "مرتجع من البيع")
                    .Select(id => new ReturnItem
                    {
                        InvoiceDetailId = id.DetailId,
                        ProductId = id.Product.ProductId,
                        ProductName = id.Product?.ProductName,
                        Product = id.Product,
                        OriginalQuantity = id.Quantity,
                        RemainingQuantity = id.Quantity - (returnedQuantities.ContainsKey(id.ProductId ?? 0) ? returnedQuantities[id.ProductId ?? 0] : 0),
                        Price = id.UnitPrice,
                        ReturnedQuantity = 0
                    })
                    .Where(ri => ri.RemainingQuantity > 0)
                    .ToList()
            };

            // Initialize return quantities dictionary
            foreach (var item in viewModel.ReturnItems)
            {
                viewModel.ReturnQuantities[item.InvoiceDetailId] = 0;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(int originalInvoiceId, Dictionary<int, int> ReturnQuantities, string notes, List<PaymentDetailViewModel> PaymentDetails)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "معرف المستخدم غير موجود في الجلسة" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Retrieve original invoice details
                var originalDetails = await _context.Invoicedetails
                    .AsNoTracking()
                    .Include(d => d.Invoice)
                    .Include(d => d.Product)
                    .Where(d => d.InvoiceId == originalInvoiceId)
                    .ToListAsync();

                if (!originalDetails.Any())
                    return Json(new { success = false, message = "تفاصيل الفاتورة الأصلية غير موجودة" });

                var originalInvoice = await _context.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvoiceId == originalInvoiceId);

                if (originalInvoice == null)
                    return Json(new { success = false, message = "الفاتورة الأصلية غير موجودة" });

                var originalPayment = await _context.Payments
                    .Include(p => p.Paymentdetails)
                    .FirstOrDefaultAsync(p => p.InvoiceId == originalInvoiceId);

                decimal returnSubtotal = 0;
                decimal returnTax = 0;
                decimal returnTotal = 0;
                bool hasReturns = false;

                // Get account mappings for journal entries
                var accountMappings = await _context.AccountMappings.ToDictionaryAsync(
                    m => m.TransactionType,
                    m => m.ChildAccountId
                );

                // Validate we have all required account mappings
                var requiredAccounts = new[] {
            "إيرادات المبيعات",
            "ضريبة القيمة المضافة المستحقة",
            "المخزون",
            "تكلفة البضاعة المباعة"
        };

                foreach (var account in requiredAccounts)
                {
                    if (!accountMappings.ContainsKey(account) || accountMappings[account] == 0)
                    {
                        await transaction.RollbackAsync();
                        return Json(new
                        {
                            success = false,
                            message = $"حساب {account} غير معرف في تعيينات الحسابات"
                        });
                    }
                }

                var journalEntries = new List<JournalEntry>();
                var productCosts = new Dictionary<int, decimal>();

                foreach (var item in ReturnQuantities)
                {
                    if (item.Value <= 0) continue;

                    var originalDetail = originalDetails.FirstOrDefault(d => d.DetailId == item.Key);
                    if (originalDetail == null) continue;

                    if (item.Value > originalDetail.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return Json(new
                        {
                            success = false,
                            message = $"الكمية المرتجعة ({item.Value}) تتجاوز الكمية المباعة ({originalDetail.Quantity}) للمنتج {originalDetail.Product?.ProductName}"
                        });
                    }

                    hasReturns = true;
                    decimal lineSubtotal = item.Value * originalDetail.UnitPrice;
                    decimal vatRate = originalDetail.Product?.Vatrate ?? 5m;
                    decimal lineTax = lineSubtotal * (vatRate / 100);
                    decimal lineTotal = lineSubtotal + lineTax;

                    returnSubtotal += lineSubtotal;
                    returnTax += lineTax;
                    returnTotal += lineTotal;

                    // Calculate COGS (Cost of Goods Sold)
                    decimal cogsAmount = item.Value * (originalDetail.Product?.PurchasePrice ?? 0);
                    productCosts[originalDetail.ProductId ?? 0] = cogsAmount;

                    // Create return invoice detail
                    _context.Invoicedetails.Add(new Invoicedetail
                    {
                        InvoiceId = originalInvoiceId,
                        ProductId = originalDetail.ProductId,
                        Quantity = item.Value,
                        UnitPrice = originalDetail.UnitPrice,
                        LineTotal = lineSubtotal,
                        VatAmount = lineTax,
                        InvoiceTyping = "مرتجع من البيع"
                    });

                    // Update inventory
                    _context.Inventories.Add(new Inventory
                    {
                        ProductId = originalDetail.ProductId,
                        IncomingQuantity = item.Value,
                        OutgoingQuantity = 0,
                        ReferenceType = "Invoice",
                        ReferenceId = originalInvoiceId,
                        LastUpdated = DateTime.Now,
                        UserId = userId.Value
                    });

                    // Create journal entries for the return
                    // 1. Debit Sales Revenue (reverse original credit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"مرتجع مبيعات - فاتورة رقم {originalInvoiceId}, منتج {originalDetail.ProductId}",
                        DebitAmount = lineSubtotal,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = originalInvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = accountMappings["إيرادات المبيعات"]
                    });

                    // 2. Debit VAT Payable (reverse original credit)
                    if (lineTax > 0)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مرتجع مبيعات - فاتورة رقم {originalInvoiceId}, منتج {originalDetail.ProductId}",
                            DebitAmount = lineTax,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = originalInvoiceId,
                            InvoiceType = "مرتجع من البيع",
                            ChildAccountId = accountMappings["ضريبة القيمة المضافة المستحقة"]
                        });
                    }

                    // 3. Debit Inventory (reverse original credit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"زيادة المخزون - مرتجع فاتورة رقم {originalInvoiceId}, منتج {originalDetail.ProductId}",
                        DebitAmount = cogsAmount,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = originalInvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = accountMappings["المخزون"]
                    });

                    // 4. Credit COGS (reverse original debit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تخفيض تكلفة البضاعة المباعة - مرتجع فاتورة رقم {originalInvoiceId}, منتج {originalDetail.ProductId}",
                        DebitAmount = 0,
                        CreditAmount = cogsAmount,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = originalInvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = accountMappings["تكلفة البضاعة المباعة"]
                    });
                }

                if (!hasReturns)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "لم يتم تحديد كميات صالحة للإرجاع" });
                }

                var remainingReturnAmount = returnTotal;

                // Get cash/bank account IDs from session
                int? cashAccountId = HttpContext.Session.GetInt32("CashAccountId");
                int? bankAccountId = HttpContext.Session.GetInt32("BankAccountId");

                // Log session values for debugging
                Console.WriteLine($"Session - CashAccountId: {cashAccountId}, BankAccountId: {bankAccountId}");

                if (!bankAccountId.HasValue)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "معرف حساب البنك غير متوفر في الجلسة" });
                }

                var cashAccount = cashAccountId.HasValue
                    ? await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == cashAccountId.Value)
                    : null;

                var bankAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == bankAccountId.Value);

                if (bankAccount == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"حساب البنك بمعرف {bankAccountId.Value} غير موجود في جدول Cashandbankaccounts" });
                }

                // Log bank account details
                Console.WriteLine($"Bank Account - ID: {bankAccount.AccountId}, Balance: {bankAccount.Balance}");

                // 1️⃣ Try to update "آجل"
                var creditPayment = originalPayment?.Paymentdetails
                    .FirstOrDefault(pd => pd.Description.Trim() == "آجل");

                if (creditPayment != null && creditPayment.Amount > 0)
                {
                    var reductionAmount = Math.Min(creditPayment.Amount, remainingReturnAmount);
                    creditPayment.Amount -= reductionAmount;
                    creditPayment.UpdatedAt = DateTime.Now;
                    _context.Paymentdetails.Update(creditPayment);
                    remainingReturnAmount -= reductionAmount;

                    // Journal entry for accounts receivable adjustment
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تسوية رصيد آجل - مرتجع فاتورة رقم {originalInvoiceId}",
                        DebitAmount = 0,
                        CreditAmount = reductionAmount,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = originalInvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = accountMappings.ContainsKey("الحسابات المستحقة القبض")
                            ? accountMappings["الحسابات المستحقة القبض"]
                            : 0
                    });

                    Console.WriteLine($"Credit payment reduced by {reductionAmount}, new amount: {creditPayment.Amount}");
                }

                // 2️⃣ Then create negative Paymentdetails for كاش and تحويل بنكي + adjust balance
                if (remainingReturnAmount > 0)
                {
                    var descriptions = new[] { "كاش", "تحويل بنكي" };

                    foreach (var desc in descriptions)
                    {
                        if (remainingReturnAmount <= 0) break;

                        // Broaden description matching
                        var originalMethod = originalPayment?.Paymentdetails
                            .FirstOrDefault(pd => pd.Description.Trim().ToLower().Contains(desc.ToLower()));

                        if (originalMethod != null)
                        {
                            var negativeAmount = Math.Min(originalMethod.Amount, remainingReturnAmount);
                            var returnDescription = MapReturnDescription(originalMethod.Description);

                            var negativePaymentDetail = new Paymentdetail
                            {
                                Payid = originalPayment.Payid,
                                UserId = userId.Value,
                                Description = returnDescription,
                                Amount = -negativeAmount,
                                DueDate = DateOnly.FromDateTime(DateTime.Now),
                                IsPaid = true,
                                PaidDate = DateOnly.FromDateTime(DateTime.Now),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            _context.Paymentdetails.Add(negativePaymentDetail);
                            remainingReturnAmount -= negativeAmount;

                            // Adjust balance and create journal entry
                            if (desc.Trim().ToLower() == "كاش" && cashAccount != null)
                            {
                                cashAccount.Balance -= negativeAmount;
                                _context.Cashandbankaccounts.Update(cashAccount);

                                // Journal entry for cash refund
                                journalEntries.Add(new JournalEntry
                                {
                                    EntryDate = DateOnly.FromDateTime(DateTime.Now),
                                    Description = $"استرداد نقدي - مرتجع فاتورة رقم {originalInvoiceId}",
                                    DebitAmount = 0,
                                    CreditAmount = negativeAmount,
                                    UserId = userId.Value,
                                    CostCenterId = 0,
                                    CreatedAt = DateTime.Now,
                                    InvoiceId = originalInvoiceId,
                                    InvoiceType = "مرتجع من البيع",
                                    ChildAccountId = accountMappings.ContainsKey("النقدية بالصندوق")
                                        ? accountMappings["النقدية بالصندوق"]
                                        : 0
                                });

                                Console.WriteLine($"Cash account {cashAccount.AccountId} balance updated to {cashAccount.Balance} (decreased by {negativeAmount})");
                            }
                            else if (desc.Trim().ToLower() == "تحويل بنكي" && bankAccount != null)
                            {
                                bankAccount.Balance -= negativeAmount;
                                _context.Cashandbankaccounts.Update(bankAccount);

                                // Journal entry for bank refund
                                journalEntries.Add(new JournalEntry
                                {
                                    EntryDate = DateOnly.FromDateTime(DateTime.Now),
                                    Description = $"استرداد بنكي - مرتجع فاتورة رقم {originalInvoiceId}",
                                    DebitAmount = 0,
                                    CreditAmount = negativeAmount,
                                    UserId = userId.Value,
                                    CostCenterId = 0,
                                    CreatedAt = DateTime.Now,
                                    InvoiceId = originalInvoiceId,
                                    InvoiceType = "مرتجع من البيع",
                                    ChildAccountId = accountMappings.ContainsKey("النقدية بالبنك")
                                        ? accountMappings["النقدية بالبنك"]
                                        : 0
                                });

                                Console.WriteLine($"Bank account {bankAccount.AccountId} balance updated to {bankAccount.Balance} (decreased by {negativeAmount})");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No payment detail found for description: {desc}");
                        }
                    }
                }

                // 3️⃣ Insert a new negative payment record for logging
                var returnPayment = new Payment
                {
                    CustomerId = originalInvoice.CustomerId,
                    UserId = userId.Value,
                    Paydate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = -returnTotal,
                    PaymentStatus = "Paid",
                    Notes = notes ?? $"مرتجع فاتورة رقم {originalInvoiceId}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    InvoiceId = originalInvoiceId
                };

                _context.Payments.Add(returnPayment);

                // Add all journal entries to context
                _context.JournalEntries.AddRange(journalEntries);

                // Log changes before saving
                Console.WriteLine($"Saving changes - Bank account balance: {bankAccount.Balance}, Cash account balance: {(cashAccount != null ? cashAccount.Balance : "N/A")}");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Verify bank account balance after save
                var updatedBankAccount = await _context.Cashandbankaccounts.FirstOrDefaultAsync(a => a.AccountId == bankAccountId.Value);
                Console.WriteLine($"After save - Bank account {updatedBankAccount.AccountId} balance: {updatedBankAccount.Balance}");

                return Json(new
                {
                    success = true,
                    message = "تم تسجيل المرتجع بنجاح",
                    returnInvoiceId = originalInvoiceId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = $"حدث خطأ: {ex.Message}"
                });
            }
        }

        private string MapReturnDescription(string originalDescription)
        {
            if (string.IsNullOrEmpty(originalDescription))
                return "إرجاع";

            switch (originalDescription.ToLower().Trim())
            {
                case "نقدي":
                case "cash":
                case "كاش":
                    return "إرجاع نقدي";
                case "بطاقة ائتمان":
                case "credit card":
                    return "إرجاع ببطاقة ائتمان";
                case "تحويل بنكي":
                case "bank transfer":
                    return "إرجاع بتحويل بنكي";
                default:
                    return $"إرجاع: {originalDescription}";
            }
        }










        [HttpPost]
        public async Task<IActionResult> UpdateBankBalance([FromForm] decimal amount)
        {
            // Get cash account ID from session
            var BankAccountId = HttpContext.Session.GetInt32("BankAccountId");

            if (!BankAccountId.HasValue)
                return Json(new { success = false, message = "Bank account not selected" });

            if (amount <= 0)
                return Json(new { success = false, message = "Amount must be positive" });

            // Find and update the account
            var account = await _context.Cashandbankaccounts
                .FirstOrDefaultAsync(a => a.AccountId == BankAccountId.Value);

            if (account == null)
                return Json(new { success = false, message = "Cash account not found" });

            account.Balance += amount;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Balance updated. New balance: {account.Balance}"
            });
        }










        [HttpGet]
        public IActionResult SessionDetails(int sessionId)
        {
            var session = _context.PosSessions
                .Include(s => s.Branch)
                .Include(s => s.PosNavigation)
                .Include(s => s.Employee)
                .Include(s => s.Storehouse)
                .Include(s => s.Invoices)
                    .ThenInclude(i => i.Invoicedetails)
                .FirstOrDefault(s => s.SessionId == sessionId);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }


        [HttpPost]
        public IActionResult CloseSession(int sessionId)
        {
            try
            {
                var session = _context.PosSessions
                    .Include(s => s.Invoices)
                    .FirstOrDefault(s => s.SessionId == sessionId);

                if (session == null)
                {
                    return Json(new { success = false, error = "Session not found" });
                }

                // Update session details
                session.Status = "Closed";
                session.EndTime = DateTime.Now;

                // Calculate ending cash (adjust as needed)
                session.EndingCash = session.StartingCash + (session.Invoices?.Sum(i => i.TotalAmount) ?? 0);

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }








        [HttpGet]
        public IActionResult GetPagedProducts(int page = 1, int pageSize = 10)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Storehouse)
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PartialView("_ProductsPartial", products);
        }








        [HttpGet]
        public IActionResult SessionReturnsReport(int sessionId)
        {
            // Get session information
            var session = _context.PosSessions
                .Include(s => s.Branch)
                .Include(s => s.PosNavigation)
                .Include(s => s.Employee)
                .FirstOrDefault(s => s.SessionId == sessionId);

            if (session == null)
            {
                return NotFound();
            }

            // Get returned products details for this session
            var returnedProducts = _context.Invoices
                .Where(i => i.SessionId == sessionId)
                .Join(_context.Invoicedetails,
                    i => i.InvoiceId,
                    d => d.InvoiceId,
                    (i, d) => new { Invoice = i, Detail = d })
                .Where(x => x.Detail.InvoiceTyping == "مرتجع من البيع")
                .Join(_context.Products,
                    x => x.Detail.ProductId,
                    p => p.ProductId,
                    (x, p) => new ReturnedProductViewModel
                    {
                        ProductName = p.ProductName,
                        Quantity = x.Detail.Quantity,
                        UnitPrice = x.Detail.UnitPrice,
                        LineTotal = x.Detail.LineTotal,
                        VatAmount = x.Detail.VatAmount ?? 0,
                        TotalAmount = x.Detail.LineTotal + (x.Detail.VatAmount ?? 0),
                        InvoiceDate = x.Invoice.InvoiceDate,
                        InvoiceId = x.Invoice.InvoiceId
                    })
                .OrderBy(x => x.ProductName)
                .ToList();

            // Calculate totals
            var totals = new
            {
                TotalQuantity = returnedProducts.Sum(x => x.Quantity),
                TotalLineTotal = returnedProducts.Sum(x => x.LineTotal),
                TotalVatAmount = returnedProducts.Sum(x => x.VatAmount),
                TotalAmount = returnedProducts.Sum(x => x.TotalAmount)
            };

            var viewModel = new SessionReturnsReportViewModel
            {
                Session = session,
                ReturnedProducts = returnedProducts,
                Totals = totals
            };

            return View(viewModel);
        }












        [HttpGet]
        public IActionResult detailingreturn(int id)
        {
            // Get the invoice with its details
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Include(i => i.Pos)
                .Include(i => i.Invoicedetails)
                .ThenInclude(id => id.Product)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // Prepare the view model
            var viewModel = new InvoiceDetailViewModel
            {
                Invoice = invoice,
                SaleDetails = invoice.Invoicedetails
                    .Where(d => d.InvoiceTyping == "بيع")
                    .ToList(),
                ReturnDetails = invoice.Invoicedetails
                    .Where(d => d.InvoiceTyping == "مرتجع من البيع")
                    .ToList()
            };

            return View(viewModel);
        }

















        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsertPayment([FromForm] PaymentViewModel model)
        {
            try
            {
                // 1. Log the incoming model for debugging
                Console.WriteLine($"Received PaymentViewModel: CustomerId={model.CustomerId}, SessionId={model.SessionId}, Total={model.Total}");
                Console.WriteLine($"PaymentTypes count: {model.PaymentTypes?.Count ?? 0}");
                if (model.PaymentTypes != null)
                {
                    for (int i = 0; i < model.PaymentTypes.Count; i++)
                    {
                        var pt = model.PaymentTypes[i];
                        Console.WriteLine($"PaymentTypes[{i}]: Type={pt.Type}, Amount={pt.Amount}, DueDate={pt.DueDate?.ToString() ?? "null"}");
                    }
                }

                // 2. Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    Console.WriteLine("ModelState Errors: " + string.Join(", ", errors));
                    return Json(new { success = false, message = "بيانات غير صالحة", errors = errors });
                }

                // 3. Validate session ID
                if (!model.SessionId.HasValue)
                {
                    return Json(new { success = false, message = "معرف الجلسة غير موجود" });
                }

                // 4. Verify session exists
                var sessionExists = await _context.PosSessions
                    .AnyAsync(s => s.SessionId == model.SessionId.Value);
                if (!sessionExists)
                {
                    return Json(new { success = false, message = "الجلسة غير صالحة أو منتهية" });
                }

                // 5. Validate customer
                if (!model.CustomerId.HasValue)
                {
                    return Json(new { success = false, message = "الرجاء اختيار عميل" });
                }

                // 6. Validate PaymentTypes
                if (model.PaymentTypes == null || !model.PaymentTypes.Any())
                {
                    return Json(new { success = false, message = "يجب اختيار طريقة دفع واحدة على الأقل" });
                }

                // 7. Get current employee ID
                var employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 1;

                // 8. Validate PaymentTypes total
                var totalAmount = model.Total;
                var paymentTypesTotal = model.PaymentTypes.Sum(pt => pt.Amount);
                if (Math.Abs(paymentTypesTotal - totalAmount) > 0.01m)
                {
                    return Json(new { success = false, message = "مجموع مبالغ طرق الدفع لا يساوي المبلغ الإجمالي" });
                }

                // 9. Start transaction
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 10. Create Payment record
                var payment = new Payment
                {
                    CustomerId = model.CustomerId.Value,
                    VendorId = null, // Not used in this context
                    UserId = employeeId,
                    Paydate = DateOnly.FromDateTime(DateTime.Now),
                    TotalAmount = totalAmount,
                    PaymentStatus = "Paid",
                    Notes = null, // Optional, can be set if needed
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                Console.WriteLine($"Creating Payment: CustomerId={payment.CustomerId}, TotalAmount={payment.TotalAmount}");
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync(); // Save to get Payid
                Console.WriteLine($"Payment saved with Payid: {payment.Payid}");

                // 11. Create PaymentDetails records
                var paymentDetails = model.PaymentTypes.Select(paymentType => new Paymentdetail
                {
                    Payid = payment.Payid,
                    UserId = employeeId,
                    Description = $"Payment via {paymentType.Type}",
                    Amount = paymentType.Amount,
                    DueDate = paymentType.Type == "اجل"
                        ? DateOnly.FromDateTime(paymentType.DueDate ?? DateTime.Now.AddDays(30))
                        : null,
                    IsPaid = paymentType.Type != "اجل",
                    PaidDate = paymentType.Type != "اجل" ? DateOnly.FromDateTime(DateTime.Now) : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }).ToList();

                if (!paymentDetails.Any())
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "لا توجد تفاصيل دفع للحفظ" });
                }
                Console.WriteLine($"Saving {paymentDetails.Count} Paymentdetails rows");
                await _context.Paymentdetails.AddRangeAsync(paymentDetails);
                await _context.SaveChangesAsync();

                // 12. Commit transaction
                await transaction.CommitAsync();

                // 13. Return success
                return Json(new
                {
                    success = true,
                    payId = payment.Payid,
                    sessionId = model.SessionId.Value,
                    message = "تم حفظ الدفع بنجاح"
                });
            }
            catch (Exception ex)
            {
                // Enhanced error logging
                Console.WriteLine($"InsertPayment error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء حفظ الدفع",
                    error = ex.Message,
                    detailedError = ex.InnerException?.Message ?? ex.Message
                });
            }
        }





































        public async Task<IActionResult> closingshift(int? sessionId)
        {
            // Get sessionId from Session if not provided
            if (!sessionId.HasValue)
            {
                sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");
                Console.WriteLine($"Retrieved sessionId from ASP.NET session: {sessionId}");

                if (!sessionId.HasValue)
                {
                    return BadRequest("Session ID is required");
                }
            }

            var viewModel = new FinancialSummaryViewModel();

            // Cash Payments Query
            var cashResult = await (from i in _context.Invoices
                                    join p in _context.Payments on i.InvoiceId equals p.InvoiceId
                                    join pd in _context.Paymentdetails on p.Payid equals pd.Payid
                                    where i.SessionId == sessionId
                                    group pd by 1 into g
                                    select new
                                    {
                                        CashAmount = g.Sum(pd => pd.Description == "كاش" ? pd.Amount : 0),
                                        RefundAmount = g.Sum(pd => pd.Description == "إرجاع نقدي" ? Math.Abs(pd.Amount) : 0),
                                        NetAmount = g.Sum(pd => pd.Description == "كاش" ? pd.Amount : 0) -
                                                   g.Sum(pd => pd.Description == "إرجاع نقدي" ? Math.Abs(pd.Amount) : 0)
                                    }).FirstOrDefaultAsync();

            if (cashResult != null)
            {
                viewModel.CashAmount = cashResult.CashAmount;
                viewModel.RefundAmount = cashResult.RefundAmount;
                viewModel.CashNetAmount = cashResult.NetAmount;
            }

            // Bank Transfer Query
            var bankResult = await (from i in _context.Invoices
                                    join p in _context.Payments on i.InvoiceId equals p.InvoiceId
                                    join pd in _context.Paymentdetails on p.Payid equals pd.Payid
                                    where i.SessionId == sessionId
                                    group pd by 1 into g
                                    select new
                                    {
                                        BankTransferAmount = g.Sum(pd => pd.Description == "تحويل بنكي" ? pd.Amount : 0),
                                        BankTransferRefundAmount = g.Sum(pd => pd.Description == "إرجاع بتحويل بنكي" ? Math.Abs(pd.Amount) : 0),
                                        NetAmount = g.Sum(pd => pd.Description == "تحويل بنكي" ? pd.Amount : 0) -
                                                   g.Sum(pd => pd.Description == "إرجاع بتحويل بنكي" ? Math.Abs(pd.Amount) : 0)
                                    }).FirstOrDefaultAsync();

            if (bankResult != null)
            {
                viewModel.BankTransferAmount = bankResult.BankTransferAmount;
                viewModel.BankTransferRefundAmount = bankResult.BankTransferRefundAmount;
                viewModel.BankTransferNetAmount = bankResult.NetAmount;
            }

            // Deferred Payments Query
            viewModel.DeferredAmount = await (from i in _context.Invoices
                                              join p in _context.Payments on i.InvoiceId equals p.InvoiceId
                                              join pd in _context.Paymentdetails on p.Payid equals pd.Payid
                                              where i.SessionId == sessionId
                                              select pd)
                .SumAsync(pd => pd.Description == "آجل" ? pd.Amount : 0);

            // Total Net Amount Query
            viewModel.TotalNetAmount = await (from i in _context.Invoices
                                              join p in _context.Payments on i.InvoiceId equals p.InvoiceId
                                              join pd in _context.Paymentdetails on p.Payid equals pd.Payid
                                              where i.SessionId == sessionId
                                              select pd)
                .SumAsync(pd => pd.Description == "كاش" ? pd.Amount :
                               pd.Description == "إرجاع نقدي" ? -Math.Abs(pd.Amount) :
                               pd.Description == "تحويل بنكي" ? pd.Amount :
                               pd.Description == "إرجاع بتحويل بنكي" ? -Math.Abs(pd.Amount) :
                               pd.Description == "آجل" ? pd.Amount : 0);

            // Transfers Query
            var transferResult = await (from i in _context.Invoices
                                        join t in _context.Transfers on i.InvoiceId equals t.InvoiceId
                                        where i.SessionId == sessionId
                                        group t by 1 into g
                                        select new
                                        {
                                            AdditionAmount = g.Sum(t => t.Type == "اضافة" ? t.Amount : 0),
                                            DisbursementAmount = g.Sum(t => t.Type == "صرف" ? t.Amount : 0)
                                        }).FirstOrDefaultAsync();

            if (transferResult != null)
            {
                viewModel.AdditionAmount = transferResult.AdditionAmount;
                viewModel.DisbursementAmount = transferResult.DisbursementAmount;
            }

            return View(viewModel);
        }










        [HttpPost]
        public async Task<IActionResult> ClosingSession(int? sessionId)
        {
            // Get sessionId from Session if not provided
            if (!sessionId.HasValue)
            {
                sessionId = HttpContext.Session.GetInt32("CurrentPosSessionId");
                Console.WriteLine($"Retrieved sessionId from ASP.NET session: {sessionId}");

                if (!sessionId.HasValue)
                {
                    return BadRequest("Session ID is required");
                }
            }

            // Fetch the session record
            var session = await _context.PosSessions.FindAsync(sessionId);

            if (session == null)
            {
                return NotFound("POS Session not found.");
            }

            // Update the session status and ending cash/time
            session.Status = "Closed";
            session.EndTime = DateTime.Now;
            session.EndingCash = session.EndingCash ?? 0; // Optionally update this if needed

            await _context.SaveChangesAsync();

            // Redirect to home or another page
            return RedirectToAction("Index", "Home");
        }


    }
}

