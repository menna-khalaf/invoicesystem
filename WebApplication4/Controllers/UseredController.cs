using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class UseredController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public UseredController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Usered
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only users that match the logged-in user's ID
            var users = await _context.Users
                .Where(u => u.UserId == userId.Value)
                .ToListAsync();

            return _context.Users != null ?
                        View(users) :
                        Problem("Entity set 'InvoiceSystemrahtkContext.Users' is null.");
        }

        // GET: Usered/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Usered/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usered/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Usered/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Usered/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,PasswordHash,Email,Phone,Role,CreatedAt,SubscriptionPlan,SubscriptionStartDate,SubscriptionEndDate,AutoRenewal,PaymentStatus")] User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password before saving it to the database
                user.PasswordHash = HashPassword(user.PasswordHash);

                // Set the CreatedAt field to the current date and time
                user.CreatedAt = DateTime.UtcNow;

                _context.Add(user);
                await _context.SaveChangesAsync();

                // Optionally, you can automatically log the user in after registration
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserEmail", user.Email);

                // Redirect to the Invoices controller, Index action
                return RedirectToAction("Index", "Invoices");
            }

            // If the model state is not valid, return the view with the user model
            return View(user);
        }

        // GET: Usered/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        } 
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Usered/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,PasswordHash,Email,Role,CreatedAt,SubscriptionPlan,SubscriptionStartDate,SubscriptionEndDate,AutoRenewal,PaymentStatus")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing user from the database
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Check if the password has been changed
                    if (user.PasswordHash != existingUser.PasswordHash)
                    {
                        // Hash the new password before saving
                        user.PasswordHash = HashPassword(user.PasswordHash);
                    }

                    // Update the user in the database
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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

            // If the model state is not valid, return the view with the user model
            return View(user);
        }

        // GET: Usered/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Usered/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
          return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }











        [HttpGet]
        [ActionName("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(User userModel)
        {
            if (ModelState.IsValid)
            {
                // Hash the entered password for comparison
                string hashedPassword = HashPassword(userModel.PasswordHash);

                // First, try to validate employee credentials
                var employee = _context.Employees
                    .FirstOrDefault(e => e.Email == userModel.Email && e.Password == hashedPassword && e.AllowSystemAccess);

                if (employee != null && employee.UserId.HasValue)
                {
                    // Employee found and has a valid UserId
                    HttpContext.Session.SetInt32("UserId", employee.UserId.Value);
                    HttpContext.Session.SetString("UserEmail", employee.Email);
                    return RedirectToAction("Index", "Invoices");
                }

                // If no employee is found, check the Users table
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == userModel.Email && u.PasswordHash == hashedPassword);

                if (user != null)
                {
                    // User found
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("Index", "Invoices");
                }
            }

            // Login failed: Show error message
            ViewBag.LoginStatus = 0;
            ModelState.AddModelError("", "Invalid email or password.");
            return View("Login");
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
        public IActionResult PlansView()
        {
            return View();
        }
    }
}
