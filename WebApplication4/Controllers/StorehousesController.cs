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
    public class StorehousesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public StorehousesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Storehouses
        public async Task<IActionResult> Index()
        {
            // Get UserID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (_context.Storehouses == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Storehouses' is null.");
            }

            // Filter storehouses by user ID
            var userStorehouses = await _context.Storehouses
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View(userStorehouses);
        }
        // GET: Storehouses/Create
        public IActionResult Create()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only branches that belong to this user
            var branches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName");
            return View();
        }


        // AJAX endpoint to get branch employees
        public IActionResult GetBranchEmployees(int branchId)
        {
            var employees = _context.Employees
                .Where(e => e.BranchId == branchId)
                .Select(e => new {
                    e.EmployeeId,
                    e.FullName,
                    e.Position
                })
                .ToList();

            return Json(employees);
        }
        // POST: Storehouses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("StorehouseName,Location,SubLocation,DetailedLocation,Status,BranchId")] Storehouse storehouse)
        {
            // Get UserID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (ModelState.IsValid)
            {
                // Create new storehouse and assign user & branch
                var newStorehouse = new Storehouse
                {
                    StorehouseName = storehouse.StorehouseName,
                    Location = storehouse.Location,
                    SubLocation = storehouse.SubLocation,
                    DetailedLocation = storehouse.DetailedLocation,
                    Status = storehouse.Status,
                    BranchId = storehouse.BranchId, // Make sure this is posted from the form
                    UserId = userId.Value, // From session
                    CreatedAt = DateTime.Now
                };

                _context.Add(newStorehouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate the branch list based on the current user
            var branches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", storehouse.BranchId);
            return View(storehouse);
        }


        // GET: Storehouses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Get UserID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Storehouses == null)
            {
                return NotFound();
            }

            // Only find storehouse if it belongs to the current user
            var storehouse = await _context.Storehouses
                .FirstOrDefaultAsync(s => s.StorehouseId == id && s.UserId == userId);

            if (storehouse == null)
            {
                return NotFound();
            }

            // Repopulate the branch list based on the current user
            var branches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", storehouse.BranchId);
            return View(storehouse);
        }

        // POST: Storehouses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("StorehouseId,StorehouseName,Location,SubLocation,DetailedLocation,Status,BranchId")] Storehouse storehouse)
        {
            // Get UserID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id != storehouse.StorehouseId)
            {
                return NotFound();
            }

            // Verify the storehouse belongs to the current user
            var existingStorehouse = await _context.Storehouses
                .FirstOrDefaultAsync(s => s.StorehouseId == id && s.UserId == userId);

            if (existingStorehouse == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only the allowed fields
                    existingStorehouse.StorehouseName = storehouse.StorehouseName;
                    existingStorehouse.Location = storehouse.Location;
                    existingStorehouse.SubLocation = storehouse.SubLocation;
                    existingStorehouse.DetailedLocation = storehouse.DetailedLocation;
                    existingStorehouse.Status = storehouse.Status;
                    existingStorehouse.BranchId = storehouse.BranchId;
                    existingStorehouse.UserId = userId.Value; // Ensure user ID remains the same

                    _context.Update(existingStorehouse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StorehouseExists(storehouse.StorehouseId))
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

            // Repopulate the branch list if model state is invalid
            var branches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewBag.Branches = new SelectList(branches, "BranchId", "BranchName", storehouse.BranchId);
            return View(storehouse);
        }

        //private bool StorehouseExists(int id)
        //{
        //    return (_context.Storehouses?.Any(e => e.StorehouseId == id)).GetValueOrDefault();
        //}












        // GET: Storehouses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Get UserID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Storehouses == null)
            {
                return NotFound();
            }

            // Only find storehouse if it belongs to the current user
            var storehouse = await _context.Storehouses
                .FirstOrDefaultAsync(m => m.StorehouseId == id && m.UserId == userId);

            if (storehouse == null)
            {
                return NotFound();
            }

            return View(storehouse);
        }
        // GET: Storehouses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Storehouses == null)
            {
                return NotFound();
            }

            var storehouse = await _context.Storehouses
                .FirstOrDefaultAsync(m => m.StorehouseId == id);
            if (storehouse == null)
            {
                return NotFound();
            }

            return View(storehouse);
        }

        // POST: Storehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Storehouses == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Storehouses'  is null.");
            }
            var storehouse = await _context.Storehouses.FindAsync(id);
            if (storehouse != null)
            {
                _context.Storehouses.Remove(storehouse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StorehouseExists(int id)
        {
            return (_context.Storehouses?.Any(e => e.StorehouseId == id)).GetValueOrDefault();
        }







        // GET: Storehouses/AdditionPermission
        public ActionResult AdditionPermission()
        {
            // Populate dropdowns
            ViewBag.Storehouses = _context.Storehouses
                .Select(s => new { s.StorehouseId, s.StorehouseName })
                .ToList();

            ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName");
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "CustomerName");

            return View();
        }

        [HttpPost]
        public ActionResult AdditionPermission(AdditionPermissionViewModel model)
        {
            Console.WriteLine("AdditionPermission POST action called.");

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid.");

                try
                {
                    // Fetch the current balance of the product
                    var product = _context.Products
                        .FirstOrDefault(p => p.ProductId == model.ProductID);

                    if (product == null)
                    {
                        Console.WriteLine("Product not found in the database.");
                        ModelState.AddModelError("", "Product not found.");
                        return View(model);
                    }

                    Console.WriteLine($"Product found: ProductId = {product.ProductId}, Balance = {product.Balance}");

                    // Create a new Inventory record
                    var inventory = new Inventory
                    {
                        ProductId = model.ProductID,
                        StorehouseId = model.StorehouseID,
                        IncomingQuantity = model.IncomingQuantity,
                        OutgoingQuantity = 0, // Set outgoing quantity to 0
                        Balance = product.Balance + model.IncomingQuantity, // Update balance
                        LastUpdated = DateTime.Now
                    };

                    Console.WriteLine("New Inventory record created:");
                    Console.WriteLine($"ProductId = {inventory.ProductId}, StorehouseId = {inventory.StorehouseId}, IncomingQuantity = {inventory.IncomingQuantity}, Balance = {inventory.Balance}");

                    // Add the new inventory record to the context
                    _context.Inventories.Add(inventory);
                    Console.WriteLine("Inventory record added to the context.");

                    // Update the product's balance in the Products table
                    product.Balance = inventory.Balance;
                    Console.WriteLine($"Product balance updated to: {product.Balance}");

                    // Save changes to the database
                    Console.WriteLine("Saving changes to the database...");
                    _context.SaveChanges();
                    Console.WriteLine("Changes saved successfully.");

                    return RedirectToAction("Success"); // Redirect to a success page
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine("Error saving to database: " + ex.Message);
                    Console.WriteLine("Stack Trace: " + ex.StackTrace);
                    ModelState.AddModelError("", "An error occurred while saving the data.");
                }
            }
            else
            {
                Console.WriteLine("ModelState is invalid. Validation errors:");

                // Log all validation errors
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }
            }

            // Repopulate dropdowns on validation failure
            ViewBag.Storehouses = _context.Storehouses.ToList();
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName");
            ViewBag.Customers = new SelectList(_context.Customers, "CustomerId", "CustomerName");

            return View(model);
        }
        // GET: Storehouses/GetProductDetails
        public JsonResult GetProductDetails(int productId)
        {
            var product = _context.Products
                .Where(p => p.ProductId == productId)
                .Select(p => new
                {
                    PurchasePrice = p.PurchasePrice, // Ensure this matches the database column
                    Balance = p.Balance
                })
                .FirstOrDefault();

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            // Log the fetched product details for debugging
            Console.WriteLine($"Product Found: PurchasePrice = {product.PurchasePrice}, Balance = {product.Balance}");

            return Json(new { success = true, data = product });
        }

    }
}

