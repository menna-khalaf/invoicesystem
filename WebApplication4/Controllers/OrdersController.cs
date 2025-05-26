using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;
using System.Text.Json;

namespace WebApplication4.Controllers
{
    public class OrdersController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public OrdersController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Orders/Index
        public async Task<IActionResult> Index()
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Filter orders by user ID and include related entities
            var invoiceSystemrahtkContext = _context.Orders
                .Where(o => o.UserId == userId.Value)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Product)
                .Include(o => o.Storehouse);

            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Product)
                .Include(o => o.Storehouse)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

// GET: Orders/Create
public IActionResult Create()
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var model = new CreateOrderViewModel
            {
                Products = new List<OrderProductingViewModel> { new OrderProductingViewModel() }
            };

            // Filter all data by user ID
            ViewData["CustomerId"] = new SelectList(_context.Customers
                .Where(c => c.UserId == userId), "CustomerId", "CustomerName");

            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName");

            ViewData["StorehouseId"] = new SelectList(_context.Storehouses
                .Where(s => s.UserId == userId), "StorehouseId", "StorehouseName");

            ViewData["Products"] = _context.Products
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.PurchasePrice
                }).ToList();

            return View(model);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateOrderViewModel model)
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            Console.WriteLine($"Create action called. Model: {JsonSerializer.Serialize(model)}");

            if (model == null || model.Products == null || !model.Products.Any())
            {
                Console.WriteLine("Model or products are null or empty.");
                return Json(new { success = false, message = "No data received." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate foreign keys and ensure they belong to the user
                    bool customerExists = await _context.Customers
                        .AnyAsync(c => c.CustomerId == model.CustomerId && c.UserId == userId);

                    bool storehouseExists = await _context.Storehouses
                        .AnyAsync(s => s.StorehouseId == model.StorehouseId && s.UserId == userId);

                    bool employeeExists = await _context.Employees
                        .AnyAsync(e => e.EmployeeId == model.EmployeeId && e.UserId == userId);

                    bool allProductsExist = model.Products.All(p =>
                        _context.Products.Any(pr => pr.ProductId == p.ProductId && pr.UserId == userId && pr.StorehouseId == model.StorehouseId));

                    Console.WriteLine($"Customer exists: {customerExists}, Storehouse exists: {storehouseExists}, Employee exists: {employeeExists}, All products exist: {allProductsExist}");

                    if (!customerExists) return Json(new { success = false, message = "Customer not found or not authorized." });
                    if (!storehouseExists) return Json(new { success = false, message = "Storehouse not found or not authorized." });
                    if (!employeeExists) return Json(new { success = false, message = "Employee not found or not authorized." });
                    if (!allProductsExist) return Json(new { success = false, message = "One or more products not found, not authorized, or not associated with the selected storehouse." });

                    // Create one Order record for each product
                    foreach (var product in model.Products)
                    {
                        var order = new Order
                        {
                            OrderType = model.OrderType,
                            CustomerId = model.CustomerId,
                            StorehouseId = model.StorehouseId,
                            EmployeeId = model.EmployeeId,
                            Notes = model.Notes,
                            OrderDate = model.OrderDate,
                            ProductId = product.ProductId,
                            Quantity = product.Quantity,
                            UnitPrice = product.UnitPrice,
                            Total = product.Total,
                            UserId = userId.Value // Add user ID to the order
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Order Inserted - Order ID: {order.OrderId}, Product ID: {product.ProductId}, Quantity: {product.Quantity}");

                        // Insert Inventory record for this product
                        var inventory = new Inventory
                        {
                            ProductId = product.ProductId,
                            StorehouseId = model.StorehouseId,
                            IncomingQuantity = product.Quantity,
                            LastUpdated = DateTime.Now,
                            OrderId = order.OrderId
                        };

                        _context.Inventories.Add(inventory);
                        Console.WriteLine($"Added inventory for Product ID: {product.ProductId}");

                        // Update Product Balance
                        var productToUpdate = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && p.StorehouseId == model.StorehouseId && p.UserId == userId.Value);

                        if (productToUpdate != null)
                        {
                            productToUpdate.Balance += product.Quantity;
                            _context.Products.Update(productToUpdate);
                            Console.WriteLine($"Updated balance for Product ID: {product.ProductId}, New Balance: {productToUpdate.Balance}");
                        }
                        else
                        {
                            Console.WriteLine($"Product ID: {product.ProductId} not found for Storehouse ID: {model.StorehouseId} or User ID: {userId.Value}");
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = $"Product ID: {product.ProductId} not found for the selected storehouse." });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("✅ All orders, inventory records, and product balances updated successfully.");

                    return Json(new { success = true, message = "تم إنشاء الطلب، تحديث المخزون، وتعديل رصيد المنتج بنجاح!", redirectUrl = Url.Action("Index") });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ Transaction rolled back due to error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء الحفظ: {ex.InnerException?.Message ?? ex.Message}" });
                }
            }
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", order.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", order.EmployeeId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", order.ProductId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", order.StorehouseId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderType,CustomerId,StorehouseId,ProductId,Quantity,UnitPrice,Total,OrderDate,Notes,EmployeeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", order.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", order.EmployeeId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", order.ProductId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", order.StorehouseId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Product)
                .Include(o => o.Storehouse)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
          return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }



















 


        public IActionResult Add()
        {
            // Create a new instance of the view model
            var model = new CreateOrderViewModel
            {
                Products = new List<OrderProductingViewModel> { new OrderProductingViewModel() } // Initialize with one product row
            };

            // Populate dropdowns from the database
            ViewBag.Customers = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.CustomerId.ToString(),
                Text = c.CustomerName
            }).ToList();

            ViewBag.Storehouses = _context.Storehouses.Select(s => new SelectListItem
            {
                Value = s.StorehouseId.ToString(),
                Text = s.StorehouseName
            }).ToList();

            ViewBag.Employees = _context.Employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.FullName
            }).ToList();

            // Pass Products with PurchasePrice and Balance
            ViewBag.Products = _context.Products.Select(p => new
            {
                p.ProductId,
                p.ProductName,
                p.PurchasePrice,
                p.Balance
            }).ToList();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateOrderViewModel model)
        {
            if (model == null || model.Products == null || !model.Products.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate required fields
                    if (string.IsNullOrEmpty(model.OrderType) ||
                        model.CustomerId <= 0 ||
                        model.StorehouseId <= 0 ||
                        model.EmployeeId <= 0 ||
                        model.OrderDate == default(DateTime)) // Validate OrderDate
                    {
                        return Json(new { success = false, message = "جميع الحقول مطلوبة. يرجى التأكد من إدخال جميع البيانات." });
                    }

                    // Validate foreign keys
                    bool customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == model.CustomerId);
                    bool storehouseExists = await _context.Storehouses.AnyAsync(s => s.StorehouseId == model.StorehouseId);
                    bool employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == model.EmployeeId);

                    if (!customerExists) return Json(new { success = false, message = "Customer not found." });
                    if (!storehouseExists) return Json(new { success = false, message = "Storehouse not found." });
                    if (!employeeExists) return Json(new { success = false, message = "Employee not found." });

                    // Calculate total quantity from all products
                    int totalQuantity = model.Products.Sum(p => p.Quantity);

                    // Insert a single row in Orders table with total quantity
                    var order = new Order
                    {
                        OrderType = model.OrderType,
                        CustomerId = model.CustomerId,
                        StorehouseId = model.StorehouseId,
                        EmployeeId = model.EmployeeId,
                        Notes = model.Notes,
                        OrderDate = model.OrderDate, // Use the selected date
                        Quantity = totalQuantity // Store the total quantity in Orders
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Order Inserted - Order ID: {order.OrderId}, Total Quantity: {totalQuantity}");

                    // Insert multiple rows in Inventory table for each product
                    foreach (var product in model.Products)
                    {
                        if (product.Quantity <= 0)
                        {
                            return Json(new { success = false, message = $"Quantity for product {product.ProductId} must be greater than 0." });
                        }

                        var inventory = new Inventory
                        {
                            ProductId = product.ProductId,
                            StorehouseId = model.StorehouseId,
                            IncomingQuantity = product.Quantity,
                            LastUpdated = DateTime.Now,
                            OrderId = order.OrderId // Link to the same OrderId
                        };

                        _context.Inventories.Add(inventory);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("✅ Inventory records created and linked to the order successfully.");

                    return Json(new { success = true, message = "تم إنشاء الطلب وتحديث المخزون بنجاح!", redirectUrl = Url.Action("Index") });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ Transaction rolled back due to error: {ex.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء الحفظ: {ex.InnerException?.Message ?? ex.Message}" });
                }
            }
        }




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




















        // GET: Orders/CreateMinus
        public IActionResult CreateMinus()
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var model = new CreateOrderViewModel
            {
                Products = new List<OrderProductingViewModel> { new OrderProductingViewModel() },
                OrderType = "صرف" // Set default OrderType
            };

            // Filter all data by user ID
            ViewData["CustomerId"] = new SelectList(_context.Customers
                .Where(c => c.UserId == userId), "CustomerId", "CustomerName");

            ViewData["EmployeeId"] = new SelectList(_context.Employees
                .Where(e => e.UserId == userId), "EmployeeId", "FullName");

            ViewData["StorehouseId"] = new SelectList(_context.Storehouses
                .Where(s => s.UserId == userId), "StorehouseId", "StorehouseName");

            ViewData["Products"] = _context.Products
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Price, // Use Price instead of PurchasePrice
                    p.Balance // Include Balance for validation
                }).ToList();

            return View(model);
        }

        // POST: Orders/CreateMinus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMinus([FromBody] CreateOrderViewModel model)
        {
            // Get user ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            Console.WriteLine($"CreateMinus action called. Model: {JsonSerializer.Serialize(model)}");

            if (model == null || model.Products == null || !model.Products.Any())
            {
                Console.WriteLine("Model or products are null or empty.");
                return Json(new { success = false, message = "No data received." });
            }

            // Force OrderType to be "صرف"
            model.OrderType = "صرف";

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Validation errors: {string.Join(", ", errors)}");
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Validate foreign keys and ensure they belong to the user
                    bool customerExists = await _context.Customers
                        .AnyAsync(c => c.CustomerId == model.CustomerId && c.UserId == userId);

                    bool storehouseExists = await _context.Storehouses
                        .AnyAsync(s => s.StorehouseId == model.StorehouseId && s.UserId == userId);

                    bool employeeExists = await _context.Employees
                        .AnyAsync(e => e.EmployeeId == model.EmployeeId && e.UserId == userId);

                    bool allProductsExist = model.Products.All(p =>
                        _context.Products.Any(pr => pr.ProductId == p.ProductId && pr.UserId == userId && pr.StorehouseId == model.StorehouseId));

                    Console.WriteLine($"Customer exists: {customerExists}, Storehouse exists: {storehouseExists}, Employee exists: {employeeExists}, All products exist: {allProductsExist}");

                    if (!customerExists) return Json(new { success = false, message = "Customer not found or not authorized." });
                    if (!storehouseExists) return Json(new { success = false, message = "Storehouse not found or not authorized." });
                    if (!employeeExists) return Json(new { success = false, message = "Employee not found or not authorized." });
                    if (!allProductsExist) return Json(new { success = false, message = "One or more products not found, not authorized, or not associated with the selected storehouse." });

                    // Create one Order record for each product
                    foreach (var product in model.Products)
                    {
                        var order = new Order
                        {
                            OrderType = model.OrderType,
                            CustomerId = model.CustomerId,
                            StorehouseId = model.StorehouseId,
                            EmployeeId = model.EmployeeId,
                            Notes = model.Notes,
                            OrderDate = model.OrderDate,
                            ProductId = product.ProductId,
                            Quantity = product.Quantity,
                            UnitPrice = product.UnitPrice,
                            Total = product.Total,
                            UserId = userId.Value // Add user ID to the order
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"✅ Order Inserted - Order ID: {order.OrderId}, Product ID: {product.ProductId}, Quantity: {product.Quantity}");

                        // Insert Inventory record for this product (OutgoingQuantity)
                        var inventory = new Inventory
                        {
                            ProductId = product.ProductId,
                            StorehouseId = model.StorehouseId,
                            OutgoingQuantity = product.Quantity, // Set OutgoingQuantity
                            IncomingQuantity = 0, // Set IncomingQuantity to 0
                            LastUpdated = DateTime.Now,
                            OrderId = order.OrderId
                        };

                        _context.Inventories.Add(inventory);
                        Console.WriteLine($"Added inventory for Product ID: {product.ProductId} with OutgoingQuantity: {product.Quantity}");

                        // Update Product Balance (Decrease)
                        var productToUpdate = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == product.ProductId && p.StorehouseId == model.StorehouseId && p.UserId == userId.Value);

                        if (productToUpdate != null)
                        {
                            // Check if balance will go negative
                            if (productToUpdate.Balance < product.Quantity)
                            {
                                Console.WriteLine($"Insufficient balance for Product ID: {product.ProductId}. Current Balance: {productToUpdate.Balance}, Requested: {product.Quantity}");
                                await transaction.RollbackAsync();
                                return Json(new { success = false, message = $"الرصيد غير كافٍ للمنتج {productToUpdate.ProductName}. الرصيد الحالي: {productToUpdate.Balance}, المطلوب: {product.Quantity}" });
                            }

                            productToUpdate.Balance -= product.Quantity;
                            _context.Products.Update(productToUpdate);
                            Console.WriteLine($"Decreased balance for Product ID: {product.ProductId}, New Balance: {productToUpdate.Balance}");
                        }
                        else
                        {
                            Console.WriteLine($"Product ID: {product.ProductId} not found for Storehouse ID: {model.StorehouseId} or User ID: {userId.Value}");
                            await transaction.RollbackAsync();
                            return Json(new { success = false, message = $"Product ID: {product.ProductId} not found for the selected storehouse." });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("✅ All orders, inventory records, and product balances updated successfully.");

                    return Json(new { success = true, message = "تم إنشاء إذن الصرف، تحديث المخزون، وتعديل رصيد المنتج بنجاح!", redirectUrl = Url.Action("Index") });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"❌ Transaction rolled back due to error: {ex.Message}, Inner: {ex.InnerException?.Message}");
                    return Json(new { success = false, message = $"حدث خطأ أثناء الحفظ: {ex.InnerException?.Message ?? ex.Message}" });
                }
            }
        }
    }
}
