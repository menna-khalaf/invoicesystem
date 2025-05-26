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
    public class BranchesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public BranchesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }
        // GET: Branches
        public async Task<IActionResult> Index()
        {
            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Return a problem result if UserId is not found in session
                return Problem("معرف المستخدم غير موجود في الجلسة. الرجاء تسجيل الدخول.");
            }

            // Check if Branches context is null
            if (_context.Branches == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Branches' is null.");
            }

            // Retrieve branches where UserId matches the session UserId
            var branches = await _context.Branches
                .Where(b => b.UserId == userId.Value)
                .ToListAsync();

            return View(branches);
        }

        // GET: Branches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            // Include the Employees navigation property in the query
            var branch = await _context.Branches
                .Include(b => b.Employees) // Include related Employees
                .FirstOrDefaultAsync(m => m.BranchId == id);

            if (branch == null)
            {
                return NotFound();
            }

            // Fetch the selected employee (if any) and their contact details
            var selectedEmployee = branch.Employees.FirstOrDefault(); // Assuming only one employee is assigned
            if (selectedEmployee != null)
            {
                ViewBag.SelectedEmployee = new
                {
                    FullName = selectedEmployee.FullName,
                    Email = selectedEmployee.Email,
                    MobileNumber = selectedEmployee.MobileNumber,
                    Position = selectedEmployee.Position
                };
            }
            else
            {
                ViewBag.SelectedEmployee = null;
            }

            return View(branch);
        }

        // GET: Branches/Create
        public IActionResult Create()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if no session
            }

            // Fetch all countries from Countrycurrency table
            var countries = _context.Countrycurrencies
                .Where(c => c.IsActive == true) // Only active countries
                .Select(c => new { c.CountryCurrencyId, c.CountryName })
                .OrderBy(c => c.CountryName)
                .ToList();
            ViewBag.Countries = new SelectList(countries, "CountryName", "CountryName");

            // Fetch employees for the dropdown, filtered by UserId
            var employees = _context.Employees
                .Where(e => e.UserId == userId.Value) // Filter by session UserId
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.FullName
                })
                .ToList();
            ViewBag.Employees = employees;

            // Set default country to Saudi Arabia and default status to "نشط"
            var branch = new Branch
            {
                Country = "المملكة العربية السعودية", // Default country
                BranchStatus = "نشط", // Set default status
                UserId = userId.Value // Set the UserId from session
            };

            return View(branch);
        }

        [HttpGet]
        public async Task<JsonResult> GetEmployeePosition(int employeeId)
        {
            var employee = await _context.Employees
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => new { e.Position })
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return Json(""); // Return empty string if employee not found
            }

            return Json(employee.Position);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch, int? EmployeeId)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure the UserId from session is used
            branch.UserId = userId.Value;

            if (ModelState.IsValid)
            {
                // Add the branch to the database
                _context.Add(branch);
                await _context.SaveChangesAsync();

                // Assign the selected employee to the branch
                if (EmployeeId.HasValue)
                {
                    var employee = await _context.Employees.FindAsync(EmployeeId.Value);
                    if (employee != null)
                    {
                        employee.BranchId = branch.BranchId; // Set the BranchId for the employee
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if the model state is invalid
            ViewBag.Countries = new SelectList(
                _context.Countrycurrencies
                    .Where(c => c.IsActive == true)
                    .Select(c => new { c.CountryCurrencyId, c.CountryName })
                    .OrderBy(c => c.CountryName),
                "CountryName",
                "CountryName",
                branch.Country
            );

            ViewBag.Employees = _context.Employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.FullName
            }).ToList();

            return View(branch);
        }


// Static list of cities for Saudi Arabia
private List<string> GetCitiesForSaudiArabia()
        {
            return new List<string>
            {
                "الرياض",
                "جدة",
                "مكة المكرمة",
                "المدينة المنورة",
                "الدمام",
                "الخبر",
                "الطائف",
                "تبوك",
                "بريدة",
                "حائل",
                "أبها",
                "نجران",
                "جازان",
                "الظهران",
                "القصيم",
                "ينبع",
                "خميس مشيط",
                "حفر الباطن",
                "الجبيل",
                "ضباء"
            };
        }

        // GET: Branches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            // Fetch the branch with related employees
            var branch = await _context.Branches
                .Include(b => b.Employees) // Include related Employees
                .FirstOrDefaultAsync(m => m.BranchId == id);

            if (branch == null)
            {
                return NotFound();
            }

            // Fetch static list of cities for Saudi Arabia
            var cities = GetCitiesForSaudiArabia();
            ViewBag.Cities = cities.Select(c => new SelectListItem { Value = c, Text = c }).ToList();

            // Fetch employees for the dropdown
            var employees = _context.Employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.FullName
            }).ToList();
            ViewBag.Employees = employees;

            return View(branch);
        }

        // POST: Branches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BranchId,BranchName,BranchStatus,City,CreatedAt,Country,Location,MasterBranch,CommercialRegister")] Branch branch, int? EmployeeId)
        {
            if (id != branch.BranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the branch
                    _context.Update(branch);

                    // Assign the selected employee to the branch
                    if (EmployeeId.HasValue)
                    {
                        var employee = await _context.Employees.FindAsync(EmployeeId.Value);
                        if (employee != null)
                        {
                            employee.BranchId = branch.BranchId; // Set the BranchId for the employee
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId))
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

            // Repopulate dropdowns if the model state is invalid
            ViewBag.Cities = GetCitiesForSaudiArabia().Select(c => new SelectListItem { Value = c, Text = c }).ToList();
            ViewBag.Employees = _context.Employees.Select(e => new SelectListItem
            {
                Value = e.EmployeeId.ToString(),
                Text = e.FullName
            }).ToList();

            return View(branch);
        }
        // GET: Branches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.BranchId == id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // POST: Branches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Branches == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Branches'  is null.");
            }
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
          return (_context.Branches?.Any(e => e.BranchId == id)).GetValueOrDefault();
        }
    }
}
