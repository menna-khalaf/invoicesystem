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
    public class ShiftsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public ShiftsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }



        // GET: Shifts
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get shifts for the logged-in user including related Employee and Pos data
            var shifts = await _context.Shifts
                .Where(s => s.UserId == userId)
                .Include(s => s.Employee)
                .Include(s => s.Pos)
                .OrderBy(s => s.ShiftId)
                .ToListAsync();

            return View(shifts);
        }

        // GET: Shifts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Shifts == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Employee)
                .Include(s => s.Pos)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ShiftId == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }


        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Simplified query for employees, assuming Employee has a UserId column
            var userEmployees = _context.Employees
                .Where(e => e.UserId == userId)
                .ToList();
            Console.WriteLine($"UserId: {userId}, Employee Count: {userEmployees.Count}");

            var userPositions = _context.Pos
                .Where(p => p.UserId == userId)
                .ToList();
            Console.WriteLine($"Position Count: {userPositions.Count}");

            var days = new List<string> { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
            ViewData["Days"] = new MultiSelectList(days);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName");
            ViewData["Posid"] = new SelectList(userPositions, "Posid", "Posname");
            return View();
        }

        // POST: Shifts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ShiftId,StartDateTime,ShiftHours,EmployeeId,Posid,Notes")] Shift shift, bool RepeatWeekly, string[] SelectedDays)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Set UserId and CreatedAt
            shift.UserId = userId;
            shift.CreatedAt = DateTime.Now;

            // Handle RepeatWeekly checkbox
            shift.RepeatWeekly = RepeatWeekly ? true : null;

            // Handle selected days
            shift.RepeatDays = SelectedDays != null && SelectedDays.Any() ? string.Join(",", SelectedDays) : null;

            if (ModelState.IsValid)
            {
                _context.Add(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns and days if model state is invalid
            var userEmployees = _context.Employees
                .Where(e => _context.Branches
                    .Any(b => b.BranchId == e.BranchId && b.UserId == userId))
                .ToList();

            var userPositions = _context.Pos
                .Where(p => p.UserId == userId)
                .ToList();

            var days = new List<string> { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
            ViewData["Days"] = new MultiSelectList(days, SelectedDays);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", shift.EmployeeId);
            ViewData["Posid"] = new SelectList(userPositions, "Posid", "Posname", shift.Posid);
            return View(shift);
        }


        // GET: Shifts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Where(s => s.ShiftId == id && s.UserId == userId)
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return NotFound();
            }

            // Get employees that belong to the user's branches
            var userEmployees = _context.Employees
                .Where(e => _context.Branches
                    .Any(b => b.BranchId == e.BranchId && b.UserId == userId))
                .ToList();

            // Get positions (Pos) that belong to the logged-in user
            var userPositions = _context.Pos
                .Where(p => p.UserId == userId)
                .ToList();

            // List of days in Arabic
            var days = new List<string> { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
            ViewData["Days"] = new MultiSelectList(days, shift.RepeatDays?.Split(',', StringSplitOptions.RemoveEmptyEntries));
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", shift.EmployeeId);
            ViewData["Posid"] = new SelectList(userPositions, "Posid", "Posname", shift.Posid);

            return View(shift);
        }

        // POST: Shifts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ShiftId,StartDateTime,ShiftHours,EmployeeId,Posid,Notes,UserId,CreatedAt")] Shift shift, bool RepeatWeekly, string[] SelectedDays)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id != shift.ShiftId || shift.UserId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle RepeatWeekly checkbox
                    shift.RepeatWeekly = RepeatWeekly ? true : null;

                    // Handle selected days
                    shift.RepeatDays = SelectedDays != null && SelectedDays.Any() ? string.Join(",", SelectedDays) : null;

                    _context.Update(shift);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shift.ShiftId))
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

            // Repopulate dropdowns and days if model state is invalid
            var userEmployees = _context.Employees
                .Where(e => _context.Branches
                    .Any(b => b.BranchId == e.BranchId && b.UserId == userId))
                .ToList();

            var userPositions = _context.Pos
                .Where(p => p.UserId == userId)
                .ToList();

            var days = new List<string> { "السبت", "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة" };
            ViewData["Days"] = new MultiSelectList(days, SelectedDays);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", shift.EmployeeId);
            ViewData["Posid"] = new SelectList(userPositions, "Posid", "Posname", shift.Posid);

            return View(shift);
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ShiftId == id);
        }

        // GET: Shifts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Shifts == null)
            {
                return NotFound();
            }

            var shift = await _context.Shifts
                .Include(s => s.Employee)
                .Include(s => s.Pos)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.ShiftId == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // POST: Shifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Shifts == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Shifts'  is null.");
            }
            var shift = await _context.Shifts.FindAsync(id);
            if (shift != null)
            {
                _context.Shifts.Remove(shift);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //private bool ShiftExists(int id)
        //{
        //  return (_context.Shifts?.Any(e => e.ShiftId == id)).GetValueOrDefault();
        //}
    }
}
