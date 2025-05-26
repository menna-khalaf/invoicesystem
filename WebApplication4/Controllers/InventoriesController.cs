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
    public class InventoriesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public InventoriesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Inventories
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.Inventories.Include(i => i.Product);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Inventories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Inventories == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // GET: Inventories/Create
        public IActionResult Create()
        {
            // Fetch products and create a SelectList
            var products = _context.Products
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName
                })
                .ToList();

            // Pass the SelectList to the view
            ViewBag.ProductId = new SelectList(products, "Value", "Text");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int ProductId, int IncomingQuantity, int OutgoingQuantity)
        {
            try
            {
                // Create a new Inventory object
                var inventory = new Inventory
                {
                    ProductId = ProductId,
                    IncomingQuantity = IncomingQuantity,
                    OutgoingQuantity = OutgoingQuantity,
                    LastUpdated = DateTime.Now
                };

                // Add the inventory to the context and save changes
                _context.Add(inventory);
                await _context.SaveChangesAsync();

                // Return a success response
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine("Error saving to database: " + ex.Message);

                // Return an error response
                return Json(new { success = false });
            }
        }





        // GET: Inventories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Inventories == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", inventory.ProductId);
            return View(inventory);
        }

        // POST: Inventories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InventoryId,ProductId,IncomingQuantity,OutgoingQuantity,Balance,LastUpdated")] Inventory inventory)
        {
            if (id != inventory.InventoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventory.InventoryId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", inventory.ProductId);
            return View(inventory);
        }

        // GET: Inventories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Inventories == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // POST: Inventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Inventories == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Inventories'  is null.");
            }
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryExists(int id)
        {
          return (_context.Inventories?.Any(e => e.InventoryId == id)).GetValueOrDefault();
        }
    }
}
