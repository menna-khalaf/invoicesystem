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
    public class SitestatsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public SitestatsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Sitestats
        public async Task<IActionResult> Index()
        {
              return _context.Sitestats != null ? 
                          View(await _context.Sitestats.ToListAsync()) :
                          Problem("Entity set 'InvoiceSystemrahtkContext.Sitestats'  is null.");
        }

        // GET: Sitestats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sitestats == null)
            {
                return NotFound();
            }

            var sitestat = await _context.Sitestats
                .FirstOrDefaultAsync(m => m.StatId == id);
            if (sitestat == null)
            {
                return NotFound();
            }

            return View(sitestat);
        }

        // GET: Sitestats/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sitestats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatId,PageName,Views,Visitors,LastUpdated")] Sitestat sitestat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sitestat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sitestat);
        }

        // GET: Sitestats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sitestats == null)
            {
                return NotFound();
            }

            var sitestat = await _context.Sitestats.FindAsync(id);
            if (sitestat == null)
            {
                return NotFound();
            }
            return View(sitestat);
        }

        // POST: Sitestats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatId,PageName,Views,Visitors,LastUpdated")] Sitestat sitestat)
        {
            if (id != sitestat.StatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sitestat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SitestatExists(sitestat.StatId))
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
            return View(sitestat);
        }

        // GET: Sitestats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sitestats == null)
            {
                return NotFound();
            }

            var sitestat = await _context.Sitestats
                .FirstOrDefaultAsync(m => m.StatId == id);
            if (sitestat == null)
            {
                return NotFound();
            }

            return View(sitestat);
        }

        // POST: Sitestats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sitestats == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Sitestats'  is null.");
            }
            var sitestat = await _context.Sitestats.FindAsync(id);
            if (sitestat != null)
            {
                _context.Sitestats.Remove(sitestat);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SitestatExists(int id)
        {
          return (_context.Sitestats?.Any(e => e.StatId == id)).GetValueOrDefault();
        }
    }
}
