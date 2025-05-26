using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;
using QRCoder; // Ensure this is present
using System.Drawing; // Ensure this is present
using System.IO; // Ensure this is present

using ZXing; // Add this using directive for ZXing
using ZXing.QrCode; // Add this using directive for QR code options
namespace WebApplication4.Controllers
{
    public class InvoicedetailsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public InvoicedetailsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }
        // GET: Invoices/Index/5
        // GET: Invoices/Index/5
        public async Task<IActionResult> Index(int? invoiceId)
        {
            try
            {
                // Check if invoiceId is null
                if (invoiceId == null)
                {
                    return NotFound("معرف الفاتورة غير متوفر.");
                }

                // Retrieve the Invoice with related data (Customer, Employee, Vendor)
                var invoice = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.Employee)
                    .Include(i => i.Vendor)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                // Check if the invoice is null
                if (invoice == null)
                {
                    return NotFound("الفاتورة غير موجودة.");
                }

                // Retrieve Invoice Details with Product Information
                var invoiceDetails = await _context.Invoicedetails
                    .Include(d => d.Product)
                    .Where(d => d.InvoiceId == invoiceId)
                    .ToListAsync();

                // Check if invoiceDetails is null or empty
                if (invoiceDetails == null || !invoiceDetails.Any())
                {
                    return NotFound("لا توجد تفاصيل فاتورة لهذه الفاتورة.");
                }

                // Log data for debugging
                foreach (var detail in invoiceDetails)
                {
                    if (detail == null || detail.Product == null)
                    {
                        Console.WriteLine($"Skipping null detail or product for DetailId={detail?.DetailId}");
                        continue;
                    }

                    // Log UnitPrice, VatAmount, LineTotal
                    Console.WriteLine($"DetailId={detail.DetailId}, ProductId={detail.ProductId}, " +
                        $"InvoiceTyping={detail.InvoiceTyping}, UnitPrice={detail.UnitPrice}, " +
                        $"Quantity={detail.Quantity}, VatAmount={detail.VatAmount ?? 0}, " +
                        $"LineTotal={detail.LineTotal }, ProductPrice={detail.Product.Price}");
                }

                // Pass the first InvoicedetailId to the view
                var firstDetailId = invoiceDetails.First().DetailId.ToString();
                ViewBag.InvoicedetailId = firstDetailId;
                ViewBag.Invoice = invoice;

                // Pass invoiceDetails as the Model to the View
                return View(invoiceDetails);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"حدث خطأ: {ex.Message} - StackTrace: {ex.StackTrace}");
                return StatusCode(500, "حدث خطأ أثناء معالجة طلبك.");
            }
        }
            public async Task<IActionResult> Details()
        {
            // Retrieve InvoiceId from the session
            var invoiceId = HttpContext.Session.GetInt32("InvoiceId");

            if (invoiceId == null || _context.Invoicedetails == null)
            {
                return NotFound();
            }

            var invoicedetail = await _context.Invoicedetails
                .Include(i => i.Invoice)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.DetailId == invoiceId);

            if (invoicedetail == null)
            {
                return NotFound();
            }

            return View(invoicedetail);
        }

        // GET: Invoicedetails/Create
        public IActionResult Create()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            if (!invoiceId.HasValue)
            {
                return RedirectToAction("Index", "Invoices");
            }

            // Filter products by UserId
            var userProducts = _context.Products.Where(p => p.UserId == userId).ToList();

            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", invoiceId.Value);
            ViewData["ProductId"] = new SelectList(userProducts, "ProductId", "ProductName");

            // Pass product prices as a dictionary (filtered by user)
            ViewBag.ProductPrices = userProducts.ToDictionary(p => p.ProductId, p => p.Price);

            ViewBag.InvoiceId = invoiceId.Value;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] List<Invoicedetail> invoicedetails)
        {
            // Get UserId and StorehouseId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            var storehouseId = HttpContext.Session.GetInt32("StorehouseId");

            if (userId == null)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            if (storehouseId == null || storehouseId == 0)
            {
                return Json(new { success = false, message = "يجب تحديد المخزن أولاً." });
            }

            if (invoicedetails == null || !invoicedetails.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            if (!invoiceId.HasValue)
            {
                return Json(new { success = false, message = "InvoiceId not found in session." });
            }

            try
            {
                foreach (var detail in invoicedetails)
                {
                    // Check if product belongs to the current user
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId && p.UserId == userId);

                    if (product == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Product with ID {detail.ProductId} not found or doesn't belong to you."
                        });
                    }

                    if (detail.Quantity > product.Balance)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"الكمية المدخلة ({detail.Quantity}) أكبر من الرصيد المتاح ({product.Balance}) للعنصر {product.ProductName}."
                        });
                    }

                    detail.InvoiceId = invoiceId.Value;
                    detail.UnitPrice = product.Price;
                    detail.InvoiceTyping = "بيع";

                    if (detail.Quantity <= 0 || detail.UnitPrice <= 0)
                    {
                        return Json(new { success = false, message = "Quantity and UnitPrice must be greater than 0." });
                    }

                    detail.LineTotal = detail.Quantity * detail.UnitPrice;
                    _context.Invoicedetails.Add(detail);

                    // Inventory update with StorehouseId
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId && i.StorehouseId == storehouseId);

                    if (inventory != null)
                    {
                        inventory.OutgoingQuantity += detail.Quantity;
                        inventory.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        inventory = new Inventory
                        {
                            ProductId = detail.ProductId,
                            OutgoingQuantity = detail.Quantity,
                            IncomingQuantity = 0,
                            LastUpdated = DateTime.Now,
                            StorehouseId = storehouseId.Value
                        };
                        _context.Inventories.Add(inventory);
                    }

                    product.Balance -= detail.Quantity;
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "تم حفظ الطلب بنجاح!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء الحفظ",
                    errorDetails = new { message = ex.Message, stackTrace = ex.StackTrace }
                });
            }
        }

        // GET: Invoicedetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invoicedetails == null)
            {
                return NotFound();
            }

            var invoicedetail = await _context.Invoicedetails.FindAsync(id);
            if (invoicedetail == null)
            {
                return NotFound();
            }
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", invoicedetail.InvoiceId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", invoicedetail.ProductId);
            return View(invoicedetail);
        }

        // POST: Invoicedetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetailId,InvoiceId,ProductId,Quantity,UnitPrice,LineTotal")] Invoicedetail invoicedetail)
        {
            if (id != invoicedetail.DetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoicedetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoicedetailExists(invoicedetail.DetailId))
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
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", invoicedetail.InvoiceId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", invoicedetail.ProductId);
            return View(invoicedetail);
        }

        // GET: Invoicedetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invoicedetails == null)
            {
                return NotFound();
            }

            var invoicedetail = await _context.Invoicedetails
                .Include(i => i.Invoice)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.DetailId == id);
            if (invoicedetail == null)
            {
                return NotFound();
            }

            return View(invoicedetail);
        }

        // POST: Invoicedetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invoicedetails == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Invoicedetails'  is null.");
            }
            var invoicedetail = await _context.Invoicedetails.FindAsync(id);
            if (invoicedetail != null)
            {
                _context.Invoicedetails.Remove(invoicedetail);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvoicedetailExists(int id)
        {
          return (_context.Invoicedetails?.Any(e => e.DetailId == id)).GetValueOrDefault();
        }












        public IActionResult StoreInvoiceId(int invoiceId)
        {
            // Store the invoiceId in the session
            HttpContext.Session.SetInt32("InvoiceId", invoiceId);

            // Redirect to the Index action in the same controller to show details
            return RedirectToAction("Index", new { invoiceId });
        }








        // GET: Invoicedetails/CreatePurshaseSend
        public IActionResult CreatePurshaseSend()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            if (!invoiceId.HasValue)
            {
                return RedirectToAction("Index", "Invoices");
            }

            // Filter products by UserId (only show products belonging to current user)
            var userProducts = _context.Products
                .Where(p => p.UserId == userId)
                .ToList();

            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", invoiceId.Value);
            ViewData["ProductId"] = new SelectList(userProducts, "ProductId", "ProductName");

            // Pass product prices as a dictionary (filtered by user)
            ViewBag.ProductPrices = userProducts.ToDictionary(p => p.ProductId, p => p.Price);
            ViewBag.InvoiceId = invoiceId.Value;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurshaseSend([FromBody] List<Invoicedetail> invoicedetails)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }

            if (invoicedetails == null || !invoicedetails.Any())
            {
                return BadRequest(new { success = false, message = "No data received." });
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            int? storehouseId = HttpContext.Session.GetInt32("StorehouseId");

            if (!invoiceId.HasValue || !storehouseId.HasValue || storehouseId == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "InvoiceId or StorehouseId not found in session."
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var detail in invoicedetails)
                {
                    // Validate required fields
                    if (detail.ProductId == 0 || detail.Quantity == 0)
                    {
                        throw new Exception("Product ID and Quantity are required.");
                    }

                    // Verify product belongs to current user
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId && p.UserId == userId);

                    if (product == null)
                    {
                        throw new Exception($"Product with ID {detail.ProductId} not found or doesn't belong to you.");
                    }

                    // Add invoice detail
                    detail.InvoiceId = invoiceId.Value;
                    detail.LineTotal = detail.Quantity * detail.UnitPrice;
                    detail.InvoiceTyping = "شراء";
                    _context.Invoicedetails.Add(detail);

                    // Update product balance (increase for purchases)
                    product.Balance += detail.Quantity;
                    _context.Products.Update(product);

                    // Create inventory record
                    var inventory = new Inventory
                    {
                        ProductId = detail.ProductId,
                        IncomingQuantity = detail.Quantity,
                        OutgoingQuantity = 0,
                        Balance = product.Balance, // Use updated balance
                        ReferenceType = "Invoice",
                        LastUpdated = DateTime.Now,
                        StorehouseId = storehouseId.Value,
                       
                    };
                    _context.Inventories.Add(inventory);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "تم حفظ الفاتورة بنجاح!",
                    redirectUrl = Url.Action("Index", "Home")
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new
                {
                    success = false,
                    message = $"حدث خطأ أثناء الحفظ: {ex.Message}",
                    errorDetails = ex.InnerException?.Message
                });
            }
        }









        public async Task<IActionResult> GetInvoiceSummary()
        {
            var result = await _context.Invoicedetails
                .Join(
                    _context.Products,
                    detail => detail.ProductId,
                    product => product.ProductId,
                    (detail, product) => new { detail, product }
                )
                .Join(
                    _context.Invoices,
                    combined => combined.detail.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (combined, invoice) => new
                    {
                        combined.detail.InvoiceTyping,
                        combined.detail.Quantity,
                        PurchasePrice = (decimal?)combined.product.PurchasePrice, // Ensure nullable
                        Price = (decimal?)combined.product.Price,               // Ensure nullable
                        Vatrate = (decimal?)combined.product.Vatrate             // Ensure nullable
                    }
                )
                .GroupBy(x => 1) // Group all records into a single group
                .Select(g => new InvoiceSummaryViewModel
                {
                    // Net Purchases
                    NetPurchases = g.Sum(x =>
                        x.InvoiceTyping == "شراء" ? (x.Quantity * x.PurchasePrice) ?? 0 :
                        x.InvoiceTyping == "مرتجع من الشراء" ? -(x.Quantity * x.PurchasePrice ?? 0) :
                        0
                    ),

                    // VAT on Purchases
                    VATPurchases = (g.Sum(x =>
                        x.InvoiceTyping == "شراء" ? (x.Quantity * x.PurchasePrice * x.Vatrate / 100) ?? 0 :
                        0
                    )) -
                    (g.Sum(x =>
                        x.InvoiceTyping == "مرتجع من الشراء" ? (x.Quantity * x.PurchasePrice * x.Vatrate / 100) ?? 0 :
                        0
                    )),

                    // Net Sales
                    NetSales = g.Sum(x =>
                        x.InvoiceTyping == "بيع" ? (x.Quantity * x.Price) ?? 0 :
                        x.InvoiceTyping == "مرتجع من البيع" ? -(x.Quantity * x.Price ?? 0) :
                        0
                    ),

                    // VAT on Sales
                    VATSales = (g.Sum(x =>
                        x.InvoiceTyping == "بيع" ? (x.Quantity * x.Price * x.Vatrate / 100) ?? 0 :
                        0
                    )) -
                    (g.Sum(x =>
                        x.InvoiceTyping == "مرتجع من البيع" ? (x.Quantity * x.Price * x.Vatrate / 100) ?? 0 :
                        0
                    )),

                    // Net VAT (VAT on Purchases - VAT on Sales)
                    NetVAT = ((g.Sum(x =>
                        x.InvoiceTyping == "شراء" ? (x.Quantity * x.PurchasePrice * x.Vatrate / 100) ?? 0 :
                        0
                    )) -
                    (g.Sum(x =>
                        x.InvoiceTyping == "مرتجع من الشراء" ? (x.Quantity * x.PurchasePrice * x.Vatrate / 100) ?? 0 :
                        0
                    ))) -
                    ((g.Sum(x =>
                        x.InvoiceTyping == "بيع" ? (x.Quantity * x.Price * x.Vatrate / 100) ?? 0 :
                        0
                    )) -
                    (g.Sum(x =>
                        x.InvoiceTyping == "مرتجع من البيع" ? (x.Quantity * x.Price * x.Vatrate / 100) ?? 0 :
                        0
                    )))
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound("No data found.");
            }

            return View(result);
        }












        // GET: Invoicedetails/ReturnPurshasing
        public IActionResult ReturnPurshasing(int invoiceId, int productId)
        {
            // Store both IDs in session for later use
            HttpContext.Session.SetInt32("InvoiceId", invoiceId);
            HttpContext.Session.SetInt32("ProductId", productId);

            // Retrieve only the specific product detail from the invoice
            var invoiceDetail = _context.Invoicedetails
                .Where(id => id.InvoiceId == invoiceId &&
                            id.ProductId == productId &&
                            id.InvoiceTyping != "مرتجع من الشراء") // Only original purchase items
                .Include(id => id.Product)
                .FirstOrDefault();

            if (invoiceDetail == null)
            {
                TempData["ErrorMessage"] = "Product not found in this invoice or already returned";
                return RedirectToAction("Index", "Invoices");
            }

            // Get total returned quantity for this specific product
            var totalReturned = _context.Invoicedetails
                .Where(id => id.InvoiceId == invoiceId &&
                            id.ProductId == productId &&
                            id.InvoiceTyping == "مرتجع من الشراء")
                .Sum(id => (int?)id.Quantity) ?? 0; // Handle null with coalesce

            // Calculate remaining quantity
            invoiceDetail.Quantity -= totalReturned;

            // Verify there's quantity left to return
            if (invoiceDetail.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "This product has already been fully returned";
                return RedirectToAction("Index", "Invoices");
            }

            // Create a list with just this single product
            var invoiceDetails = new List<Invoicedetail> { invoiceDetail };

            // Pass the invoice details to the view
            ViewBag.InvoiceDetails = invoiceDetails;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReturnPurchasing([FromBody] Dictionary<int, int> returnRequests)
        {
            if (returnRequests == null || !returnRequests.Any())
            {
                Console.WriteLine("Error: No data received in returnRequests.");
                return BadRequest("لم يتم إدخال بيانات المرتجع.");
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!invoiceId.HasValue)
            {
                Console.WriteLine("Error: InvoiceId not found in session.");
                return BadRequest("لم يتم العثور على معرف الفاتورة في الجلسة.");
            }

            if (!userId.HasValue)
            {
                Console.WriteLine("Error: UserId not found in session.");
                return BadRequest("لم يتم العثور على معرف المستخدم في الجلسة.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Retrieve the invoice to get StorehouseId and VendorId
                var invoice = await _context.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId.Value && i.UserId == userId.Value && i.InvoiceType == "شراء");
                if (invoice == null)
                {
                    Console.WriteLine($"Error: Invoice not found for InvoiceId={invoiceId.Value}, UserId={userId.Value}.");
                    return BadRequest("الفاتورة غير موجودة أو لا تخصك.");
                }
                Console.WriteLine($"Retrieved Invoice: InvoiceId={invoice.InvoiceId}, StorehouseId={invoice.StorehouseId}, VendorId={invoice.VendorId}");

                int? storehouseId = invoice.StorehouseId;
                if (!storehouseId.HasValue)
                {
                    Console.WriteLine("Error: StorehouseId not specified in invoice.");
                    return BadRequest("يجب تحديد المخزن في الفاتورة.");
                }

                // Fetch ChildAccountIds from account_mappings
                var inventoryAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "المخزون")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (inventoryAccountId == 0)
                {
                    Console.WriteLine("Error: Inventory account not defined in account_mappings.");
                    return BadRequest("حساب المخزون غير معرف في تعيينات الحسابات.");
                }

                var recoverableVatAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "ضريبة القيمة المضافة القابلة للاسترداد")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (recoverableVatAccountId == 0)
                {
                    Console.WriteLine("Error: Recoverable VAT account not defined in account_mappings.");
                    return BadRequest("حساب ضريبة القيمة المضافة القابلة للاسترداد غير معرف في تعيينات الحسابات.");
                }

                var accountsPayableAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "الحسابات المستحقة الدفع")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (accountsPayableAccountId == 0)
                {
                    Console.WriteLine("Error: Accounts payable account not defined in account_mappings.");
                    return BadRequest("حساب الحسابات المستحقة الدفع غير معرف في تعيينات الحسابات.");
                }

                var cashAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "النقدية بالصندوق")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cashAccountId == 0)
                {
                    Console.WriteLine("Error: Cash account not defined in account_mappings.");
                    return BadRequest("حساب النقدية بالصندوق غير معرف في تعيينات الحسابات.");
                }

                var bankAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "النقدية بالبنك")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (bankAccountId == 0)
                {
                    Console.WriteLine("Error: Bank account not defined in account_mappings.");
                    return BadRequest("حساب النقدية بالبنك غير معرف في تعيينات الحسابات.");
                }

                decimal totalReturnAmount = 0;
                var journalEntries = new List<JournalEntry>();
                var productCosts = new Dictionary<int, decimal>();

                foreach (var request in returnRequests)
                {
                    int productId = request.Key;
                    int quantityToReturn = request.Value;
                    Console.WriteLine($"Processing return: ProductId={productId}, QuantityToReturn={quantityToReturn}");

                    // Retrieve original invoice detail
                    var originalInvoiceDetail = await _context.Invoicedetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(id => id.InvoiceId == invoiceId.Value && id.ProductId == productId && id.InvoiceTyping == "شراء");
                    if (originalInvoiceDetail == null)
                    {
                        Console.WriteLine($"Error: Invoice detail not found for ProductId={productId}, InvoiceId={invoiceId.Value}.");
                        return BadRequest($"تفاصيل الفاتورة غير موجودة للمنتج برقم {productId}.");
                    }
                    Console.WriteLine($"Original InvoiceDetail: ProductId={originalInvoiceDetail.ProductId}, Quantity={originalInvoiceDetail.Quantity}, UnitPrice={originalInvoiceDetail.UnitPrice}, VatAmount={originalInvoiceDetail.VatAmount}, LineTotal={originalInvoiceDetail.LineTotal}");

                    // Calculate total returned quantity
                    var totalReturned = await _context.Invoicedetails
                        .AsNoTracking()
                        .Where(id => id.InvoiceId == invoiceId.Value && id.ProductId == productId && id.InvoiceTyping == "مرتجع من الشراء")
                        .SumAsync(id => id.Quantity);
                    int remainingQuantity = originalInvoiceDetail.Quantity - totalReturned;
                    if (quantityToReturn > remainingQuantity)
                    {
                        Console.WriteLine($"Error: Cannot return {quantityToReturn} units for ProductId={productId}. RemainingQuantity={remainingQuantity}.");
                        return BadRequest($"لا يمكن إرجاع أكثر من {remainingQuantity} وحدة للمنتج برقم {productId}.");
                    }

                    // Retrieve product for VAT rate and purchase price
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == productId && p.UserId == userId);
                    if (product == null)
                    {
                        Console.WriteLine($"Error: Product not found for ProductId={productId}, UserId={userId.Value}.");
                        return BadRequest($"المنتج برقم {productId} غير موجود.");
                    }
                    if (product.PurchasePrice == null || product.Vatrate == null)
                    {
                        Console.WriteLine($"Error: PurchasePrice={product.PurchasePrice}, Vatrate={product.Vatrate} not defined for ProductId={productId}.");
                        return BadRequest($"سعر الشراء أو معدل الضريبة غير معرف للمنتج برقم {productId}.");
                    }
                    Console.WriteLine($"Product: ProductId={product.ProductId}, PurchasePrice={product.PurchasePrice}, Vatrate={product.Vatrate}, Balance={product.Balance}");

                    // Calculate VAT, subtotal, and line total
                    decimal vatRate = product.Vatrate ?? 0m;
                    decimal subtotal = quantityToReturn * originalInvoiceDetail.UnitPrice;
                    decimal vatAmount = Math.Round(subtotal * (vatRate / 100), 2, MidpointRounding.AwayFromZero);
                    decimal lineTotal = subtotal + vatAmount;
                    decimal inventoryCost = quantityToReturn * product.PurchasePrice;
                    Console.WriteLine($"Calculations for ProductId={productId}: Quantity={quantityToReturn}, UnitPrice={originalInvoiceDetail.UnitPrice}, VatRate={vatRate}%, Subtotal={subtotal}, VatAmount={vatAmount}, LineTotal={lineTotal}, InventoryCost={inventoryCost}");

                    // Create new invoice detail
                    var newInvoiceDetail = new Invoicedetail
                    {
                        InvoiceId = invoiceId.Value,
                        ProductId = productId,
                        Quantity = quantityToReturn,
                        UnitPrice = originalInvoiceDetail.UnitPrice,
                        LineTotal = lineTotal,
                        VatAmount = vatAmount,
                        InvoiceTyping = "مرتجع من الشراء"
                    };
                    _context.Invoicedetails.Add(newInvoiceDetail);

                    // Update inventory
                    var inventory = new Inventory
                    {
                        ProductId = productId,
                        IncomingQuantity = 0,
                        OutgoingQuantity = quantityToReturn,
                        UserId = userId.Value,

                        LastUpdated = DateTime.Now,
                        StorehouseId = storehouseId.Value
                    };
                    _context.Inventories.Add(inventory);

                    // Update product balance
                    product.Balance -= quantityToReturn;
                    if (product.Balance < 0)
                    {
                        Console.WriteLine($"Error: Insufficient product balance for ProductId={productId}, Balance={product.Balance}, QuantityToReturn={quantityToReturn}.");
                        return BadRequest($"رصيد المنتج غير كافٍ للمنتج برقم {productId}.");
                    }
                    _context.Products.Update(product);

                    totalReturnAmount += lineTotal;
                    productCosts[productId] = inventoryCost;

                    // Journal entries for purchase return
                    // Credit: Inventory (reverse original debit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"مرتجع مشتريات - فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                        DebitAmount = 0,
                        CreditAmount = inventoryCost,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من الشراء",
                        ChildAccountId = inventoryAccountId, // e.g., 3
                        //ProductId = productId
                    });

                    // Credit: Recoverable VAT (reverse original debit)
                    if (vatAmount > 0)
                    {
                        var existingVatEntry = await _context.JournalEntries
                            .AnyAsync(je => je.InvoiceId == invoice.InvoiceId && je.InvoiceType == "مرتجع من الشراء" && je.ChildAccountId == recoverableVatAccountId && je.Id == productId);
                        if (existingVatEntry)
                        {
                            Console.WriteLine($"Warning: Duplicate VAT entry detected for InvoiceId={invoice.InvoiceId}, ProductId={productId}, ChildAccountId={recoverableVatAccountId}");
                        }

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مرتجع مشتريات - فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                            DebitAmount = 0,
                            CreditAmount = vatAmount,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من الشراء",
                            ChildAccountId = recoverableVatAccountId, // e.g., 5
                            //ProductId = productId
                        });
                    }
                }

                // Process payment refunds
                var payment = await _context.Payments
                    .Include(p => p.Paymentdetails)
                    .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId.Value);
                if (payment == null)
                {
                    Console.WriteLine($"Error: Payment not found for InvoiceId={invoiceId.Value}.");
                    return BadRequest("لم يتم العثور على الدفع المرتبط بهذه الفاتورة.");
                }
                Console.WriteLine($"Payment: Payid={payment.Payid}, TotalAmount={payment.TotalAmount}, PaymentStatus={payment.PaymentStatus}, PaymentdetailsCount={(payment.Paymentdetails?.Count ?? 0)}");

                if (payment.Paymentdetails == null || !payment.Paymentdetails.Any())
                {
                    Console.WriteLine($"Error: No Paymentdetails found for Payid={payment.Payid}.");
                    return BadRequest("لا توجد تفاصيل دفع مرتبطة بالدفع.");
                }

                decimal remainingReturn = totalReturnAmount;
                Console.WriteLine($"TotalReturnAmount={totalReturnAmount}, RemainingReturn={remainingReturn}");

                // Refund priority: Credit (آجل), Cash (كاش), Bank (تحويل بنكي)
                // 1. Credit (Accounts Payable)
                var creditDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "آجل");
                if (creditDetail != null && creditDetail.Amount > 0 && remainingReturn > 0)
                {
                    var amount = Math.Min(creditDetail.Amount, remainingReturn);
                    Console.WriteLine($"Credit Refund: Payid={creditDetail.Payid}, OriginalAmount={creditDetail.Amount}, RefundAmount={amount}");
                    creditDetail.Amount -= amount;
                    if (creditDetail.Amount < 0)
                    {
                        Console.WriteLine($"Error: Credit detail amount would become negative: {creditDetail.Amount}.");
                        return BadRequest("لا يمكن تقليل مبلغ الدفع الآجل إلى قيمة سالبة.");
                    }
                    creditDetail.UpdatedAt = DateTime.Now;
                    _context.Paymentdetails.Update(creditDetail);
                    remainingReturn -= amount;

                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تسوية رصيد آجل - مرتجع فاتورة رقم {invoice.InvoiceId}",
                        DebitAmount = amount,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من الشراء",
                        ChildAccountId = accountsPayableAccountId, // e.g., 8
                        //ProductId = null // No specific product for payment entries
                    });
                }

                // 2. Cash
                if (remainingReturn > 0)
                {
                    var cashDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "كاش");
                    if (cashDetail != null && cashDetail.Amount > 0)
                    {
                        var amount = Math.Min(cashDetail.Amount, remainingReturn);
                        Console.WriteLine($"Cash Refund: Payid={cashDetail.Payid}, OriginalAmount={cashDetail.Amount}, RefundAmount={amount}");
                        _context.Paymentdetails.Add(new Paymentdetail
                        {
                            Payid = payment.Payid,
                            UserId = userId.Value,
                            Description = "إرجاع نقدي",
                            Amount = amount,
                            DueDate = DateOnly.FromDateTime(DateTime.Now),
                            IsPaid = true,
                            PaidDate = DateOnly.FromDateTime(DateTime.Now),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        if (invoice.InvoiceCashAccountId != null)
                        {
                            var cashAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceCashAccountId.Value);
                            if (cashAccount == null)
                            {
                                Console.WriteLine($"Error: Cash account not found for AccountId={invoice.InvoiceCashAccountId.Value}.");
                                return BadRequest($"الحساب النقدي برقم {invoice.InvoiceCashAccountId.Value} غير موجود.");
                            }
                            if (cashAccount.ChildAccountId == null && cashAccount.AccountType == "خزينة")
                            {
                                cashAccount.ChildAccountId = cashAccountId; // e.g., 1
                            }
                            cashAccount.Balance += amount; // Increase balance for refund
                            _context.Cashandbankaccounts.Update(cashAccount);
                            Console.WriteLine($"Updated Cash Account: AccountId={cashAccount.AccountId}, ChildAccountId={cashAccount.ChildAccountId}, NewBalance={cashAccount.Balance}");
                        }

                        remainingReturn -= amount;

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"استرداد نقدي - مرتجع فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من الشراء",
                            ChildAccountId = cashAccountId, // e.g., 1
                            //ProductId = null // No specific product for payment entries
                        });
                    }
                }

                // 3. Bank
                if (remainingReturn > 0)
                {
                    var bankDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "تحويل بنكي");
                    if (bankDetail != null && bankDetail.Amount > 0)
                    {
                        var amount = Math.Min(bankDetail.Amount, remainingReturn);
                        Console.WriteLine($"Bank Refund: Payid={bankDetail.Payid}, OriginalAmount={bankDetail.Amount}, RefundAmount={amount}");
                        _context.Paymentdetails.Add(new Paymentdetail
                        {
                            Payid = payment.Payid,
                            UserId = userId.Value,
                            Description = "إرجاع بتحويل بنكي",
                            Amount = amount,
                            DueDate = DateOnly.FromDateTime(DateTime.Now),
                            IsPaid = true,
                            PaidDate = DateOnly.FromDateTime(DateTime.Now),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        if (invoice.InvoiceBankAccountId != null)
                        {
                            var bankAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceBankAccountId.Value);
                            if (bankAccount == null)
                            {
                                Console.WriteLine($"Error: Bank account not found for AccountId={invoice.InvoiceBankAccountId.Value}.");
                                return BadRequest($"الحساب البنكي برقم {invoice.InvoiceBankAccountId.Value} غير موجود.");
                            }
                            if (bankAccount.ChildAccountId == null && bankAccount.AccountType == "حساب بنكي")
                            {
                                bankAccount.ChildAccountId = bankAccountId; // e.g., 2
                            }
                            bankAccount.Balance += amount; // Increase balance for refund
                            _context.Cashandbankaccounts.Update(bankAccount);
                            Console.WriteLine($"Updated Bank Account: AccountId={bankAccount.AccountId}, ChildAccountId={bankAccount.ChildAccountId}, NewBalance={bankAccount.Balance}");
                        }

                        remainingReturn -= amount;

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"استرداد بنكي - مرتجع فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = amount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من الشراء",
                            ChildAccountId = bankAccountId, // e.g., 2
                            //ProductId = null // No specific product for payment entries
                        });
                    }
                }

                if (remainingReturn > 0)
                {
                    Console.WriteLine($"Error: Unable to refund remaining amount={remainingReturn} due to insufficient payment method balances.");
                    await transaction.RollbackAsync();
                    return BadRequest("غير قادر على إرجاع المبلغ المتبقي بسبب عدم كفاية الأرصدة في طرق الدفع.");
                }

                // Log journal entries before saving
                Console.WriteLine($"Adding {journalEntries.Count} JournalEntries to context");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }
                _context.JournalEntries.AddRange(journalEntries);

                // Save changes
                Console.WriteLine("Calling SaveChangesAsync...");
                await _context.SaveChangesAsync();
                Console.WriteLine("SaveChangesAsync completed.");

                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed.");

                // Log saved journal entries
                var savedJournalEntries = await _context.JournalEntries
                    .AsNoTracking()
                    .Where(j => j.UserId == userId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now))
                    .ToListAsync();
                Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                foreach (var entry in savedJournalEntries)
                {
                    Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, EntryDate={entry.EntryDate}, Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }

                return Json(new { success = true, message = "تم إرجاع الكمية واسترداد المبلغ (شامل الضريبة) وقيود اليومية بنجاح!" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DbUpdateException in ReturnPurchasing POST: {ex.Message}\n{ex.StackTrace}");
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
                Console.WriteLine($"Exception in ReturnPurchasing POST: Type={ex.GetType().Name}, Message={ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: Type={ex.InnerException.GetType().Name}, Message={ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء الحفظ.",
                    errorDetails = new
                    {
                        type = ex.GetType().Name,
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }



        // GET: Invoicedetails/ReturnSales
        public IActionResult ReturnSales(int invoiceId, int productId)
        {
            // Store both IDs in session for later use
            HttpContext.Session.SetInt32("InvoiceId", invoiceId);
            HttpContext.Session.SetInt32("ProductId", productId);

            // ✅ استرجاع الحسابات من الجلسة
            int? cashAccountId = HttpContext.Session.GetInt32("InvoiceCashAccountId");
            int? bankAccountId = HttpContext.Session.GetInt32("InvoiceBankAccountId");

            // ✅ طباعة الحسابات في Console
            Console.WriteLine($"Cash Account ID from session: {cashAccountId}");
            Console.WriteLine($"Bank Account ID from session: {bankAccountId}");

            // Retrieve only the specific product detail from the invoice
            var invoiceDetail = _context.Invoicedetails
                .Where(id => id.InvoiceId == invoiceId && id.ProductId == productId)
                .Include(id => id.Product)
                .FirstOrDefault();

            if (invoiceDetail == null)
            {
                return RedirectToAction("Index", "Invoices"); // Redirect if product not found
            }

            // Get total returned quantity for this specific product
            var totalReturned = _context.Invoicedetails
                .Where(id => id.InvoiceId == invoiceId &&
                             id.ProductId == productId &&
                             id.InvoiceTyping == "مرتجع من البيع")
                .Sum(id => id.Quantity);

            // Calculate remaining quantity
            invoiceDetail.Quantity -= totalReturned;

            // Create a list with just this single product (to maintain your view compatibility)
            var invoiceDetails = new List<Invoicedetail> { invoiceDetail };
            
            // Pass the invoice details to the view
            ViewBag.InvoiceDetails = invoiceDetails;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ReturnSales([FromBody] Dictionary<int, int> returnRequests)
        {
            if (returnRequests == null || !returnRequests.Any())
            {
                Console.WriteLine("Error: No data received in returnRequests.");
                return BadRequest("No data received.");
            }

            int? invoiceId = HttpContext.Session.GetInt32("InvoiceId");
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!invoiceId.HasValue)
            {
                Console.WriteLine("Error: InvoiceId not found in session.");
                return BadRequest("InvoiceId not found in session.");
            }

            if (!userId.HasValue)
            {
                Console.WriteLine("Error: UserId not found in session.");
                return BadRequest("UserId not found in session.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Retrieve the invoice to get StorehouseId
                var invoice = await _context.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId.Value);
                if (invoice == null)
                {
                    Console.WriteLine($"Error: Invoice not found for InvoiceId={invoiceId.Value}.");
                    return BadRequest("Invoice not found.");
                }
                Console.WriteLine($"Retrieved Invoice: InvoiceId={invoice.InvoiceId}, StorehouseId={invoice.StorehouseId}, CustomerId={invoice.CustomerId}");

                int? storehouseId = invoice.StorehouseId;
                if (!storehouseId.HasValue)
                {
                    Console.WriteLine("Error: StorehouseId not specified in invoice.");
                    return BadRequest("StorehouseId not specified in invoice.");
                }

                // Fetch ChildAccountIds from account_mappings
                var salesAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "إيرادات المبيعات")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (salesAccountId == 0)
                {
                    Console.WriteLine("Error: Sales account not defined in account_mappings.");
                    return BadRequest("حساب إيرادات المبيعات غير معرف في تعيينات الحسابات.");
                }

                var vatAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "ضريبة القيمة المضافة المستحقة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (vatAccountId == 0)
                {
                    Console.WriteLine("Error: VAT account not defined in account_mappings.");
                    return BadRequest("حساب ضريبة القيمة المضافة المستحقة غير معرف في تعيينات الحسابات.");
                }
                if (vatAccountId != 6)
                {
                    Console.WriteLine($"Error: VAT account ChildAccountId must be 6, found {vatAccountId}.");
                    return BadRequest($"حساب ضريبة القيمة المضافة المستحقة يجب أن يكون child_account_id = 6، ولكن تم العثور على {vatAccountId}.");
                }

                var accountsReceivableAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "الحسابات المستحقة القبض")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (accountsReceivableAccountId == 0)
                {
                    Console.WriteLine("Error: Accounts receivable account not defined in account_mappings.");
                    return BadRequest("حساب الحسابات المستحقة القبض غير معرف في تعيينات الحسابات.");
                }

                var cashAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "النقدية بالصندوق")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cashAccountId == 0)
                {
                    Console.WriteLine("Error: Cash account not defined in account_mappings.");
                    return BadRequest("حساب النقدية بالصندوق غير معرف في تعيينات الحسابات.");
                }

                var bankAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "النقدية بالبنك")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (bankAccountId == 0)
                {
                    Console.WriteLine("Error: Bank account not defined in account_mappings.");
                    return BadRequest("حساب النقدية بالبنك غير معرف في تعيينات الحسابات.");
                }

                var inventoryAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "المخزون")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (inventoryAccountId == 0)
                {
                    Console.WriteLine("Error: Inventory account not defined in account_mappings.");
                    return BadRequest("حساب المخزون غير معرف في تعيينات الحسابات.");
                }

                var cogsAccountId = await _context.AccountMappings
                    .AsNoTracking()
                    .Where(m => m.TransactionType == "تكلفة البضاعة المباعة")
                    .Select(m => m.ChildAccountId)
                    .FirstOrDefaultAsync();
                if (cogsAccountId == 0)
                {
                    Console.WriteLine("Error: COGS account not defined in account_mappings.");
                    return BadRequest("حساب تكلفة البضاعة المباعة غير معرف في تعيينات الحسابات.");
                }

                decimal totalReturnAmount = 0;
                var journalEntries = new List<JournalEntry>();
                var productCosts = new Dictionary<int, decimal>();

                foreach (var request in returnRequests)
                {
                    int productId = request.Key;
                    int quantityToReturn = request.Value;
                    Console.WriteLine($"Processing return: ProductId={productId}, QuantityToReturn={quantityToReturn}");

                    var originalInvoiceDetail = await _context.Invoicedetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(id => id.InvoiceId == invoiceId.Value && id.ProductId == productId && id.InvoiceTyping != "مرتجع من البيع");
                    if (originalInvoiceDetail == null)
                    {
                        Console.WriteLine($"Error: Invoice detail not found for ProductId={productId}, InvoiceId={invoiceId.Value}.");
                        return BadRequest($"Invoice detail not found for ProductId: {productId}");
                    }
                    Console.WriteLine($"Original InvoiceDetail: ProductId={originalInvoiceDetail.ProductId}, Quantity={originalInvoiceDetail.Quantity}, UnitPrice={originalInvoiceDetail.UnitPrice}, VatAmount={originalInvoiceDetail.VatAmount}, LineTotal={originalInvoiceDetail.LineTotal}");

                    var totalReturned = await _context.Invoicedetails
                        .AsNoTracking()
                        .Where(id => id.InvoiceId == invoiceId.Value && id.ProductId == productId && id.InvoiceTyping == "مرتجع من البيع")
                        .SumAsync(id => id.Quantity);
                    int remainingQuantity = originalInvoiceDetail.Quantity - totalReturned;
                    if (quantityToReturn > remainingQuantity)
                    {
                        Console.WriteLine($"Error: Cannot return {quantityToReturn} units for ProductId={productId}. RemainingQuantity={remainingQuantity}.");
                        return BadRequest($"Cannot return more than {remainingQuantity} units for ProductId: {productId}");
                    }

                    // Retrieve product for VAT rate and purchase price
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == productId && p.UserId == userId);
                    if (product == null)
                    {
                        Console.WriteLine($"Error: Product not found for ProductId={productId}, UserId={userId.Value}.");
                        return BadRequest($"Product not found for ProductId: {productId}");
                    }
                    if (product.PurchasePrice == null || product.Vatrate == null)
                    {
                        Console.WriteLine($"Error: PurchasePrice={product.PurchasePrice}, Vatrate={product.Vatrate} not defined for ProductId={productId}.");
                        return BadRequest($"Purchase price or VAT rate not defined for ProductId: {productId}");
                    }
                    Console.WriteLine($"Product: ProductId={product.ProductId}, PurchasePrice={product.PurchasePrice}, Vatrate={product.Vatrate}, Balance={product.Balance}");

                    // Calculate VAT, subtotal, and line total
                    decimal vatRate = product.Vatrate ?? 0m;
                    decimal subtotal = quantityToReturn * originalInvoiceDetail.UnitPrice;
                    decimal vatAmount = Math.Round(subtotal * (vatRate / 100), 2, MidpointRounding.AwayFromZero);
                    decimal lineTotal = subtotal + vatAmount;
                    decimal cogsAmount = quantityToReturn * product.PurchasePrice;
                    Console.WriteLine($"Calculations for ProductId={productId}: Quantity={quantityToReturn}, UnitPrice={originalInvoiceDetail.UnitPrice}, VatRate={vatRate}%, Subtotal={subtotal}, VatAmount={vatAmount}, LineTotal={lineTotal}, COGSAmount={cogsAmount}");

                    var newInvoiceDetail = new Invoicedetail
                    {
                        InvoiceId = invoiceId.Value,
                        ProductId = productId,
                        Quantity = quantityToReturn,
                        UnitPrice = originalInvoiceDetail.UnitPrice,
                        LineTotal = lineTotal,
                        VatAmount = vatAmount,
                        InvoiceTyping = "مرتجع من البيع"
                    };
                    _context.Invoicedetails.Add(newInvoiceDetail);

                    var inventory = new Inventory
                    {
                        ProductId = productId,
                        IncomingQuantity = quantityToReturn,
                        OutgoingQuantity = 0,
                        LastUpdated = DateTime.Now,
                        UserId = userId.Value,
                        StorehouseId = storehouseId.Value
                    };
                    _context.Inventories.Add(inventory);

                    // Update Product Balance
                    product.Balance += quantityToReturn;
                    _context.Products.Update(product);

                    totalReturnAmount += lineTotal;
                    productCosts[productId] = cogsAmount;

                    // Journal entries for sales return
                    // Debit: Sales Revenue (reverse original credit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"مرتجع مبيعات - فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                        DebitAmount = subtotal,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = salesAccountId // e.g., 7
                    });

                    // Debit: VAT Payable (reverse original credit)
                    if (vatAmount > 0)
                    {
                        // Check for existing conflicting VAT entries
                        var existingVatEntry = await _context.JournalEntries
                            .AnyAsync(je => je.InvoiceId == invoice.InvoiceId && je.InvoiceType == "مرتجع من البيع" && je.ChildAccountId == vatAccountId && je.Id == productId);
                        if (existingVatEntry)
                        {
                            Console.WriteLine($"Warning: Duplicate VAT entry detected for InvoiceId={invoice.InvoiceId}, ProductId={productId}, ChildAccountId={vatAccountId}");
                        }

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مرتجع مبيعات - فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                            DebitAmount = vatAmount,
                            CreditAmount = 0,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من البيع",
                            ChildAccountId = vatAccountId, // e.g., 6
                            //ProductId = productId // Use ProductId for tracking
                        });
                    }

                    // Debit: Inventory (reverse original credit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"زيادة المخزون - مرتجع فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                        DebitAmount = cogsAmount,
                        CreditAmount = 0,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = inventoryAccountId // e.g., 3
                    });

                    // Credit: COGS (reverse original debit)
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تخفيض تكلفة البضاعة المباعة - مرتجع فاتورة رقم {invoice.InvoiceId}, منتج {productId}",
                        DebitAmount = 0,
                        CreditAmount = cogsAmount,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = cogsAccountId // e.g., 9
                    });
                }

                // Process payment refunds
                var payment = await _context.Payments
                    .Include(p => p.Paymentdetails)
                    .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId.Value);
                if (payment == null)
                {
                    Console.WriteLine($"Error: Payment not found for InvoiceId={invoiceId.Value}.");
                    return BadRequest("لم يتم العثور على الدفع المرتبط بهذه الفاتورة.");
                }
                Console.WriteLine($"Payment: Payid={payment.Payid}, TotalAmount={payment.TotalAmount}, PaymentStatus={payment.PaymentStatus}, PaymentdetailsCount={(payment.Paymentdetails?.Count ?? 0)}");

                if (payment.Paymentdetails == null || !payment.Paymentdetails.Any())
                {
                    Console.WriteLine($"Error: No Paymentdetails found for Payid={payment.Payid}.");
                    return BadRequest("لا توجد تفاصيل دفع مرتبطة بالدفع.");
                }

                decimal remainingReturn = totalReturnAmount;
                Console.WriteLine($"TotalReturnAmount={totalReturnAmount}, RemainingReturn={remainingReturn}");

                // Refund priority: Credit (آجل), Cash (كاش), Bank (تحويل بنكي)
                // 1. Credit (Accounts Receivable)
                var creditDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "آجل");
                if (creditDetail != null && creditDetail.Amount > 0 && remainingReturn > 0)
                {
                    var amount = Math.Min(creditDetail.Amount, remainingReturn);
                    Console.WriteLine($"Credit Refund: Payid={creditDetail.Payid}, OriginalAmount={creditDetail.Amount}, RefundAmount={amount}");
                    creditDetail.Amount -= amount;
                    if (creditDetail.Amount < 0)
                    {
                        Console.WriteLine($"Error: Credit detail amount would become negative: {creditDetail.Amount}.");
                        return BadRequest("لا يمكن تقليل مبلغ الدفع الآجل إلى قيمة سالبة.");
                    }
                    creditDetail.UpdatedAt = DateTime.Now;
                    _context.Paymentdetails.Update(creditDetail);
                    remainingReturn -= amount;

                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تسوية رصيد آجل - مرتجع فاتورة رقم {invoice.InvoiceId}",
                        DebitAmount = 0,
                        CreditAmount = amount,
                        UserId = userId.Value,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "مرتجع من البيع",
                        ChildAccountId = accountsReceivableAccountId // e.g., 4
                    });
                }

                // 2. Cash
                if (remainingReturn > 0)
                {
                    var cashDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "كاش");
                    if (cashDetail != null && cashDetail.Amount > 0)
                    {
                        var amount = Math.Min(cashDetail.Amount, remainingReturn);
                        Console.WriteLine($"Cash Refund: Payid={cashDetail.Payid}, OriginalAmount={cashDetail.Amount}, RefundAmount={amount}");
                        _context.Paymentdetails.Add(new Paymentdetail
                        {
                            Payid = payment.Payid,
                            UserId = userId.Value,
                            Description = "إرجاع نقدي",
                            Amount = amount,
                            DueDate = DateOnly.FromDateTime(DateTime.Now),
                            IsPaid = true,
                            PaidDate = DateOnly.FromDateTime(DateTime.Now),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        if (invoice.InvoiceCashAccountId != null)
                        {
                            var cashAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceCashAccountId.Value);
                            if (cashAccount == null)
                            {
                                Console.WriteLine($"Error: Cash account not found for AccountId={invoice.InvoiceCashAccountId.Value}.");
                                return BadRequest($"الحساب النقدي برقم {invoice.InvoiceCashAccountId.Value} غير موجود.");
                            }
                            if (cashAccount.ChildAccountId == null && cashAccount.AccountType == "خزينة")
                            {
                                cashAccount.ChildAccountId = cashAccountId; // Use mapped ChildAccountId
                            }
                            cashAccount.Balance -= amount;
                            if (cashAccount.Balance < 0)
                            {
                                Console.WriteLine($"Error: Insufficient cash balance for AccountId={cashAccount.AccountId}, Balance={cashAccount.Balance}, RefundAmount={amount}.");
                                return BadRequest($"الرصيد النقدي غير كافٍ في الحساب {cashAccount.AccountId}.");
                            }
                            _context.Cashandbankaccounts.Update(cashAccount);
                            Console.WriteLine($"Updated Cash Account: AccountId={cashAccount.AccountId}, ChildAccountId={cashAccount.ChildAccountId}, NewBalance={cashAccount.Balance}");
                        }

                        remainingReturn -= amount;

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"استرداد نقدي - مرتجع فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = 0,
                            CreditAmount = amount,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من البيع",
                            ChildAccountId = cashAccountId // e.g., 1
                        });
                    }
                }

                // 3. Bank
                if (remainingReturn > 0)
                {
                    var bankDetail = payment.Paymentdetails.FirstOrDefault(p => p.Description.Trim() == "تحويل بنكي");
                    if (bankDetail != null && bankDetail.Amount > 0)
                    {
                        var amount = Math.Min(bankDetail.Amount, remainingReturn);
                        Console.WriteLine($"Bank Refund: Payid={bankDetail.Payid}, OriginalAmount={bankDetail.Amount}, RefundAmount={amount}");
                        _context.Paymentdetails.Add(new Paymentdetail
                        {
                            Payid = payment.Payid,
                            UserId = userId.Value,
                            Description = "إرجاع بتحويل بنكي",
                            Amount = amount,
                            DueDate = DateOnly.FromDateTime(DateTime.Now),
                            IsPaid = true,
                            PaidDate = DateOnly.FromDateTime(DateTime.Now),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });

                        if (invoice.InvoiceBankAccountId != null)
                        {
                            var bankAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceBankAccountId.Value);
                            if (bankAccount == null)
                            {
                                Console.WriteLine($"Error: Bank account not found for AccountId={invoice.InvoiceBankAccountId.Value}.");
                                return BadRequest($"الحساب البنكي برقم {invoice.InvoiceBankAccountId.Value} غير موجود.");
                            }
                            if (bankAccount.ChildAccountId == null && bankAccount.AccountType == "حساب بنكي")
                            {
                                bankAccount.ChildAccountId = bankAccountId; // Use mapped ChildAccountId
                            }
                            bankAccount.Balance -= amount;
                            if (bankAccount.Balance < 0)
                            {
                                Console.WriteLine($"Error: Insufficient bank balance for AccountId={bankAccount.AccountId}, Balance={bankAccount.Balance}, RefundAmount={amount}.");
                                return BadRequest($"الرصيد البنكي غير كافٍ في الحساب {bankAccount.AccountId}.");
                            }
                            _context.Cashandbankaccounts.Update(bankAccount);
                            Console.WriteLine($"Updated Bank Account: AccountId={bankAccount.AccountId}, ChildAccountId={bankAccount.ChildAccountId}, NewBalance={bankAccount.Balance}");
                        }

                        remainingReturn -= amount;

                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"استرداد بنكي - مرتجع فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = 0,
                            CreditAmount = amount,
                            UserId = userId.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "مرتجع من البيع",
                            ChildAccountId = bankAccountId // e.g., 2
                        });
                    }
                }

                if (remainingReturn > 0)
                {
                    Console.WriteLine($"Error: Unable to refund remaining amount={remainingReturn} due to insufficient payment method balances.");
                    await transaction.RollbackAsync();
                    return BadRequest("غير قادر على إرجاع المبلغ المتبقي بسبب عدم كفاية الأرصدة في طرق الدفع.");
                }

                // Log journal entries before saving
                Console.WriteLine($"Adding {journalEntries.Count} JournalEntries to context");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }
                _context.JournalEntries.AddRange(journalEntries);

                // Save changes
                Console.WriteLine("Calling SaveChangesAsync...");
                await _context.SaveChangesAsync();
                Console.WriteLine("SaveChangesAsync completed.");

                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed.");

                // Log saved journal entries
                var savedJournalEntries = await _context.JournalEntries
                    .AsNoTracking()
                    .Where(j => j.UserId == userId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now))
                    .ToListAsync();
                Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                foreach (var entry in savedJournalEntries)
                {
                    Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, EntryDate={entry.EntryDate}, Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceId={entry.InvoiceId}, InvoiceType={entry.InvoiceType}, ProductId={entry.Id}");
                }

                return Json(new { success = true, message = "تم إرجاع الكمية واسترداد المبلغ (شامل الضريبة) وقيود اليومية بنجاح!" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DbUpdateException in ReturnSales POST: {ex.Message}\n{ex.StackTrace}");
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
                Console.WriteLine($"Exception in ReturnSales POST: Type={ex.GetType().Name}, Message={ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: Type={ex.InnerException.GetType().Name}, Message={ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء الحفظ.",
                    errorDetails = new
                    {
                        type = ex.GetType().Name,
                        message = ex.Message,
                        innerMessage = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace
                    }
                });
            }
        }








        public async Task<IActionResult> IndexingReturnPurshasing(int? invoiceId)
        {
            if (invoiceId == null)
            {
                return NotFound();
            }

            // Retrieve Invoice with related data
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            // Retrieve Invoice Details with Product Information where InvoiceTyping is "مرتجع من الشراء"
            var invoiceDetails = await _context.Invoicedetails
                .Include(d => d.Product)
                .Where(d => d.InvoiceId == invoiceId && d.InvoiceTyping == "مرتجع من الشراء") // Filter by InvoiceTyping
                .ToListAsync();

            // Pass Invoice data to View
            ViewBag.Invoice = invoice;

            return View(invoiceDetails); // Pass filtered invoiceDetails as Model
        }









        public async Task<IActionResult> IndexingReturnSales(int? invoiceId)
        {
            if (invoiceId == null)
            {
                return NotFound();
            }

            // Retrieve Invoice with related data
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            // Retrieve Invoice Details with Product Information where InvoiceTyping is "مرتجع من البيع"
            var invoiceDetails = await _context.Invoicedetails
                .Include(d => d.Product)
                .Where(d => d.InvoiceId == invoiceId && d.InvoiceTyping == "مرتجع من البيع") // Filter by InvoiceTyping
                .ToListAsync();

            // Pass Invoice data to View
            ViewBag.Invoice = invoice;

            return View(invoiceDetails); // Pass filtered invoiceDetails as Model
        }
    }
}
