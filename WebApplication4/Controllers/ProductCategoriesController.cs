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
    public class ProductCategoriesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ProductCategoriesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }


        // GET: ProductCategories
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var invoiceSystemrahtkContext = _context.ProductCategories
                .Include(p => p.User)
                .Where(p => p.UserId == userId.Value);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: ProductCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ProductCategories == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .Include(p => p.Parent)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }
        // GET: ProductCategories/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }
            return View();
        }

        // POST: ProductCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,DisplayOrder,IsActive")] ProductCategory productCategory, IFormFile? Icon)
        {
            const long MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB limit

            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (!ModelState.IsValid)
            {
                return View(productCategory);
            }

            // Handle image upload
            if (Icon != null && Icon.Length > 0)
            {
                if (Icon.Length > MAX_FILE_SIZE)
                {
                    ModelState.AddModelError("Icon", "حجم الصورة كبير جدًا! الحد الأقصى 2 ميجابايت.");
                    return View(productCategory);
                }

                if (!Icon.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("Icon", "يرجى تحميل ملف صورة صالح.");
                    return View(productCategory);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await Icon.CopyToAsync(memoryStream);
                    productCategory.Icon = memoryStream.ToArray(); // Store as byte[]
                }
            }

            productCategory.UserId = userId.Value;
            _context.Add(productCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductCategories == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory == null)
            {
                return NotFound();
            }
            ViewData["ParentId"] = new SelectList(_context.ProductCategories, "CategoryId", "CategoryName", productCategory.ParentId);
            ViewData["UserID"] = new SelectList(_context.Users, "UserId", "Email", productCategory.UserId);
            return View(productCategory);
        }

        // POST: ProductCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,ParentId,DisplayOrder,Icon,IsActive,UserID")] ProductCategory productCategory)
        {
            if (id != productCategory.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductCategoryExists(productCategory.CategoryId))
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
            ViewData["ParentId"] = new SelectList(_context.ProductCategories, "CategoryId", "CategoryName", productCategory.ParentId);
            ViewData["UserID"] = new SelectList(_context.Users, "UserId", "Email", productCategory.UserId);
            return View(productCategory);
        }

        // GET: ProductCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductCategories == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .Include(p => p.Parent)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: ProductCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductCategories == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.ProductCategories'  is null.");
            }
            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory != null)
            {
                _context.ProductCategories.Remove(productCategory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductCategoryExists(int id)
        {
          return (_context.ProductCategories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }
    }
}
