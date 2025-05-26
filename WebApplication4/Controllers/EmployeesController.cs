using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public EmployeesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get the logged-in user's ID from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // If no user is logged in, redirect to login page
                return RedirectToAction("Login", "Usered");
            }

            // Filter employees by the logged-in user's ID
            var invoiceSystemrahtkContext = _context.Employees
                .Include(e => e.User)
                .Where(e => e.UserId == userId);  // Assuming there's a UserId foreign key in Employees table

            return View(await invoiceSystemrahtkContext.ToListAsync());
        }
        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }
        // GET: Employee/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Populate roles
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "SalesRepresentative", Text = "مندوب مبيعات" },
                new SelectListItem { Value = "PurchaseRepresentative", Text = "مندوب شراء" }
            };
            ViewBag.Role = new SelectList(roles, "Value", "Text");

            // Populate countries from Countrycurrency table
            var countries = _context.Countrycurrencies
                .Select(c => new SelectListItem
                {
                    Value = c.CountryName,
                    Text = c.CountryName
                })
                .ToList();
            ViewBag.Countries = countries;

            // Initialize the employee with default values
            var employee = new Employee
            {
                UserId = userId.Value, // Set UserId from session
                HireDate = DateTime.Today, // Use DateTime directly
                AttendanceTime = new TimeOnly(8, 0),
                DepartureTime = new TimeOnly(17, 0),
                AllowSystemAccess = false,
                Country = "المملكة العربية السعودية" // Set default country
            };

            return View(employee);
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("EmployeeId,UserId,FullName,Position,Role,Salary,HireDate,AttendanceTime,DepartureTime,BranchID,MobileNumber,SecondaryMobileNumber,Email,AllowSystemAccess,Country,City,Address,Notes")] Employee employee,
            IFormFileCollection attachments,
            string password) // Password passed separately to avoid binding it directly
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Validate that the UserId from the form matches the session UserId
            if (employee.UserId != userId.Value)
            {
                ModelState.AddModelError("UserId", "معرف المستخدم غير ص valid.");
                return await RepopulateView(employee);
            }

            // Remove Password from ModelState to avoid validation issues
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the password using SHA-256
                    if (!string.IsNullOrEmpty(password))
                    {
                        employee.Password = HashPassword(password);
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "كلمة المرور مطلوبة.");
                        return await RepopulateView(employee);
                    }

                    // Handle file uploads for attachments
                    if (attachments != null && attachments.Count > 0)
                    {
                        var filePaths = new List<string>();
                        foreach (var file in attachments)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }
                                filePaths.Add($"/uploads/{fileName}");
                            }
                        }
                        employee.Attachments = string.Join(",", filePaths);
                    }

                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    // Generate WhatsApp link with pre-filled message including email and original password
                    if (!string.IsNullOrEmpty(employee.MobileNumber) && !string.IsNullOrEmpty(employee.Email))
                    {
                        try
                        {
                            // Format mobile number to WhatsApp format (e.g., "+1234567890")
                            var formattedNumber = employee.MobileNumber.StartsWith("+") ? employee.MobileNumber : $"+{employee.MobileNumber}";
                            formattedNumber = formattedNumber.Replace(" ", "").Replace("-", "");

                            // Create the message with email and original password
                            var message = $"مرحبًا! تفاصيل الدخول إلى النظام:%0Aالبريد الإلكتروني: {employee.Email}%0Aكلمة المرور: {password}";
                            var encodedMessage = HttpUtility.UrlEncode(message);
                            var whatsappLink = $"https://wa.me/{formattedNumber}?text={encodedMessage}";

                            return Redirect(whatsappLink);
                        }
                        catch (Exception ex)
                        {
                            // Log the error (replace with proper logging)
                            Console.WriteLine($"Failed to generate WhatsApp link: {ex.Message}");
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the error (replace with proper logging)
                    Console.WriteLine($"Error creating employee: {ex.Message}");
                    ModelState.AddModelError("", "حدث خطأ أثناء إنشاء الموظف. يرجى المحاولة مرة أخرى.");
                }
            }

            return await RepopulateView(employee);
        }

        // Helper method to repopulate dropdowns and return view
        private async Task<IActionResult> RepopulateView(Employee employee)
        {
            // Get the logged-in user ID
            var userId = HttpContext.Session.GetInt32("UserId");

            // Populate roles
            ViewBag.Role = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "SalesRepresentative", Text = "مندوب مبيعات" },
                new SelectListItem { Value = "PurchaseRepresentative", Text = "مندوب شراء" }
            }, "Value", "Text", employee.Role);

            // Populate countries dropdown
            var countries = _context.Countrycurrencies
                .Select(c => new SelectListItem
                {
                    Value = c.CountryName,
                    Text = c.CountryName
                })
                .ToList();
            ViewBag.Countries = countries;

            return View(employee);
        }
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", employee.UserId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,UserId,FullName,Position,Salary,HireDate,AttendanceTime,DepartureTime")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
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
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", employee.UserId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Employees'  is null.");
            }
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
          return (_context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }
















        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
