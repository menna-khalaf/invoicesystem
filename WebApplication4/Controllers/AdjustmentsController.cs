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
    public class AdjustmentsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public AdjustmentsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }

            var invoiceSystemrahtkContext = _context.Adjustments
                .Include(a => a.Employee)
                .Where(a => a.Employee.UserId == userId.Value);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: Adjustments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Adjustments == null)
            {
                return NotFound();
            }

            var adjustment = await _context.Adjustments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.AdjustmentId == id);
            if (adjustment == null)
            {
                return NotFound();
            }

            return View(adjustment);
        }

        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }

            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId.Value),
                "EmployeeId",
                "FullName"
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AdjustmentId,AdjustmentDate,EmployeeId,Notes")] Adjustment adjustment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (ModelState.IsValid)
            {
                adjustment.UserId = userId.Value; // Set UserId from session
                _context.Add(adjustment);
                await _context.SaveChangesAsync();

                // Save the AdjustmentId in the session
                HttpContext.Session.SetInt32("AdjustmentId", adjustment.AdjustmentId);

                // Redirect to the CreateAdjustmentProduct action in the AdjustmentProduct controller
                return RedirectToAction("CreateAdjustmentProduct", "AdjustmentProducts");
            }

            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Where(e => e.UserId == userId.Value),
                "EmployeeId",
                "FullName",
                adjustment.EmployeeId
            );
            return View(adjustment);
        }

        // GET: Adjustments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Adjustments == null)
            {
                return NotFound();
            }

            var adjustment = await _context.Adjustments.FindAsync(id);
            if (adjustment == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", adjustment.EmployeeId);
            return View(adjustment);
        }

        // POST: Adjustments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AdjustmentId,AdjustmentDate,EmployeeId,Notes")] Adjustment adjustment)
        {
            if (id != adjustment.AdjustmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(adjustment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdjustmentExists(adjustment.AdjustmentId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", adjustment.EmployeeId);
            return View(adjustment);
        }

        // GET: Adjustments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Adjustments == null)
            {
                return NotFound();
            }

            var adjustment = await _context.Adjustments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.AdjustmentId == id);
            if (adjustment == null)
            {
                return NotFound();
            }

            return View(adjustment);
        }

        // POST: Adjustments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Adjustments == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Adjustments'  is null.");
            }
            var adjustment = await _context.Adjustments.FindAsync(id);
            if (adjustment != null)
            {
                _context.Adjustments.Remove(adjustment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdjustmentExists(int id)
        {
          return (_context.Adjustments?.Any(e => e.AdjustmentId == id)).GetValueOrDefault();
        }
    }
}
