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
    public class PosTransactionsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public PosTransactionsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: PosTransactions
        public async Task<IActionResult> Index()
        {
            var invoiceSystemrahtkContext = _context.PosTransactions.Include(p => p.Employee).Include(p => p.Invoice);
            return View(await invoiceSystemrahtkContext.ToListAsync());
        }

        // GET: PosTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PosTransactions == null)
            {
                return NotFound();
            }

            var posTransaction = await _context.PosTransactions
                .Include(p => p.Employee)
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (posTransaction == null)
            {
                return NotFound();
            }

            return View(posTransaction);
        }

        // GET: PosTransactions/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName");
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId");
            return View();
        }

        // POST: PosTransactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,EmployeeId,InvoiceId,PaymentMethod,AmountPaid,CreatedAt")] PosTransaction posTransaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(posTransaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", posTransaction.EmployeeId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", posTransaction.InvoiceId);
            return View(posTransaction);
        }

        // GET: PosTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PosTransactions == null)
            {
                return NotFound();
            }

            var posTransaction = await _context.PosTransactions.FindAsync(id);
            if (posTransaction == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", posTransaction.EmployeeId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", posTransaction.InvoiceId);
            return View(posTransaction);
        }

        // POST: PosTransactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,EmployeeId,InvoiceId,PaymentMethod,AmountPaid,CreatedAt")] PosTransaction posTransaction)
        {
            if (id != posTransaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(posTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PosTransactionExists(posTransaction.TransactionId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", posTransaction.EmployeeId);
            ViewData["InvoiceId"] = new SelectList(_context.Invoices, "InvoiceId", "InvoiceId", posTransaction.InvoiceId);
            return View(posTransaction);
        }

        // GET: PosTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PosTransactions == null)
            {
                return NotFound();
            }

            var posTransaction = await _context.PosTransactions
                .Include(p => p.Employee)
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (posTransaction == null)
            {
                return NotFound();
            }

            return View(posTransaction);
        }

        // POST: PosTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PosTransactions == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.PosTransactions'  is null.");
            }
            var posTransaction = await _context.PosTransactions.FindAsync(id);
            if (posTransaction != null)
            {
                _context.PosTransactions.Remove(posTransaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PosTransactionExists(int id)
        {
          return (_context.PosTransactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }
    }
}
