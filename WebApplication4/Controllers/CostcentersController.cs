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
    public class CostcentersController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public CostcentersController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Costcenters
        // GET: Costcenters
        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only cost centers that belong to the logged-in user
            var userCostCenters = _context.Costcenters
                .Where(c => c.UserId == userId)
                .Include(c => c.Branch)
                .Include(c => c.User)
                .ToListAsync();

            return View(await userCostCenters);
        }

        // GET: Costcenters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Costcenters == null)
            {
                return NotFound();
            }

            var costcenter = await _context.Costcenters
                .Include(c => c.Branch)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CostCenterId == id);
            if (costcenter == null)
            {
                return NotFound();
            }

            return View(costcenter);
        }

        // GET: Costcenters/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only branches that belong to the logged-in user
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName");
            return View();
        }

        // POST: Costcenters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CostCenterId,CenterName,BranchId,CreatedAt")] Costcenter costcenter)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Additional validation - check if the selected branch belongs to the user
            var userBranch = _context.Branches
                .Any(b => b.BranchId == costcenter.BranchId && b.UserId == userId);

            if (!userBranch)
            {
                ModelState.AddModelError("BranchId", "You don't have permission to use this branch");
            }

            if (ModelState.IsValid)
            {
                // Set the UserId from session
                costcenter.UserId = userId.Value;
                costcenter.CreatedAt = DateTime.Now;

                _context.Add(costcenter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed; redisplay form with only user's branches
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", costcenter.BranchId);
            return View(costcenter);
        }

        // GET: Costcenters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Costcenters == null)
            {
                return NotFound();
            }

            var costcenter = await _context.Costcenters.FindAsync(id);
            if (costcenter == null)
            {
                return NotFound();
            }

            // Verify the cost center belongs to the current user
            if (costcenter.UserId != userId)
            {
                return Forbid();
            }

            // Get only branches that belong to the logged-in user
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", costcenter.BranchId);
            return View(costcenter);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CostCenterId,CenterName,BranchId,CreatedAt")] Costcenter costcenter)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id != costcenter.CostCenterId)
            {
                return NotFound();
            }

            // Get the existing cost center from the database
            var existingCostCenter = await _context.Costcenters
                .FirstOrDefaultAsync(c => c.CostCenterId == id && c.UserId == userId);

            if (existingCostCenter == null)
            {
                return Forbid();
            }

            // Additional validation - check if the selected branch belongs to the user
            var userBranch = _context.Branches
                .Any(b => b.BranchId == costcenter.BranchId && b.UserId == userId);

            if (!userBranch)
            {
                ModelState.AddModelError("BranchId", "You don't have permission to use this branch");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only the properties that should change
                    existingCostCenter.CenterName = costcenter.CenterName;
                    existingCostCenter.BranchId = costcenter.BranchId;
                    // Preserve original CreatedAt and UserId
                    existingCostCenter.CreatedAt = existingCostCenter.CreatedAt;
                    existingCostCenter.UserId = userId.Value;

                    _context.Update(existingCostCenter);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CostcenterExists(costcenter.CostCenterId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed; redisplay form with only user's branches
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", costcenter.BranchId);
            return View(costcenter);
        }
        // GET: Costcenters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (id == null || _context.Costcenters == null)
            {
                return NotFound();
            }

            // Only find cost centers that belong to the current user
            var costcenter = await _context.Costcenters
                .Include(c => c.Branch)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CostCenterId == id && c.UserId == userId);

            if (costcenter == null)
            {
                return NotFound();
            }

            return View(costcenter);
        }

        // POST: Costcenters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Costcenters == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Costcenters'  is null.");
            }
            var costcenter = await _context.Costcenters.FindAsync(id);
            if (costcenter != null)
            {
                _context.Costcenters.Remove(costcenter);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CostcenterExists(int id)
        {
          return (_context.Costcenters?.Any(e => e.CostCenterId == id)).GetValueOrDefault();
        }
    }
}
