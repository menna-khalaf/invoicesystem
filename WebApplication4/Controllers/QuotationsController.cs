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
    public class QuotationsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public QuotationsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Quotations
        public async Task<IActionResult> Index()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var invoiceSystemrahtkContext = _context.Quotations
                .Where(q => q.UserId == userId.Value)
                .Include(q => q.Branch)
                .Include(q => q.Customer)
                .Include(q => q.Employee)
                .Include(q => q.Invoice)
                .Include(q => q.Storehouse)
                .Include(q => q.User)
                // Ensure related entities are not null
                .Where(q => q.Customer != null && q.Employee != null && q.Branch != null && q.Storehouse != null);

            var quotations = await invoiceSystemrahtkContext.ToListAsync();

            // Log quotations with missing Customer for debugging
            var allQuotations = await _context.Quotations
                .Where(q => q.UserId == userId.Value)
                .ToListAsync();
            foreach (var q in allQuotations)
            {
                if (q.CustomerId == null)
                {
                    Console.WriteLine($"Quotation {q.QuotationId} has null CustomerId.");
                }
            }

            return View(quotations);
        }

        // GET: Quotations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var quotation = await _context.Quotations
                .Include(q => q.Branch)
                .Include(q => q.Customer)
                .Include(q => q.Employee)
                .Include(q => q.Invoice)
                .Include(q => q.Storehouse)
                .Include(q => q.User)
                .Include(q => q.Quotationdetails)
                .ThenInclude(qd => qd.Product)
                .FirstOrDefaultAsync(q => q.QuotationId == id && q.UserId == userId.Value);

            if (quotation == null)
            {
                return NotFound();
            }

            var viewModel = new QuotationViewModel
            {
                Quotation = quotation,
                QuotationDetails = quotation.Quotationdetails.ToList()
            };

            return View(viewModel);
        }

        // GET: Quotations/Create
        public IActionResult Create()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var viewModel = new QuotationViewModel
            {
                Quotation = new Quotation
                {
                    QuotationDate = DateTime.Now.Date,
                    ExpiryDate = DateTime.Now.AddDays(30).Date,
                    UserId = userId.Value // Set UserId from session
                },
                QuotationDetails = new List<Quotationdetail> { new Quotationdetail() } // Initialize with one empty product
            };
            // Populate dropdowns with entities filtered by UserId
            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId.Value),
                "BranchId",
                "BranchName"
            );
            ViewData["CustomerId"] = new SelectList(
                _context.Customers.Where(c => c.UserId == userId.Value),
                "CustomerId",
                "CustomerName"
            );
            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId.Value),
                "EmployeeId",
                "FullName"
            );
            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId.Value),
                "StorehouseId",
                "StorehouseName"
            );
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId.Value),
                "ProductId",
                "ProductName"
            );

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuotationViewModel viewModel)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure UserId is set from session, not form
            viewModel.Quotation.UserId = userId.Value;

            // Validate ProductIDs
            var productIds = viewModel.QuotationDetails.Select(d => d.ProductId).ToList();
            var products = await _context.Products
                .Where(p => p.UserId == userId.Value && productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => new { p.Price, VatRate = p.Vatrate });

            var invalidProductIds = productIds.Except(products.Keys).ToList();
            if (invalidProductIds.Any())
            {
                ModelState.AddModelError("", $"Invalid Product IDs: {string.Join(", ", invalidProductIds)}. Please select valid products associated with your account.");
            }

            if (ModelState.IsValid)
            {
                var quotation = viewModel.Quotation;
                quotation.CreatedAt = DateTime.Now;
                quotation.ItemsCount = viewModel.QuotationDetails.Count;

                // Calculate totals
                decimal subtotal = 0;
                decimal totalVat = 0;

                foreach (var detail in viewModel.QuotationDetails)
                {
                    if (!products.TryGetValue(detail.ProductId, out var product))
                    {
                        ModelState.AddModelError("", $"Product ID {detail.ProductId} not found or not associated with your account.");
                        continue;
                    }

                    // Validate Quantity
                    if (detail.Quantity <= 0)
                    {
                        ModelState.AddModelError("", $"Quantity for Product ID {detail.ProductId} must be greater than zero.");
                        continue;
                    }

                    // Calculate LineTotal as Price * Quantity
                    decimal lineTotal = product.Price * detail.Quantity;
                    detail.LineTotal = lineTotal;

                    // Apply discount to LineTotal
                    decimal adjustedLineTotal = detail.DiscountType == "Percentage"
                        ? lineTotal * (1 - detail.Discount / 100)
                        : lineTotal - detail.Discount;

                    // Ensure adjusted LineTotal is non-negative
                    adjustedLineTotal = Math.Max(adjustedLineTotal, 0);

                    // Calculate UnitPrice as adjustedLineTotal / Quantity (after discount)
                    decimal calculatedUnitPrice = detail.Quantity > 0 ? adjustedLineTotal / detail.Quantity : 0;
                    detail.UnitPrice = calculatedUnitPrice; // Override form-submitted UnitPrice with discounted unit price

                    // Calculate VAT for the line (for QuotationDetails.VatAmount) after discount
                    decimal vatRate = product.VatRate ?? 0;
                    decimal vatAmount = adjustedLineTotal * (vatRate / 100);
                    detail.VatAmount = vatAmount;
                    detail.TotalWithVat = adjustedLineTotal + vatAmount;

                    // Store adjusted LineTotal for Subtotal calculation
                    subtotal += adjustedLineTotal;
                    totalVat += vatAmount;
                }

                quotation.Subtotal = subtotal;
                quotation.Vatamount = totalVat;
                quotation.TotalAmount = subtotal + totalVat;

                // Begin transaction to ensure data consistency
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Add quotation to context
                    _context.Add(quotation);
                    await _context.SaveChangesAsync();

                    // Add quotation details
                    foreach (var detail in viewModel.QuotationDetails)
                    {
                        detail.QuotationId = quotation.QuotationId;
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"An error occurred while saving the quotation: {ex.Message}");
                }
            }

            // Repopulate ViewData for dropdowns
            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId.Value),
                "BranchId",
                "BranchName",
                viewModel.Quotation.BranchId
            );
            ViewData["CustomerId"] = new SelectList(
                _context.Customers.Where(c => c.UserId == userId.Value),
                "CustomerId",
                "CustomerName",
                viewModel.Quotation.CustomerId
            );
            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId.Value),
                "EmployeeId",
                "FullName",
                viewModel.Quotation.EmployeeId
            );
            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId.Value),
                "StorehouseId",
                "StorehouseName",
                viewModel.Quotation.StorehouseId
            );
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId.Value),
                "ProductId",
                "ProductName"
            );

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new { productId = p.ProductId, price = p.Price, vatrate = p.Vatrate })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Json(product);
        }
        // GET: Quotations/Edit/5
        public IActionResult Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null)
            {
                return NotFound();
            }

            var quotation = _context.Quotations
                .Include(q => q.Quotationdetails)
                .ThenInclude(qd => qd.Product)
                .FirstOrDefault(q => q.QuotationId == id && q.UserId == userId.Value);

            if (quotation == null)
            {
                return NotFound();
            }

            var viewModel = new QuotationViewModel
            {
                Quotation = quotation,
                QuotationDetails = quotation.Quotationdetails.ToList()
            };

            if (!viewModel.QuotationDetails.Any())
            {
                viewModel.QuotationDetails.Add(new Quotationdetail());
            }

            RepopulateViewData(userId.Value, quotation);
            return View(viewModel);
        }

        // POST: Quotations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuotationViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != viewModel.Quotation.QuotationId)
            {
                return NotFound();
            }

            viewModel.Quotation.UserId = userId.Value;

            // Validate ProductIDs
            var productIds = viewModel.QuotationDetails.Select(d => d.ProductId).ToList();
            var products = await _context.Products
                .Where(p => p.UserId == userId.Value && productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => new { p.Price, VatRate = p.Vatrate });

            var invalidProductIds = productIds.Except(products.Keys).ToList();
            if (invalidProductIds.Any())
            {
                ModelState.AddModelError("", $"Invalid Product IDs: {string.Join(", ", invalidProductIds)}. Please select valid products associated with your account.");
            }

            // Check for duplicate products
            var duplicateProductIds = productIds
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (duplicateProductIds.Any())
            {
                ModelState.AddModelError("", $"Duplicate Product IDs: {string.Join(", ", duplicateProductIds)}. Each product can only be selected once.");
            }

            if (ModelState.IsValid)
            {
                var quotation = viewModel.Quotation;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Update quotation
                    _context.Update(quotation);
                    await _context.SaveChangesAsync();

                    // Update quotation details
                    await UpdateQuotationDetails(quotation.QuotationId, viewModel.QuotationDetails);

                    // Recalculate totals
                    await RecalculateQuotationTotals(quotation);

                    _context.Update(quotation);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"An error occurred while updating the quotation: {ex.Message}");
                }
            }

            await RepopulateViewData(userId.Value, viewModel.Quotation);
            return View(viewModel);
        }

        //// GET: Quotations/GetProductDetails
        //[HttpGet]
        //public IActionResult GetProductDetails(int productId)
        //{
        //    var userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId == null)
        //    {
        //        return Unauthorized();
        //    }

        //    var product = _context.Products
        //        .Where(p => p.ProductId == productId && p.UserId == userId.Value)
        //        .Select(p => new
        //        {
        //            productId = p.ProductId,
        //            price = p.Price,
        //            vatrate = p.Vatrate ?? 0
        //        })
        //        .FirstOrDefault();

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return Json(product);
        //}

        private async Task UpdateQuotationDetails(int quotationId, List<Quotationdetail> updatedDetails)
        {
            var existingDetails = await _context.Quotationdetails
                .Where(d => d.QuotationId == quotationId)
                .ToListAsync();

            foreach (var existingDetail in existingDetails)
            {
                if (!updatedDetails.Any(d => d.DetailId == existingDetail.DetailId))
                {
                    _context.Quotationdetails.Remove(existingDetail);
                }
            }

            foreach (var detail in updatedDetails)
            {
                var product = await _context.Products
                    .Where(p => p.ProductId == detail.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    continue;
                }

                detail.UnitPrice = product.Price;
                decimal adjustedUnitPrice = detail.DiscountType == "Percentage"
                    ? detail.UnitPrice * (1 - detail.Discount / 100)
                    : detail.UnitPrice - detail.Discount;

                adjustedUnitPrice = Math.Max(adjustedUnitPrice, 0);
                decimal lineTotal = detail.Quantity * adjustedUnitPrice;
                decimal vatRate = product.Vatrate ?? 0;
                decimal vatAmount = lineTotal * (vatRate / 100);

                detail.VatAmount = vatAmount;
                detail.LineTotal = lineTotal;
                detail.TotalWithVat = lineTotal + vatAmount;

                if (detail.DetailId == 0)
                {
                    detail.QuotationId = quotationId;
                    _context.Quotationdetails.Add(detail);
                }
                else
                {
                    var existingDetail = existingDetails.FirstOrDefault(d => d.DetailId == detail.DetailId);
                    if (existingDetail != null)
                    {
                        _context.Entry(existingDetail).CurrentValues.SetValues(detail);
                    }
                }
            }
        }

        private async Task RecalculateQuotationTotals(Quotation quotation)
        {
            var details = await _context.Quotationdetails
                .Include(d => d.Product)
                .Where(d => d.QuotationId == quotation.QuotationId)
                .ToListAsync();

            decimal subtotal = 0;
            decimal totalVat = 0;

            foreach (var detail in details)
            {
                decimal adjustedUnitPrice = detail.DiscountType == "Percentage"
                    ? detail.UnitPrice * (1 - detail.Discount / 100)
                    : detail.UnitPrice - detail.Discount;

                adjustedUnitPrice = Math.Max(adjustedUnitPrice, 0);
                decimal lineTotal = detail.Quantity * adjustedUnitPrice;
                decimal vatRate = detail.Product?.Vatrate ?? 0;
                decimal vatAmount = lineTotal * (vatRate / 100);

                detail.VatAmount = vatAmount;
                detail.LineTotal = lineTotal;
                detail.TotalWithVat = lineTotal + vatAmount;

                subtotal += lineTotal;
                totalVat += vatAmount;
            }

            quotation.Subtotal = subtotal;
            quotation.Vatamount = totalVat;
            quotation.TotalAmount = subtotal + totalVat;
            quotation.ItemsCount = details.Count;
        }

        private async Task RepopulateViewData(int userId, Quotation quotation)
        {
            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId),
                "BranchId", "BranchName", quotation.BranchId);

            ViewData["CustomerId"] = new SelectList(
                _context.Customers.Where(c => c.UserId == userId),
                "CustomerId", "CustomerName", quotation.CustomerId);

            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId),
                "EmployeeId", "FullName", quotation.EmployeeId);

            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId),
                "StorehouseId", "StorehouseName", quotation.StorehouseId);

            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId),
                "ProductId", "ProductName");
        }
    
    // GET: Quotations/Delete/5
    public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Quotations == null)
            {
                return NotFound();
            }

            var quotation = await _context.Quotations
                .Include(q => q.Branch)
                .Include(q => q.Customer)
                .Include(q => q.Employee)
                .Include(q => q.Invoice)
                .Include(q => q.Storehouse)
                .Include(q => q.User)
                .FirstOrDefaultAsync(m => m.QuotationId == id);
            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        // POST: Quotations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Quotations == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Quotations'  is null.");
            }
            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation != null)
            {
                _context.Quotations.Remove(quotation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuotationExists(int id)
        {
          return (_context.Quotations?.Any(e => e.QuotationId == id)).GetValueOrDefault();
        }




























        // GET: Invoices/ConvertToInvoice/5
        public async Task<IActionResult> ConvertToInvoice(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null)
            {
                return NotFound();
            }

            var quotation = await _context.Quotations
                .Where(q => q.QuotationId == id && q.UserId == userId && q.Status == "غير مفوتر")
                .Include(q => q.Customer)
                .Include(q => q.Employee)
                .Include(q => q.Branch)
                .Include(q => q.Storehouse)
                .Include(q => q.Quotationdetails)
                    .ThenInclude(qd => qd.Product)
                .FirstOrDefaultAsync();

            if (quotation == null)
            {
                return NotFound("عرض السعر غير موجود أو غير صالح للتحويل.");
            }

            var viewModel = new ConvertToInvoiceViewModel
            {
                QuotationId = quotation.QuotationId,
                CustomerId = quotation.CustomerId ?? 0,
                CustomerName = quotation.Customer?.CustomerName ?? "غير محدد",
                EmployeeId = quotation.EmployeeId,
                EmployeeName = quotation.Employee?.FullName ?? "غير محدد",
                BranchId = quotation.BranchId,
                BranchName = quotation.Branch?.BranchName ?? "غير محدد",
                StorehouseId = quotation.StorehouseId,
                StorehouseName = quotation.Storehouse?.StorehouseName ?? "غير محدد",
                QuotationDate = quotation.QuotationDate,
                Subtotal = quotation.Subtotal,
                VatAmount = quotation.Vatamount ?? 0m,
                TotalAmount = quotation.TotalAmount ?? 0m,
                Details = quotation.Quotationdetails.Select(qd => new QuotationDetailViewModel
                {
                    ProductId = qd.ProductId,
                    ProductName = qd.Product?.ProductName ?? "غير محدد",
                    Quantity = qd.Quantity,
                    UnitPrice = qd.UnitPrice,
                    VatAmount = qd.VatAmount ?? 0m,
                    LineTotal = (qd.Quantity * qd.UnitPrice) + (qd.VatAmount ?? 0m), // Total after discount plus VAT
                    ProductDescription = qd.ProductDescription,
                    DiscountType = qd.DiscountType.ToString(),
                    Discount = qd.Discount
                }).ToList()
            };

            ViewBag.CashAccounts = _context.Cashandbankaccounts
                .Where(a => a.UserId == userId && a.AccountType == "خزينة" && a.Status == "نشط")
                .Select(a => new { a.AccountId, a.AccountName })
                .ToList();
            ViewBag.BankAccounts = _context.Cashandbankaccounts
                .Where(a => a.UserId == userId && a.AccountType == "حساب بنكي" && a.Status == "نشط")
                .Select(a => new { a.AccountId, a.AccountName })
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToInvoice(ConvertToInvoiceViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً." });
            }
            int userIdValue = userId.Value;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "بيانات غير صالحة.", errors });
            }

            if (viewModel.PaymentEntries == null || !viewModel.PaymentEntries.Any())
            {
                return Json(new { success = false, message = "يجب إدخال طريقة دفع واحدة على الأقل." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Fetch the quotation
                var quotation = await _context.Quotations
                    .Where(q => q.QuotationId == viewModel.QuotationId && q.UserId == userIdValue && q.Status == "غير مفوتر")
                    .Include(q => q.Quotationdetails)
                    .FirstOrDefaultAsync();

                if (quotation == null)
                {
                    return Json(new { success = false, message = "عرض السعر غير موجود أو غير صالح للتحويل." });
                }

                // Validate Quotationdetails for invalid values
                if (quotation.Quotationdetails.Any(qd => qd.Quantity <= 0 || qd.UnitPrice <= 0 || ((qd.Quantity * qd.UnitPrice) + (qd.VatAmount ?? 0m)) <= 0))
                {
                    return Json(new { success = false, message = "تفاصيل عرض السعر تحتوي على بيانات غير صالحة (كمية، سعر الوحدة، أو الإجمالي)." });
                }

                // Map EmployeeId from employees to UserId from users
                int? invoiceEmployeeId = null;
                if (quotation.EmployeeId.HasValue)
                {
                    var employee = await _context.Employees
                        .Where(e => e.EmployeeId == quotation.EmployeeId && e.UserId == userIdValue)
                        .FirstOrDefaultAsync();
                    if (employee == null)
                    {
                        return Json(new { success = false, message = "الموظف المحدد غير موجود في النظام." });
                    }
                    invoiceEmployeeId = employee.UserId; // Use the UserId associated with the employee
                }

                // Create new invoice
                var invoice = new Invoice
                {
                    UserId = userIdValue,
                    CustomerId = quotation.CustomerId,
                    EmployeeId = invoiceEmployeeId,
                    BranchId = quotation.BranchId,
                    StorehouseId = quotation.StorehouseId,
                    InvoiceDate = DateTime.Now,
                    InvoiceType = "بيع",
                    CreatedAt = DateTime.Now,
                    Status = "مدفوعة",
                    Vatrate = quotation.Vatrate,
                    Subtotal = quotation.Subtotal,
                    ItemsCount = quotation.ItemsCount
                };

                // Map quotation details to invoice details
                var invoicedetails = quotation.Quotationdetails.Select(qd => new Invoicedetail
                {
                    ProductId = qd.ProductId,
                    Quantity = qd.Quantity,
                    UnitPrice = qd.UnitPrice, // Reflects price after discount
                    VatAmount = qd.VatAmount.GetValueOrDefault(0m),
                    LineTotal = (qd.Quantity * qd.UnitPrice) + qd.VatAmount.GetValueOrDefault(0m), // Total after discount plus VAT
                    InvoiceTyping = "بيع"
                }).ToList();

                // Validate invoice details and product balances
                var productCosts = new Dictionary<int, decimal>();
                foreach (var detail in invoicedetails)
                {
                    var product = await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId && p.UserId == userIdValue);

                    if (product == null)
                    {
                        return Json(new { success = false, message = $"المنتج برقم {detail.ProductId} غير موجود أو لا يخصك." });
                    }

                    if (detail.Quantity > product.Balance)
                    {
                        return Json(new { success = false, message = $"الكمية المطلوبة ({detail.Quantity}) تتجاوز الرصيد المتاح ({product.Balance}) للمنتج {product.ProductName}." });
                    }

                    if (product.PurchasePrice == null)
                    {
                        return Json(new { success = false, message = $"سعر الشراء للمنتج {product.ProductName} غير محدد." });
                    }

                    productCosts[(int)detail.ProductId] = product.PurchasePrice;
                }

                // Save invoice
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Process invoice details
                foreach (var detail in invoicedetails)
                {
                    detail.InvoiceId = invoice.InvoiceId;
                    _context.Invoicedetails.Add(detail);

                    // Insert inventory record
                    var inventory = new Inventory
                    {
                        ProductId = detail.ProductId,
                        OutgoingQuantity = detail.Quantity,
                        IncomingQuantity = 0,
                        LastUpdated = DateTime.Now,
                        StorehouseId = invoice.StorehouseId,
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.InvoiceId
                    };
                    _context.Inventories.Add(inventory);

                    // Update product balance
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    product.Balance -= detail.Quantity;
                    _context.Products.Update(product);
                }

                // Calculate invoice totals (should match quotation)
                invoice.Vatamount = invoicedetails.Sum(d => d.VatAmount);
                invoice.TotalAmount = invoice.Subtotal + invoice.Vatamount;
                _context.Invoices.Update(invoice);

                // Validate payment entries
                var totalPaymentAmount = viewModel.PaymentEntries.Sum(p => p.Amount);
                if (totalPaymentAmount != invoice.TotalAmount)
                {
                    return Json(new { success = false, message = $"إجمالي مبالغ الدفع ({totalPaymentAmount}) لا يساوي إجمالي الفاتورة ({invoice.TotalAmount})." });
                }

                foreach (var entry in viewModel.PaymentEntries)
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
                }

                // Assign cash and bank account IDs
                var firstCashEntry = viewModel.PaymentEntries.FirstOrDefault(p => p.PaymentMethod == "كاش" && p.AccountId.HasValue);
                var firstBankEntry = viewModel.PaymentEntries.FirstOrDefault(p => p.PaymentMethod == "تحويل بنكي" && p.AccountId.HasValue);
                invoice.InvoiceCashAccountId = firstCashEntry?.AccountId;
                invoice.InvoiceBankAccountId = firstBankEntry?.AccountId;
                _context.Invoices.Update(invoice);

                // Create payment record
                var payment = new Payment
                {
                    CustomerId = invoice.CustomerId,
                    UserId = userIdValue,
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
                foreach (var entry in viewModel.PaymentEntries)
                {
                    var paymentDetail = new Paymentdetail
                    {
                        Payid = payment.Payid,
                        UserId = userIdValue,
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

                // Update cash and bank account balances
                if (invoice.InvoiceCashAccountId.HasValue)
                {
                    var cashAccount = await _context.Cashandbankaccounts.FindAsync(invoice.InvoiceCashAccountId.Value);
                    if (cashAccount == null)
                    {
                        return Json(new { success = false, message = $"الحساب النقدي برقم {invoice.InvoiceCashAccountId.Value} غير موجود." });
                    }
                    if (cashAccount.ChildAccountId == null && cashAccount.AccountType == "خزينة")
                    {
                        cashAccount.ChildAccountId = (await _context.ChildAccounts.FirstOrDefaultAsync(c => c.Name == "النقدية بالصندوق" && c.UserId == userIdValue))?.Id;
                    }
                    cashAccount.Balance += viewModel.PaymentEntries.Where(p => p.PaymentMethod == "كاش").Sum(p => p.Amount);
                    _context.Cashandbankaccounts.Update(cashAccount);
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
                        bankAccount.ChildAccountId = (await _context.ChildAccounts.FirstOrDefaultAsync(c => c.Name == "النقدية بالبنك" && c.UserId == userIdValue))?.Id;
                    }
                    bankAccount.Balance += viewModel.PaymentEntries.Where(p => p.PaymentMethod == "تحويل بنكي").Sum(p => p.Amount);
                    _context.Cashandbankaccounts.Update(bankAccount);
                }

                // Fetch account mappings for journal entries
                var accountMappings = await _context.AccountMappings
                    .Where(m => new[]
                    {
                    "إيرادات المبيعات",
                    "ضريبة القيمة المضافة المستحقة",
                    "الحسابات المستحقة القبض",
                    "النقدية بالصندوق",
                    "النقدية بالبنك",
                    "المخزون",
                    "تكلفة البضاعة المباعة"
                    }.Contains(m.TransactionType))
                    .ToDictionaryAsync(m => m.TransactionType, m => m.ChildAccountId);

                var requiredAccounts = new[]
                {
                "إيرادات المبيعات",
                "المخزون",
                "تكلفة البضاعة المباعة"
            };

                foreach (var account in requiredAccounts)
                {
                    if (!accountMappings.ContainsKey(account))
                    {
                        return Json(new { success = false, message = $"حساب '{account}' غير معرف في تعيينات الحسابات." });
                    }
                }

                // Create journal entries
                var journalEntries = new List<JournalEntry>();

                // Sales revenue (credit)
                journalEntries.Add(new JournalEntry
                {
                    EntryDate = DateOnly.FromDateTime(DateTime.Now),
                    Description = $"فاتورة مبيعات رقم {invoice.InvoiceId}",
                    DebitAmount = 0,
                    CreditAmount = invoice.Subtotal,
                    UserId = userIdValue,
                    CostCenterId = 0,
                    CreatedAt = DateTime.Now,
                    InvoiceId = invoice.InvoiceId,
                    InvoiceType = "بيع",
                    ChildAccountId = accountMappings["إيرادات المبيعات"]
                });

                // VAT per product (credit)
                foreach (var detail in invoicedetails)
                {
                    if (detail.VatAmount > 0)
                    {
                        if (!accountMappings.ContainsKey("ضريبة القيمة المضافة المستحقة"))
                        {
                            return Json(new { success = false, message = "حساب ضريبة القيمة المضافة المستحقة غير معرف." });
                        }
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"ضريبة مبيعات - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                            DebitAmount = 0,
                            CreditAmount = detail.VatAmount??0,
                            UserId = userIdValue,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = accountMappings["ضريبة القيمة المضافة المستحقة"]
                        });
                    }
                }

                // Inventory and COGS
                decimal totalCogs = 0;
                foreach (var detail in invoicedetails)
                {
                    decimal cogsAmount = detail.Quantity * productCosts[(int)detail.ProductId];

                    totalCogs += cogsAmount;

                    // Debit COGS
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تكلفة البضاعة المباعة - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                        DebitAmount = cogsAmount,
                        CreditAmount = 0,
                        UserId = userIdValue,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "بيع",
                        ChildAccountId = accountMappings["تكلفة البضاعة المباعة"]
                    });

                    // Credit Inventory
                    journalEntries.Add(new JournalEntry
                    {
                        EntryDate = DateOnly.FromDateTime(DateTime.Now),
                        Description = $"تخفيض المخزون - فاتورة رقم {invoice.InvoiceId}, منتج {detail.ProductId}",
                        DebitAmount = 0,
                        CreditAmount = cogsAmount,
                        UserId = userIdValue,
                        CostCenterId = 0,
                        CreatedAt = DateTime.Now,
                        InvoiceId = invoice.InvoiceId,
                        InvoiceType = "بيع",
                        ChildAccountId = accountMappings["المخزون"]
                    });
                }

                // Payment entries (debit)
                foreach (var entry in viewModel.PaymentEntries)
                {
                    int? childAccountId = null;
                    if (entry.PaymentMethod == "كاش")
                    {
                        if (!accountMappings.ContainsKey("النقدية بالصندوق"))
                        {
                            return Json(new { success = false, message = "حساب النقدية بالصندوق غير معرف." });
                        }
                        childAccountId = accountMappings["النقدية بالصندوق"];
                    }
                    else if (entry.PaymentMethod == "تحويل بنكي")
                    {
                        if (!accountMappings.ContainsKey("النقدية بالبنك"))
                        {
                            return Json(new { success = false, message = "حساب النقدية بالبنك غير معرف." });
                        }
                        childAccountId = accountMappings["النقدية بالبنك"];
                    }
                    else if (entry.PaymentMethod == "آجل")
                    {
                        if (!accountMappings.ContainsKey("الحسابات المستحقة القبض"))
                        {
                            return Json(new { success = false, message = "حساب الحسابات المستحقة القبض غير معرف." });
                        }
                        childAccountId = accountMappings["الحسابات المستحقة القبض"];
                    }

                    if (childAccountId.HasValue)
                    {
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"دفع {entry.PaymentMethod} - فاتورة رقم {invoice.InvoiceId}",
                            DebitAmount = entry.Amount,
                            CreditAmount = 0,
                            UserId = userIdValue,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = invoice.InvoiceId,
                            InvoiceType = "بيع",
                            ChildAccountId = childAccountId.Value
                        });
                    }
                }

                _context.JournalEntries.AddRange(journalEntries);

                // Update quotation status
                quotation.Status = "مفوتر";
                quotation.InvoiceId = invoice.InvoiceId;
                _context.Quotations.Update(quotation);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "تم تحويل عرض السعر إلى فاتورة بنجاح!", redirectUrl = Url.Action("IndexSales", "Invoices") });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"DbUpdateException: {ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.",
                    errorDetails = new { ex.Message, InnerMessage = ex.InnerException?.Message }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Exception: {ex.Message}\n{ex.InnerException?.Message}\n{ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ غير متوقع أثناء الحفظ.",
                    errorDetails = new { ex.Message, InnerMessage = ex.InnerException?.Message }
                });
            }
        }
    }
}
