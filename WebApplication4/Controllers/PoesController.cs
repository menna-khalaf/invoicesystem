using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class PoesController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public PoesController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Retrieve POs for the logged-in user, including related data
            var pos = await _context.Pos
                .Where(p => p.UserId == userId)
                .Include(p => p.Branch)
                .Include(p => p.CashAccount)
                .Include(p => p.BankAccount)
                .Include(p => p.Storehouse)
                .Select(p => new
                {
                    p.Posid,
                    p.Posname,
                    p.Status,
                    p.ReferenceNumber,
                    p.Notes,
                    p.CreatedAt,
                    BranchName = p.Branch.BranchName,
                    CashAccountName = p.CashAccount.AccountName,
                    BankAccountName = p.BankAccount.AccountName,
                    StorehouseName = p.Storehouse.StorehouseName,
                    HasAttachment = !string.IsNullOrEmpty(p.Attachments)
                })
                .ToListAsync();

            return View(pos);
        }

        // GET: Poes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pos == null)
            {
                return NotFound();
            }

            var po = await _context.Pos
                .Include(p => p.BankAccount)
                .Include(p => p.Branch)
                .Include(p => p.CashAccount)
                .Include(p => p.Storehouse)
                .FirstOrDefaultAsync(m => m.Posid == id);
            if (po == null)
            {
                return NotFound();
            }

            return View(po);
        }

        // GET: Po/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Populate dropdowns with only user-specific data
            ViewData["BankAccountId"] = new SelectList(
                _context.Cashandbankaccounts.Where(a => a.AccountType == "حساب بنكي" && a.UserId == userId),
                "AccountId", "AccountName");

            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId),
                "BranchId", "BranchName");

            ViewData["CashAccountId"] = new SelectList(
                _context.Cashandbankaccounts.Where(a => a.AccountType == "خزينة" && a.UserId == userId),
                "AccountId", "AccountName");

            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId),
                "StorehouseId", "StorehouseName");

            return View();
        }

        // POST: Po/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Posid,BranchId,Status,Posname,CashAccountId,BankAccountId,StorehouseId,ReferenceNumber,Notes,CreatedAt")] Po po, IFormFile Attachments)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Validate that all selected entities belong to the user
            var isValidBranch = await _context.Branches.AnyAsync(b => b.BranchId == po.BranchId && b.UserId == userId);
            var isValidStorehouse = await _context.Storehouses.AnyAsync(s => s.StorehouseId == po.StorehouseId && s.UserId == userId);
            var isValidBankAccount = await _context.Cashandbankaccounts.AnyAsync(a => a.AccountId == po.BankAccountId && a.UserId == userId && a.AccountType == "حساب بنكي");
            var isValidCashAccount = await _context.Cashandbankaccounts.AnyAsync(a => a.AccountId == po.CashAccountId && a.UserId == userId && a.AccountType == "خزينة");

            if (!isValidBranch)
                ModelState.AddModelError("BranchId", "Invalid branch selected");
            if (!isValidStorehouse)
                ModelState.AddModelError("StorehouseId", "Invalid storehouse selected");
            if (!isValidBankAccount)
                ModelState.AddModelError("BankAccountId", "Invalid bank account selected");
            if (!isValidCashAccount)
                ModelState.AddModelError("CashAccountId", "Invalid cash account selected");

            if (ModelState.IsValid)
            {
                // Handle file upload (optional)
                if (Attachments != null && Attachments.Length > 0)
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Attachments.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Attachments.CopyToAsync(stream);
                    }

                    // Save file information as JSON
                    var attachmentsJson = new
                    {
                        FileName = Attachments.FileName,
                        FilePath = filePath,
                        FileSize = Attachments.Length,
                        UploadedOn = DateTime.Now
                    };

                    po.Attachments = JsonConvert.SerializeObject(attachmentsJson);
                }
                else
                {
                    po.Attachments = null;
                }

                // Set creation timestamp and user ID
                po.CreatedAt = DateTime.Now;
                po.UserId = userId.Value;

                // Save to database
                _context.Add(po);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns with user-specific data if validation fails
            ViewData["BankAccountId"] = new SelectList(
                _context.Cashandbankaccounts.Where(a => a.AccountType == "حساب بنكي" && a.UserId == userId),
                "AccountId", "AccountName", po.BankAccountId);

            ViewData["BranchId"] = new SelectList(
                _context.Branches.Where(b => b.UserId == userId),
                "BranchId", "BranchName", po.BranchId);

            ViewData["CashAccountId"] = new SelectList(
                _context.Cashandbankaccounts.Where(a => a.AccountType == "خزينة" && a.UserId == userId),
                "AccountId", "AccountName", po.CashAccountId);

            ViewData["StorehouseId"] = new SelectList(
                _context.Storehouses.Where(s => s.UserId == userId),
                "StorehouseId", "StorehouseName", po.StorehouseId);

            return View(po);
        }
        // GET: Poes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pos == null)
            {
                return NotFound();
            }

            var po = await _context.Pos.FindAsync(id);
            if (po == null)
            {
                return NotFound();
            }
            ViewData["BankAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", po.BankAccountId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", po.BranchId);
            ViewData["CashAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", po.CashAccountId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", po.StorehouseId);
            return View(po);
        }

        // POST: Poes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Posid,BranchId,Status,Posname,CashAccountId,BankAccountId,StorehouseId,ReferenceNumber,Notes,Attachments,CreatedAt")] Po po)
        {
            if (id != po.Posid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(po);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PoExists(po.Posid))
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
            ViewData["BankAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", po.BankAccountId);
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", po.BranchId);
            ViewData["CashAccountId"] = new SelectList(_context.Cashandbankaccounts, "AccountId", "AccountName", po.CashAccountId);
            ViewData["StorehouseId"] = new SelectList(_context.Storehouses, "StorehouseId", "StorehouseName", po.StorehouseId);
            return View(po);
        }

        // GET: Poes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pos == null)
            {
                return NotFound();
            }

            var po = await _context.Pos
                .Include(p => p.BankAccount)
                .Include(p => p.Branch)
                .Include(p => p.CashAccount)
                .Include(p => p.Storehouse)
                .FirstOrDefaultAsync(m => m.Posid == id);
            if (po == null)
            {
                return NotFound();
            }

            return View(po);
        }

        // POST: Poes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pos == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Pos'  is null.");
            }
            var po = await _context.Pos.FindAsync(id);
            if (po != null)
            {
                _context.Pos.Remove(po);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PoExists(int id)
        {
          return (_context.Pos?.Any(e => e.Posid == id)).GetValueOrDefault();
        }










        // GET: Transfer/Create
        public async Task<IActionResult> PoesTransfer()
        {
            // Get current account (you'll need to implement GetCurrentAccountId based on your auth system)
            var currentAccount = await _context.Cashandbankaccounts
                .FirstOrDefaultAsync(a => a.AccountId == GetCurrentAccountId());

            if (currentAccount == null)
            {
                return NotFound();
            }

            ViewBag.CurrentBalance = currentAccount.Balance;
            ViewBag.TreasuryAccounts = await _context.Cashandbankaccounts
                .Where(a => a.AccountType == "Treasury" && a.AccountId != currentAccount.AccountId)
                .ToListAsync();
            ViewBag.EmployeeAccounts = await _context.Cashandbankaccounts
                .Where(a => a.AccountType == "Employee" && a.AccountId != currentAccount.AccountId)
                .Include(a => a.Employee)
                .ToListAsync();

            return View();
        }

        // POST: Transfer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PoesTransfer(string transferType, int targetAccountId, decimal amount)
        {
            // Get source account
            var sourceAccount = await _context.Cashandbankaccounts
                .FirstOrDefaultAsync(a => a.AccountId == GetCurrentAccountId());

            if (sourceAccount == null)
            {
                return NotFound();
            }

            // Validate amount
            if (amount <= 0)
            {
                ModelState.AddModelError("Amount", "قيمة التحويل يجب أن تكون أكبر من الصفر");
                return await ReloadCreateView(sourceAccount);
            }

            if (sourceAccount.Balance < amount)
            {
                ModelState.AddModelError("Amount", "الرصيد غير كافي");
                return await ReloadCreateView(sourceAccount);
            }

            // Get target account
            var targetAccount = await _context.Cashandbankaccounts
                .FirstOrDefaultAsync(a => a.AccountId == targetAccountId);

            if (targetAccount == null)
            {
                ModelState.AddModelError("TargetAccount", "الحساب الهدف غير موجود");
                return await ReloadCreateView(sourceAccount);
            }

            // Perform transfer
            sourceAccount.Balance -= amount;
            targetAccount.Balance += amount;

            // Create transfer record
            var transfer = new Transfer
            {
                FromAccountId = sourceAccount.AccountId,
                ToAccountId = targetAccount.AccountId,
                Amount = amount,
                TransferDate = DateTime.Now
            };

            _context.Transfers.Add(transfer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }

        private async Task<IActionResult> ReloadCreateView(Cashandbankaccount currentAccount)
        {
            ViewBag.CurrentBalance = currentAccount.Balance;
            ViewBag.TreasuryAccounts = await _context.Cashandbankaccounts
                .Where(a => a.AccountType == "Treasury" && a.AccountId != currentAccount.AccountId)
                .ToListAsync();
            ViewBag.EmployeeAccounts = await _context.Cashandbankaccounts
                .Where(a => a.AccountType == "Employee" && a.AccountId != currentAccount.AccountId)
                .Include(a => a.Employee)
                .ToListAsync();

            return View("Create");
        }

        private int GetCurrentAccountId()
        {
            // Implement based on your authentication system
            // This is just a placeholder
            return 1; // Replace with actual implementation
        }




















        [HttpGet]
        public async Task<IActionResult> GetActivePosList()
        {
            // Get the logged-in user's ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Filter the POS list by UserId
            var posList = await _context.Pos
                .Where(p => p.UserId == userId)
                .Select(p => new { Id = p.Posid, Posname = p.Posname }) // Return only necessary data
                .ToListAsync();

            return View(posList); // Or return Json(posList) if this is for an API endpoint or AJAX
        }







        [HttpGet]
        public async Task<IActionResult> PosCashAccountDetails(int posId)
        {
            var posData = await _context.Pos
                .Include(p => p.CashAccount)
                .Include(p => p.Branch)
                .Include(p => p.Storehouse)
                .Where(p => p.Posid == posId)
                .Select(p => new PosCashAccountViewModel
                {
                    PosId = p.Posid,
                    PosName = p.Posname,
                    BranchName = p.Branch.BranchName,
                    BranchId = p.BranchId,
                    CashAccountName = p.CashAccount.AccountName,
                    CashAccountId = p.CashAccount.AccountId,
                    BankAccountId =p.BankAccount.AccountId,
                    ReferenceNumber = p.ReferenceNumber,
                    CurrentBalance = p.CashAccount.Balance,
                    //Currency = p.CashAccount.Currency,
                    StorehouseId = p.StorehouseId ?? 0
                    
                })
                .FirstOrDefaultAsync();

            if (posData == null || posData.StorehouseId == 0)
            {
                return NotFound("POS or Storehouse not found");
            }

            // Store CashAccountId in session
            HttpContext.Session.SetInt32("CashAccountId", posData.CashAccountId);
            HttpContext.Session.SetInt32("BankAccountId", posData.BankAccountId);
            return View(posData);
        }


        [HttpPost]
        public async Task<IActionResult> CreatePosSession([FromBody] PosSessionRequest request)
        {
            try
            {
                // Validate all required fields
                if (request.PosId <= 0 || request.StorehouseId <= 0 || request.BranchId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "بيانات الجلسة غير مكتملة"
                    });
                }

                var newSession = new PosSession
                {
                    Posid = request.PosId,
                    StorehouseId = request.StorehouseId,
                    BranchId = request.BranchId,
                    StartTime = DateTime.Now,
                    StartingCash = request.StartingCash,
                    Status = "Active"
                };

                _context.PosSessions.Add(newSession);
                await _context.SaveChangesAsync(); // This is where the SessionId gets generated

                // Now newSession.SessionId contains the auto-incremented value from DB

                // Update POS with session reference
                var pos = await _context.Pos.FindAsync(request.PosId);
                if (pos != null)
                {
                    pos.SessionId = newSession.SessionId;
                    await _context.SaveChangesAsync();
                }

                // Store the SessionId in ASP.NET session state
                HttpContext.Session.SetInt32("CurrentPosSessionId", newSession.SessionId);

                return Json(new
                {
                    success = true,
                    sessionId = newSession.SessionId,
                    redirectUrl = Url.Action("Create", "PosSessions", new
                    {
                        posId = request.PosId,
                        sessionId = newSession.SessionId
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"حدث خطأ: {ex.Message}"
                });
            }
        }

        public class PosSessionRequest
        {
            public int PosId { get; set; }
            public int StorehouseId { get; set; }
            public int BranchId { get; set; }
            public decimal StartingCash { get; set; }
        }














        // Add this action to your controller
        public async Task<IActionResult> ShiftReport(int sessionId)
        {
            // Get all invoices for this session
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Employee)
                .Where(i => i.SessionId == sessionId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            // Get session details
            var session = await _context.PosSessions
                .Include(s => s.Employee)
                .Include(s => s.Storehouse)
                .Include(s => s.Branch)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                return NotFound();
            }

            ViewBag.SessionInfo = session;
            return View(invoices);
        }

    }
}
