using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ProductsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get logged-in user's ID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // User not logged in, redirect to login
                TempData["ErrorMessage"] = "Please log in to view your products";
                return RedirectToAction("Login", "usered");
            }

            // Get only products that belong to the logged-in user
            var products = await _context.Products
                .Where(p => p.UserId == userId.Value)
                .ToListAsync();

            return View(products);
        }
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Handle case where user is not logged in
                return RedirectToAction("Login", "usered");
            }

            // Fetch categories and storehouses for the logged-in user
            var categories = _context.ProductCategories
                .Where(c => c.UserId == userId.Value)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToList();

            var storehouses = _context.Storehouses
                .Where(s => s.UserId == userId.Value)
                .Select(s => new SelectListItem
                {
                    Value = s.StorehouseId.ToString(),
                    Text = s.StorehouseName
                })
                .ToList();

            // Pass data to the view
            ViewData["CategoryId"] = new SelectList(categories, "Value", "Text");
            ViewData["StorehouseId"] = new SelectList(storehouses, "Value", "Text");

            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? Image)
        {
            const long MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB limit

            // Get logged-in user's ID from session (declared once)
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // User not logged in, redirect to login
                return RedirectToAction("Login", "usered");
            }

            if (!ModelState.IsValid)
            {
                // Re-populate dropdowns in case of validation errors
                var categories = _context.ProductCategories
                    .Where(c => c.UserId == userId.Value)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    })
                    .ToList();

                var storehouses = _context.Storehouses
                    .Where(s => s.UserId == userId.Value)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StorehouseId.ToString(),
                        Text = s.StorehouseName
                    })
                    .ToList();

                ViewData["CategoryId"] = new SelectList(categories, "Value", "Text", model.CategoryId);
                ViewData["StorehouseId"] = new SelectList(storehouses, "Value", "Text", model.StorehouseId);
                return View(model);
            }

            // Assign the logged-in user to the product
            model.UserId = userId.Value;
            if (Image != null && Image.Length > 0)
            {
                if (Image.Length > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("Image", "حجم الصورة كبير جدًا! الحد الأقصى 2MB.");
                    // Re-populate dropdowns
                    var categories = _context.ProductCategories
                        .Where(c => c.UserId == userId.Value)
                        .Select(c => new SelectListItem
                        {
                            Value = c.CategoryId.ToString(),
                            Text = c.CategoryName
                        })
                        .ToList();

                    var storehouses = _context.Storehouses
                        .Where(s => s.UserId == userId.Value)
                        .Select(s => new SelectListItem
                        {
                            Value = s.StorehouseId.ToString(),
                            Text = s.StorehouseName
                        })
                        .ToList();

                    ViewData["CategoryId"] = new SelectList(categories, "Value", "Text", model.CategoryId);
                    ViewData["StorehouseId"] = new SelectList(storehouses, "Value", "Text", model.StorehouseId);
                    return View(model);
                }

                using var memoryStream = new MemoryStream();
                await Image.CopyToAsync(memoryStream);
                model.Image = memoryStream.ToArray();
            }

            // Set default values if not provided
            model.Vatrate = model.Vatrate > 0 ? model.Vatrate : 0;
            model.PurchasePrice = model.PurchasePrice > 0 ? model.PurchasePrice : 0;
            model.RepurchasePoint = model.RepurchasePoint > 0 ? model.RepurchasePoint : 0;

            // Save Product Data
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Check user session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "usered");
            }

            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || product.UserId != userId)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,Vatrate,CreatedAt,QRCode")] Product product, IFormFile? Image)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "usered");
            }

            if (id != product.ProductId)
            {
                return NotFound();
            }

            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
            if (existingProduct == null || existingProduct.UserId != userId)
            {
                return NotFound();
            }

            if (Image != null && Image.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await Image.CopyToAsync(memoryStream);
                product.Image = memoryStream.ToArray();
            }
            else
            {
                product.Image = existingProduct.Image;
            }

            product.UserId = userId.Value;
            product.CreatedAt = existingProduct.CreatedAt;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //private bool ProductExists(int id)
        //{
        //  return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        //}




        // Action to display products that need reordering
        public IActionResult ReorderReport()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Redirect to login if no session UserId
                return RedirectToAction("Login", "usered");
            }

            // Fetch products where Balance <= RepurchasePoint and UserId matches
            var reorderProducts = _context.Products
                .Where(p => p.UserId == userId.Value && p.Balance <= p.RepurchasePoint)
                .ToList();

            // Pass the data to the view
            return View(reorderProducts);
        }
    }
}
