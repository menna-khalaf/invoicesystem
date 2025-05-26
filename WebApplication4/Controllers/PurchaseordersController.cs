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
    public class PurchaseordersController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public PurchaseordersController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Purchaseorders
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.Purchaseorders.Include(p => p.Branch).Include(p => p.Employee).Include(p => p.Storehouse).Include(p => p.User).Include(p => p.Vendor);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Purchaseorders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Purchaseorders == null)
            {
                return NotFound();
            }

            var purchaseorder = await _context.Purchaseorders
                .Include(p => p.Branch)
                .Include(p => p.Employee)
                .Include(p => p.Storehouse)
                .Include(p => p.User)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PurchaseOrderId == id);
            if (purchaseorder == null)
            {
                return NotFound();
            }

            return View(purchaseorder);
        }

        // GET: Purchaseorders/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new PurchaseOrderViewModel
            {
                Purchaseorder = new Purchaseorder
                {
                    PurchaseOrderDate = DateOnly.FromDateTime(DateTime.Now),
                    ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
                    Status = "Pending",
                    UserId = userId.Value,
                    CreatedAt = DateTime.Now
                },
                Purchaseorderdetails = new List<Purchaseorderdetail> { new Purchaseorderdetail { DiscountType = "Amount" } }
            };

            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId.Value),
                "BranchId",
                "BranchName"
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
            ViewData["VendorId"] = new SelectList(
                _context.Vendors.Where(v => v.UserId == userId.Value),
                "VendorId",
                "VendorName"
            );
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId.Value),
                "ProductId",
                "ProductName"
            );

            return View(viewModel);
        }

        // POST: Purchaseorders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            viewModel.Purchaseorder.UserId = userId.Value;

            var productIds = viewModel.Purchaseorderdetails.Select(d => d.ProductId).ToList();
            var products = await _context.Products
                .Where(p => p.UserId == userId.Value && productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId, p => new { p.PurchasePrice, p.Vatrate });

            var invalidProductIds = productIds.Except(products.Keys).ToList();
            if (invalidProductIds.Any())
            {
                ModelState.AddModelError("", $"Invalid Product IDs: {string.Join(", ", invalidProductIds)}.");
            }

            if (!viewModel.Purchaseorderdetails.Any(d => d.ProductId > 0 && d.Quantity > 0))
            {
                ModelState.AddModelError("", "At least one valid product with quantity is required.");
            }

            if (ModelState.IsValid)
            {
                var purchaseorder = viewModel.Purchaseorder;
                purchaseorder.CreatedAt = DateTime.Now;
                purchaseorder.ItemsCount = viewModel.Purchaseorderdetails.Count(d => d.ProductId > 0);

                decimal subtotal = 0;
                decimal totalVat = 0;

                foreach (var detail in viewModel.Purchaseorderdetails.Where(d => d.ProductId > 0))
                {
                    if (!products.TryGetValue(detail.ProductId, out var product))
                    {
                        ModelState.AddModelError("", $"Product ID {detail.ProductId} not found.");
                        continue;
                    }

                    detail.UnitPrice = product.PurchasePrice  ;
                    decimal lineTotal = detail.Quantity * detail.UnitPrice;
                    decimal discountAmount = detail.DiscountType == "Percentage"
                        ? lineTotal * (detail.Discount / 100)
                        : detail.Discount;
                    lineTotal -= discountAmount;
                    decimal vatRate = product.Vatrate ?? 0;
                    decimal vatAmount = lineTotal * (vatRate / 100);
                    detail.VatAmount = vatAmount;
                    detail.LineTotal = lineTotal;

                    subtotal += lineTotal;
                    totalVat += vatAmount;
                }

                purchaseorder.Subtotal = subtotal;
                purchaseorder.Vatamount = totalVat;
                purchaseorder.TotalAmount = subtotal + totalVat;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Add(purchaseorder);
                    await _context.SaveChangesAsync();

                    foreach (var detail in viewModel.Purchaseorderdetails.Where(d => d.ProductId > 0))
                    {
                        detail.PurchaseOrderId = purchaseorder.PurchaseOrderId;
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Error saving purchase order: {ex.Message}");
                }
            }

            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId.Value),
                "BranchId",
                "BranchName",
                viewModel.Purchaseorder.BranchId
            );
            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId.Value),
                "EmployeeId",
                "FullName",
                viewModel.Purchaseorder.EmployeeId
            );
            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId.Value),
                "StorehouseId",
                "StorehouseName",
                viewModel.Purchaseorder.StorehouseId
            );
            ViewData["VendorId"] = new SelectList(
                _context.Vendors.Where(v => v.UserId == userId.Value),
                "VendorId",
                "PhoneNumber1",
                viewModel.Purchaseorder.VendorId
            );
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId.Value),
                "ProductId",
                "ProductName"
            );

            return View(viewModel);
        }

        // GET: GetProductDetails for AJAX
        [HttpGet]
        public IActionResult GetProductDetails(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { Success = false, Message = "User not logged in" });
            }

            var product = _context.Products
                .Where(p => p.UserId == userId.Value && p.ProductId == productId)
                .Select(p => new { p.ProductId, p.ProductName, p.PurchasePrice, Vatrate = p.Vatrate ?? 0 })
                .FirstOrDefault();

            if (product == null)
            {
                return Json(new { ProductId = 0, ProductName = "", PurchasePrice = 0m, Vatrate = 0m });
            }

            return Json(product);
        }
        // GET: Purchaseorders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Purchaseorders == null)
            {
                return NotFound();
            }

            var purchaseorder = await _context.Purchaseorders.FindAsync(id);
            if (purchaseorder == null)
            {
                return NotFound();
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", purchaseorder.BranchId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", purchaseorder.EmployeeId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", purchaseorder.StorehouseId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", purchaseorder.UserId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "PhoneNumber1", purchaseorder.VendorId);
            return View(purchaseorder);
        }

        // POST: Purchaseorders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PurchaseOrderId,VendorId,EmployeeId,PurchaseOrderDate,ExpiryDate,Status,Subtotal,Vatrate,Vatamount,TotalAmount,StorehouseId,ItemsCount,BranchId,UserId,CreatedAt")] Purchaseorder purchaseorder)
        {
            if (id != purchaseorder.PurchaseOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseorder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseorderExists(purchaseorder.PurchaseOrderId))
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
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", purchaseorder.BranchId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", purchaseorder.EmployeeId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", purchaseorder.StorehouseId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", purchaseorder.UserId);
            ViewData["VendorId"] = new SelectList(_context.Vendors, "VendorId", "PhoneNumber1", purchaseorder.VendorId);
            return View(purchaseorder);
        }

        // GET: Purchaseorders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Purchaseorders == null)
            {
                return NotFound();
            }

            var purchaseorder = await _context.Purchaseorders
                .Include(p => p.Branch)
                .Include(p => p.Employee)
                .Include(p => p.Storehouse)
                .Include(p => p.User)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PurchaseOrderId == id);
            if (purchaseorder == null)
            {
                return NotFound();
            }

            return View(purchaseorder);
        }

        // POST: Purchaseorders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Purchaseorders == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Purchaseorders'  is null.");
            }
            var purchaseorder = await _context.Purchaseorders.FindAsync(id);
            if (purchaseorder != null)
            {
                _context.Purchaseorders.Remove(purchaseorder);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseorderExists(int id)
        {
          return (_context.Purchaseorders?.Any(e => e.PurchaseOrderId == id)).GetValueOrDefault();
        }
    }
}
