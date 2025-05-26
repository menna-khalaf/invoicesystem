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
    public class AdjustmentproductsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public AdjustmentproductsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Adjustmentproducts
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.Adjustmentproducts.Include(a => a.Adjustment).Include(a => a.Inventory).Include(a => a.Product);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Adjustmentproducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Adjustmentproducts == null)
            {
                return NotFound();
            }

            var adjustmentproduct = await _context.Adjustmentproducts
                .Include(a => a.Adjustment)
                .Include(a => a.Inventory)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(m => m.AdjustmentProductId == id);
            if (adjustmentproduct == null)
            {
                return NotFound();
            }

            return View(adjustmentproduct);
        }

        // GET: Adjustmentproducts/Create
        public IActionResult Create()
        {
            ViewData["AdjustmentId"] = new SelectList(_context.Adjustments, "AdjustmentId", "AdjustmentId");
            ViewData["InventoryId"] = new SelectList(_context.Inventories, "InventoryId", "InventoryId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: Adjustmentproducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdjustmentProductId,AdjustmentId,ProductId,ActualQuantity,Difference,Balanced,InventoryId")] Adjustmentproduct adjustmentproduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(adjustmentproduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AdjustmentId"] = new SelectList(_context.Adjustments, "AdjustmentId", "AdjustmentId", adjustmentproduct.AdjustmentId);
            ViewData["InventoryId"] = new SelectList(_context.Inventories, "InventoryId", "InventoryId", adjustmentproduct.InventoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", adjustmentproduct.ProductId);
            return View(adjustmentproduct);
        }

        // GET: Adjustmentproducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Adjustmentproducts == null)
            {
                return NotFound();
            }

            var adjustmentproduct = await _context.Adjustmentproducts.FindAsync(id);
            if (adjustmentproduct == null)
            {
                return NotFound();
            }
            ViewData["AdjustmentId"] = new SelectList(_context.Adjustments, "AdjustmentId", "AdjustmentId", adjustmentproduct.AdjustmentId);
            ViewData["InventoryId"] = new SelectList(_context.Inventories, "InventoryId", "InventoryId", adjustmentproduct.InventoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", adjustmentproduct.ProductId);
            return View(adjustmentproduct);
        }

        // POST: Adjustmentproducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdjustmentProductId,AdjustmentId,ProductId,ActualQuantity,Difference,Balanced,InventoryId")] Adjustmentproduct adjustmentproduct)
        {
            if (id != adjustmentproduct.AdjustmentProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adjustmentproduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdjustmentproductExists(adjustmentproduct.AdjustmentProductId))
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
            ViewData["AdjustmentId"] = new SelectList(_context.Adjustments, "AdjustmentId", "AdjustmentId", adjustmentproduct.AdjustmentId);
            ViewData["InventoryId"] = new SelectList(_context.Inventories, "InventoryId", "InventoryId", adjustmentproduct.InventoryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", adjustmentproduct.ProductId);
            return View(adjustmentproduct);
        }

        // GET: Adjustmentproducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Adjustmentproducts == null)
            {
                return NotFound();
            }

            var adjustmentproduct = await _context.Adjustmentproducts
                .Include(a => a.Adjustment)
                .Include(a => a.Inventory)
                .Include(a => a.Product)
                .FirstOrDefaultAsync(m => m.AdjustmentProductId == id);
            if (adjustmentproduct == null)
            {
                return NotFound();
            }

            return View(adjustmentproduct);
        }

        // POST: Adjustmentproducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Adjustmentproducts == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Adjustmentproducts'  is null.");
            }
            var adjustmentproduct = await _context.Adjustmentproducts.FindAsync(id);
            if (adjustmentproduct != null)
            {
                _context.Adjustmentproducts.Remove(adjustmentproduct);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdjustmentproductExists(int id)
        {
          return (_context.Adjustmentproducts?.Any(e => e.AdjustmentProductId == id)).GetValueOrDefault();
        }








        public IActionResult CreateAdjustmentProduct()
        {
            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Retrieve AdjustmentId from session
            var adjustmentId = HttpContext.Session.GetInt32("AdjustmentId");
            if (adjustmentId == null)
            {
                // Handle missing AdjustmentId (e.g., redirect to create adjustment)
                return RedirectToAction("Create", "Adjustments");
            }

            // Pass AdjustmentId and filtered product data to the view
            ViewBag.AdjustmentId = adjustmentId.Value; // Ensure this is not null
            ViewData["ProductId"] = new SelectList(
                _context.Products.Where(p => p.UserId == userId.Value),
                "ProductId",
                "ProductName"
            );
            ViewBag.ProductBalances = _context.Products
                .Where(p => p.UserId == userId.Value)
                .ToDictionary(p => p.ProductId, p => p.Balance);

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdjustmentProduct([FromBody] List<Adjustmentproduct> adjustmentProducts)
        {
            if (adjustmentProducts == null || !adjustmentProducts.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            var adjustmentId = HttpContext.Session.GetInt32("AdjustmentId");

            if (adjustmentId == null)
            {
                return Json(new { success = false, message = "AdjustmentId is missing." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var adjProduct in adjustmentProducts)
                    {
                        // Validate required fields
                        if (adjProduct.ProductId == null || adjProduct.ActualQuantity == null)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Product ID and Actual Quantity are required."
                            });
                        }

                        // Fetch product and ensure it’s tracked
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == adjProduct.ProductId);

                        if (product == null)
                        {
                            return Json(new
                            {
                                success = false,
                                message = $"Product with ID {adjProduct.ProductId} not found."
                            });
                        }

                        // Calculate difference
                        adjProduct.Difference = adjProduct.ActualQuantity.Value - product.Balance;

                        // Set Balanced to the stored quantity (product.Balance) before updating
                        adjProduct.Balanced = product.Balance;

                        // Update product balance explicitly
                        product.Balance = adjProduct.ActualQuantity.Value;

                        // Ensure EF tracks the update
                        _context.Entry(product).State = EntityState.Modified;

                        // Set adjustment product properties
                        adjProduct.AdjustmentId = adjustmentId;
                        adjProduct.InventoryId = null; // Since we're not using inventory

                        _context.Adjustmentproducts.Add(adjProduct);
                    }

                    // Save changes and log for debugging
                    int changes = await _context.SaveChangesAsync();
                    if (changes == 0)
                    {
                        throw new Exception("No changes were saved to the database.");
                    }

                    await transaction.CommitAsync();

                    // Clear session to prevent reuse
                    HttpContext.Session.Remove("AdjustmentId");

                    return Json(new
                    {
                        success = true,
                        message = "تم تحديث الجرد بنجاح",
                        redirectUrl = Url.Action("Index", "Adjustments")
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    // Log the error for debugging (replace with your logging mechanism)
                    Console.WriteLine($"Error in CreateAdjustmentProduct: {ex.Message}");
                    return Json(new
                    {
                        success = false,
                        message = $"حدث خطأ أثناء التحديث: {ex.Message}"
                    });
                }
            }
        }






        // Existing actions...

        public async Task<IActionResult> AdjustmentProductself(int? id)
        {
            if (id == null)
            {
                return NotFound("رقم التسوية غير محدد.");
            }

            // Fetch data related to the AdjustmentId
            var adjustmentProducts = await _context.Adjustmentproducts
                .Where(d => d.AdjustmentId == id)
                .Include(d => d.Product) // Include Product for ProductName
                .ToListAsync();

            if (adjustmentProducts == null || !adjustmentProducts.Any())
            {
                return NotFound("لا توجد منتجات تسوية مرتبطة بهذا الرقم.");
            }

            return View(adjustmentProducts);
        }
    }
}
