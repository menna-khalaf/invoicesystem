using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class CashandbankaccountsController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public CashandbankaccountsController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        // GET: Cashandbankaccounts
        public IActionResult Index()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only "حساب بنكي" accounts for the current user with related data
            var accounts = _context.Cashandbankaccounts
                .Include(c => c.Branch)
                .Include(c => c.CountryCurrency)
                .Include(c => c.Employee)
                .Where(c => c.UserId == userId.Value && c.AccountType == "حساب بنكي")
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            // Calculate total balance
            var totalBalance = accounts.Sum(a => a.Balance);

            ViewBag.TotalBalance = totalBalance;

            return View(accounts);
        }


        // GET: Cashandbankaccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cashandbankaccounts == null)
            {
                return NotFound();
            }

            var cashandbankaccount = await _context.Cashandbankaccounts
                .Include(c => c.Branch)
                .Include(c => c.CountryCurrency)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (cashandbankaccount == null)
            {
                return NotFound();
            }

            return View(cashandbankaccount);
        }

        // GET: Cashandbankaccounts/Create
        public IActionResult Create()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Redirect to login if no user is logged in
                return RedirectToAction("Login", "Account");
            }

            // Filter branches by the current user's ID
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId.Value)
                .ToList();

            // Filter employees who belong to the current user
            var userEmployees = _context.Employees
                .Where(e => e.UserId == userId.Value) // Changed to check UserId directly
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName");
            ViewData["CountryCurrencyId"] = new SelectList(_context.Countrycurrencies, "CountryCurrencyId", "CountryName");
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName");

            // Set default value for AccountType and UserId from session
            var model = new Cashandbankaccount
            {
                AccountType = "حساب بنكي",
                UserId = userId.Value
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,AccountNumber,CountryCurrencyId,IsAccountingCodeEnabled,Description,AccountName,AccountType,Status,EmployeeId,BranchId,Balance,CreatedAt,ShortName,Iban,SwiftCode,UserId,ChildAccountId")] Cashandbankaccount cashandbankaccount)
        {
            // Get UserId from session to ensure it matches
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
            {
                Console.WriteLine("Error: UserId not found in session.");
                return RedirectToAction("Login", "USERED");
            }

            // Override with session UserId to prevent tampering
            cashandbankaccount.UserId = sessionUserId.Value;

            // Set CreatedAt
            cashandbankaccount.CreatedAt = DateTime.Now;

            // Restrict to حساب بنكي only
            if (cashandbankaccount.AccountType != "حساب بنكي")
            {
                ModelState.AddModelError("AccountType", "هذا الإجراء مخصص لإنشاء حسابات بنكية فقط.");
            }

            // Validate Balance
            if (cashandbankaccount.Balance < 0)
            {
                ModelState.AddModelError("Balance", "الرصيد الافتتاحي لا يمكن أن يكون سالبًا.");
            }

            // Validate EmployeeId
            var userEmployee = await _context.Employees
                .AnyAsync(e => e.EmployeeId == cashandbankaccount.EmployeeId && e.UserId == sessionUserId);
            if (cashandbankaccount.EmployeeId.HasValue && !userEmployee)
            {
                ModelState.AddModelError("EmployeeId", "لا يمكنك استخدام هذا الموظف.");
            }

            // Validate BranchId
            var userBranch = await _context.Branches
                .AnyAsync(b => b.BranchId == cashandbankaccount.BranchId && b.UserId == sessionUserId);
            if (cashandbankaccount.BranchId.HasValue && !userBranch)
            {
                ModelState.AddModelError("BranchId", "لا يمكنك استخدام هذا الفرع.");
            }

            // Set ChildAccountId for حساب بنكي
            const int bankChildAccountId = 2; // النقدية بالبنك
            var bankAccountMapping = await _context.AccountMappings
                .AsNoTracking()
                .AnyAsync(m => m.TransactionType == "النقدية بالبنك" && m.ChildAccountId == bankChildAccountId);
            if (!bankAccountMapping)
            {
                Console.WriteLine("Error: النقدية بالبنك account not defined in account_mappings.");
                ModelState.AddModelError("AccountType", "حساب النقدية بالبنك غير معرف في تعيينات الحسابات.");
            }
            else
            {
                cashandbankaccount.ChildAccountId = bankChildAccountId;
            }

            // Validate Capital account (رأس المال)
            const int capitalAccountId = 12; // رأس المال
                                             // Note: If رأس المال uses a different child_account_id, replace '12' with the correct ID.
            var capitalAccountMapping = await _context.AccountMappings
                .AsNoTracking()
                .AnyAsync(m => m.TransactionType == "رأس المال" && m.ChildAccountId == capitalAccountId);
            if (!capitalAccountMapping)
            {
                Console.WriteLine("Error: رأس المال account not defined in account_mappings.");
                ModelState.AddModelError("", "حساب رأس المال غير معرف في تعيينات الحسابات.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Add Cashandbankaccount
                    _context.Add(cashandbankaccount);
                    await _context.SaveChangesAsync(); // Save to get AccountId
                    Console.WriteLine($"Created Cashandbankaccount: AccountId={cashandbankaccount.AccountId}, AccountName={cashandbankaccount.AccountName}, AccountType={cashandbankaccount.AccountType}, Balance={cashandbankaccount.Balance}, ChildAccountId={cashandbankaccount.ChildAccountId}");

                    // Create journal entries for opening balance
                    var journalEntries = new List<JournalEntry>();
                    if (cashandbankaccount.Balance > 0)
                    {
                        // Debit: Bank Account
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد افتتاحي لـ حساب بنكي - {cashandbankaccount.AccountName}",
                            DebitAmount = cashandbankaccount.Balance,
                            CreditAmount = 0,
                            UserId = cashandbankaccount.UserId!.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = null,
                            InvoiceType = "رصيد افتتاحي",
                            ChildAccountId = cashandbankaccount.ChildAccountId // 2
                        });

                        // Credit: Capital (رأس المال)
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد افتتاحي لـ حساب بنكي - {cashandbankaccount.AccountName}",
                            DebitAmount = 0,
                            CreditAmount = cashandbankaccount.Balance,
                            UserId = cashandbankaccount.UserId!.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = null,
                            InvoiceType = "رصيد افتتاحي",
                            ChildAccountId = capitalAccountId // 12
                        });
                    }

                    // Log journal entries
                    Console.WriteLine($"Adding {journalEntries.Count} JournalEntries to context");
                    foreach (var entry in journalEntries)
                    {
                        Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceType={entry.InvoiceType}");
                    }
                    _context.JournalEntries.AddRange(journalEntries);

                    // Save changes
                    Console.WriteLine("Calling SaveChangesAsync...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("SaveChangesAsync completed.");

                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed.");

                    // Log saved journal entries
                    var savedJournalEntries = await _context.JournalEntries
                        .AsNoTracking()
                        .Where(j => j.UserId == cashandbankaccount.UserId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now) && j.InvoiceType == "رصيد افتتاحي")
                        .ToListAsync();
                    Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                    foreach (var entry in savedJournalEntries)
                    {
                        Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, InvoiceType={entry.InvoiceType}");
                    }

                    return RedirectToAction("Index", "Cashandbankaccounts");
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"DbUpdateException in Cashandbankaccounts Create POST: {ex.Message}\n{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                    }
                    ModelState.AddModelError("", "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Exception in Cashandbankaccounts Create POST: Type={ex.GetType().Name}, Message={ex.Message}\n{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: Type={ex.InnerException.GetType().Name}, Message={ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                    }
                    ModelState.AddModelError("", "حدث خطأ أثناء الحفظ.");
                }
            }

            // Repopulate dropdowns if validation fails
            var userBranches = await _context.Branches
                .Where(b => b.UserId == sessionUserId)
                .ToListAsync();
            var userEmployees = await _context.Employees
                .Where(e => e.UserId == sessionUserId)
                .ToListAsync();
            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", cashandbankaccount.BranchId);
            ViewData["CountryCurrencyId"] = new SelectList(_context.Countrycurrencies, "CountryCurrencyId", "CountryName", cashandbankaccount.CountryCurrencyId);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", cashandbankaccount.EmployeeId);

            return View(cashandbankaccount);
        }

        // GET: Cashandbankaccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cashandbankaccounts == null)
            {
                return NotFound();
            }

            var cashandbankaccount = await _context.Cashandbankaccounts.FindAsync(id);
            if (cashandbankaccount == null)
            {
                return NotFound();
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", cashandbankaccount.BranchId);
            ViewData["CountryCurrencyId"] = new SelectList(_context.Countrycurrencies, "CountryCurrencyId", "CountryName", cashandbankaccount.CountryCurrencyId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", cashandbankaccount.EmployeeId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", cashandbankaccount.UserId);
            return View(cashandbankaccount);
        }

        // POST: Cashandbankaccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Cashandbankaccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,AccountNumber,CountryCurrencyId,IsAccountingCodeEnabled,Description,AccountName,AccountType,Status,EmployeeId,BranchId,Balance,CreatedAt,ShortName,Iban,SwiftCode,UserId")] Cashandbankaccount cashandbankaccount)
        {
            if (id != cashandbankaccount.AccountId)
            {
                return NotFound();
            }

            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "USERED");
            }

            // Override with session UserId to prevent tampering
            cashandbankaccount.UserId = sessionUserId.Value;

            // Retrieve existing record to preserve CreatedAt and validate ownership
            var existingAccount = await _context.Cashandbankaccounts
                .FirstOrDefaultAsync(a => a.AccountId == id && a.UserId == sessionUserId.Value);

            if (existingAccount == null)
            {
                return NotFound();
            }

            // Preserve CreatedAt
            cashandbankaccount.CreatedAt = existingAccount.CreatedAt;

            // Additional validation - check if the selected employee belongs to the user
            var userEmployee = await _context.Employees
                .AnyAsync(e => e.EmployeeId == cashandbankaccount.EmployeeId && e.UserId == sessionUserId);

            if (!userEmployee)
            {
                ModelState.AddModelError("EmployeeId", "You don't have permission to use this employee");
            }

            // Additional validation - check if the selected branch belongs to the user
            var userBranch = await _context.Branches
                .AnyAsync(b => b.BranchId == cashandbankaccount.BranchId && b.UserId == sessionUserId);

            if (!userBranch)
            {
                ModelState.AddModelError("BranchId", "You don't have permission to use this branch");
            }

            // Validate CountryCurrencyId
            var currencyExists = await _context.Countrycurrencies
                .AnyAsync(c => c.CountryCurrencyId == cashandbankaccount.CountryCurrencyId);

            if (!currencyExists)
            {
                ModelState.AddModelError("CountryCurrencyId", "Invalid currency selected.");
            }

            // Validate required fields (e.g., Status, AccountName, AccountType)
            if (string.IsNullOrEmpty(cashandbankaccount.Status))
            {
                ModelState.AddModelError("Status", "Status is required.");
            }

            if (string.IsNullOrEmpty(cashandbankaccount.AccountName))
            {
                ModelState.AddModelError("AccountName", "Account Name is required.");
            }

            if (string.IsNullOrEmpty(cashandbankaccount.AccountType))
            {
                ModelState.AddModelError("AccountType", "Account Type is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(existingAccount).CurrentValues.SetValues(cashandbankaccount);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Cashandbankaccounts");
                }
                catch (DbUpdateException ex)
                {
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    System.Diagnostics.Debug.WriteLine($"DbUpdateException: {errorMessage}");
                    ModelState.AddModelError("", $"Database error: {errorMessage}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex}");
                    ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                }
            }

            // Repopulate dropdowns if validation fails
            var userBranches = _context.Branches
                .Where(b => b.UserId == sessionUserId)
                .ToList();

            var userEmployees = _context.Employees
                .Where(e => e.UserId == sessionUserId)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", cashandbankaccount.BranchId);
            ViewData["CountryCurrencyId"] = new SelectList(_context.Countrycurrencies, "CountryCurrencyId", "CountryName", cashandbankaccount.CountryCurrencyId);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", cashandbankaccount.EmployeeId);

            return View(cashandbankaccount);
        }

        // GET: Cashandbankaccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Cashandbankaccounts == null)
            {
                return NotFound();
            }

            var cashandbankaccount = await _context.Cashandbankaccounts
                .Include(c => c.Branch)
                .Include(c => c.CountryCurrency)
                .Include(c => c.Employee)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (cashandbankaccount == null)
            {
                return NotFound();
            }

            return View(cashandbankaccount);
        }

        // POST: Cashandbankaccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Cashandbankaccounts == null)
            {
                return Problem("Entity set 'InvoiceSystemrahtkContext.Cashandbankaccounts'  is null.");
            }
            var cashandbankaccount = await _context.Cashandbankaccounts.FindAsync(id);
            if (cashandbankaccount != null)
            {
                _context.Cashandbankaccounts.Remove(cashandbankaccount);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CashandbankaccountExists(int id)
        {
          return (_context.Cashandbankaccounts?.Any(e => e.AccountId == id)).GetValueOrDefault();
        }












        // Add this to your CashandbankaccountsController
        [HttpGet]
        public IActionResult GetCurrencyByCountry(int countryId)
        {
            var countryCurrency = _context.Countrycurrencies
                .FirstOrDefault(cc => cc.CountryCurrencyId == countryId);

            if (countryCurrency != null)
            {
                return Json(new
                {
                    currencyName = countryCurrency.CurrencyName,
                    currencySymbol = countryCurrency.CurrencySymbol
                });
            }

            return NotFound();
        }













        // GET: Cashandbankaccounts/CreateStore
        public IActionResult CreateStore()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                Console.WriteLine("Error: UserId not found in session.");
                return RedirectToAction("Login", "usered");
            }

            // Filter branches by the current user's ID
            var userBranches = _context.Branches
                .Where(b => b.UserId == userId.Value)
                .ToList();

            // Filter employees who belong to the current user
            var userEmployees = _context.Employees
                .Where(e => e.UserId == userId.Value)
                .ToList();

            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName");
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName");

            // Set default values for cash account
            var model = new Cashandbankaccount
            {
                AccountType = "خزينة",
                Status = "نشط",
                Balance = 0.00m,
                CreatedAt = DateTime.Now,
                UserId = userId.Value,
                ChildAccountId = 1 // النقدية بالصندوق
            };

            // Indicate cash account only
            ViewBag.IsCashAccountOnly = true;

            return View(model);
        }

        // POST: Cashandbankaccounts/CreateStore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStore(
            [Bind("AccountId,AccountNumber,AccountName,BranchID,Balance,Status,EmployeeID,Description,CreatedAt,UserID,child_account_id")] Cashandbankaccount cashandbankaccount)
        {
            // Get UserId from session
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
            {
                Console.WriteLine("Error: UserId not found in session.");
                return RedirectToAction("Login", "Account");
            }

            // Log form data for debugging
            Console.WriteLine($"Form Data: AccountName={cashandbankaccount.AccountName}, AccountNumber={cashandbankaccount.AccountNumber}, Balance={cashandbankaccount.Balance}, AccountType={cashandbankaccount.AccountType}, UserID={cashandbankaccount.UserId}, child_account_id={cashandbankaccount.ChildAccountId}, BranchID={cashandbankaccount.BranchId}, EmployeeID={cashandbankaccount.EmployeeId}, Description={cashandbankaccount.Description}, Status={cashandbankaccount.Status}, CreatedAt={cashandbankaccount.CreatedAt}");

            // Override with session UserId to prevent tampering
            cashandbankaccount.UserId = sessionUserId.Value;

            // Set CreatedAt and AccountType
            cashandbankaccount.CreatedAt = DateTime.Now;
            cashandbankaccount.AccountType = "خزينة";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(cashandbankaccount.AccountName))
            {
                ModelState.AddModelError("AccountName", "اسم الحساب مطلوب.");
            }
            if (string.IsNullOrWhiteSpace(cashandbankaccount.AccountNumber))
            {
                ModelState.AddModelError("AccountNumber", "رقم الحساب مطلوب.");
            }
            else
            {
                // Check for duplicate AccountNumber
                var accountNumberExists = await _context.Cashandbankaccounts
                    .AnyAsync(a => a.AccountNumber == cashandbankaccount.AccountNumber);
                if (accountNumberExists)
                {
                    ModelState.AddModelError("AccountNumber", "رقم الحساب موجود بالفعل.");
                }
            }

            // Validate Balance
            if (cashandbankaccount.Balance < 0)
            {
                ModelState.AddModelError("Balance", "الرصيد الافتتاحي لا يمكن أن يكون سالبًا.");
            }

            // Validate EmployeeID
            if (cashandbankaccount.EmployeeId.HasValue)
            {
                var validEmployee = await _context.Employees
                    .AnyAsync(e => e.EmployeeId == cashandbankaccount.EmployeeId.Value && e.UserId == sessionUserId.Value);
                if (!validEmployee)
                {
                    ModelState.AddModelError("EmployeeID", "الموظف المحدد غير صالح.");
                }
            }

            // Validate BranchID
            if (cashandbankaccount.BranchId.HasValue)
            {
                var validBranch = await _context.Branches
                    .AnyAsync(b => b.BranchId == cashandbankaccount.BranchId.Value && b.UserId == sessionUserId.Value);
                if (!validBranch)
                {
                    ModelState.AddModelError("BranchID", "الفرع المحدد غير صالح.");
                }
            }

            // Validate and set child_account_id for خزينة
            const int cashChildAccountId = 1; // النقدية بالصندوق
            var cashAccountMapping = await _context.AccountMappings
                .AsNoTracking()
                .AnyAsync(m => m.TransactionType == "النقدية بالصندوق" && m.ChildAccountId == cashChildAccountId);
            if (!cashAccountMapping)
            {
                Console.WriteLine("Error: النقدية بالصندوق account not defined in account_mappings.");
                ModelState.AddModelError("AccountType", "حساب النقدية بالصندوق غير معرف في تعيينات الحسابات.");
            }
            else
            {
                cashandbankaccount.ChildAccountId = cashChildAccountId;
            }

            // Validate Capital account (رأس المال)
            const int capitalAccountId = 12; // رأس المال
            var capitalAccountMapping = await _context.AccountMappings
                .AsNoTracking()
                .AnyAsync(m => m.TransactionType == "رأس المال" && m.ChildAccountId == capitalAccountId);
            if (!capitalAccountMapping)
            {
                Console.WriteLine("Error: رأس المال account not defined in account_mappings.");
                ModelState.AddModelError("", "حساب رأس المال غير معرف في تعيينات الحسابات.");
            }

            // Log ModelState before checking IsValid
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors:");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error: Key={error.Key}, Error={err.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Log data before insert
                    Console.WriteLine($"Attempting to insert Cashandbankaccount: AccountName={cashandbankaccount.AccountName}, AccountNumber={cashandbankaccount.AccountNumber}, AccountType={cashandbankaccount.AccountType}, Balance={cashandbankaccount.Balance}, UserID={cashandbankaccount.UserId}, child_account_id={cashandbankaccount.ChildAccountId}");

                    // Add Cashandbankaccount
                    _context.Add(cashandbankaccount);
                    await _context.SaveChangesAsync(); // Save to get AccountId
                    Console.WriteLine($"Created Cashandbankaccount: AccountId={cashandbankaccount.AccountId}, AccountName={cashandbankaccount.AccountName}, AccountType={cashandbankaccount.AccountType}, Balance={cashandbankaccount.Balance}, child_account_id={cashandbankaccount.ChildAccountId}");

                    // Create journal entries for opening balance
                    var journalEntries = new List<JournalEntry>();
                    if (cashandbankaccount.Balance > 0)
                    {
                        // Debit: Cash Account
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد افتتاحي لـ خزينة - {cashandbankaccount.AccountName}",
                            DebitAmount = cashandbankaccount.Balance,
                            CreditAmount = 0,
                            UserId = cashandbankaccount.UserId!.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = null,
                            InvoiceType = "رصيد افتتاحي",
                            ChildAccountId = cashandbankaccount.ChildAccountId// 1
                        });

                        // Credit: Capital (رأس المال)
                        journalEntries.Add(new JournalEntry
                        {
                            EntryDate = DateOnly.FromDateTime(DateTime.Now),
                            Description = $"رصيد افتتاحي لـ خزينة - {cashandbankaccount.AccountName}",
                            DebitAmount = 0,
                            CreditAmount = cashandbankaccount.Balance,
                            UserId = cashandbankaccount.UserId!.Value,
                            CostCenterId = 0,
                            CreatedAt = DateTime.Now,
                            InvoiceId = null,
                            InvoiceType = "رصيد افتتاحي",
                            ChildAccountId = capitalAccountId // 12
                        });
                    }

                    // Log journal entries
                    Console.WriteLine($"Adding {journalEntries.Count} JournalEntries to context");
                    foreach (var entry in journalEntries)
                    {
                        Console.WriteLine($"JournalEntry: Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, UserId={entry.UserId}, InvoiceType={entry.InvoiceType}");
                    }
                    _context.JournalEntries.AddRange(journalEntries);

                    // Save changes
                    Console.WriteLine("Calling SaveChangesAsync for journal entries...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("SaveChangesAsync completed.");

                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed.");

                    // Log saved journal entries
                    var savedJournalEntries = await _context.JournalEntries
                        .AsNoTracking()
                        .Where(j => j.UserId == cashandbankaccount.UserId && j.EntryDate == DateOnly.FromDateTime(DateTime.Now) && j.InvoiceType == "رصيد افتتاحي")
                        .ToListAsync();
                    Console.WriteLine($"Saved JournalEntries count: {savedJournalEntries.Count}");
                    foreach (var entry in savedJournalEntries)
                    {
                        Console.WriteLine($"Saved JournalEntry: Id={entry.Id}, Description={entry.Description}, DebitAmount={entry.DebitAmount}, CreditAmount={entry.CreditAmount}, ChildAccountId={entry.ChildAccountId}, InvoiceType={entry.InvoiceType}");
                    }

                    // Log childbalances to confirm dual effect
                    var childBalances = await _context.Childbalances
                        .AsNoTracking()
                        .Where(cb => cb.UserId == cashandbankaccount.UserId && new[] { cashChildAccountId, capitalAccountId }.Contains(cb.ChildAccountId))
                        .ToListAsync();
                    Console.WriteLine($"Childbalances count: {childBalances.Count}");
                    foreach (var balance in childBalances)
                    {
                        Console.WriteLine($"Childbalance: UserId={balance.UserId}, ChildAccountId={balance.ChildAccountId}, Balance={balance.Balance:F2}");
                    }

                    return RedirectToAction("IndexStore", "Cashandbankaccounts");
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"DbUpdateException in Cashandbankaccounts CreateStore POST: {ex.Message}\n{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                    }
                    ModelState.AddModelError("", $"حدث خطأ أثناء حفظ البيانات: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Exception in Cashandbankaccounts CreateStore POST: Type={ex.GetType().Name}, Message={ex.Message}\n{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: Type={ex.InnerException.GetType().Name}, Message={ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                    }
                    ModelState.AddModelError("", $"حدث خطأ غير متوقع: {ex.Message}");
                }
            }

            // Repopulate dropdowns if validation fails
            var userBranches = await _context.Branches
                .Where(b => b.UserId == sessionUserId)
                .ToListAsync();
            var userEmployees = await _context.Employees
                .Where(e => e.UserId == sessionUserId)
                .ToListAsync();
            ViewData["BranchId"] = new SelectList(userBranches, "BranchId", "BranchName", cashandbankaccount.BranchId);
            ViewData["EmployeeId"] = new SelectList(userEmployees, "EmployeeId", "FullName", cashandbankaccount.EmployeeId);
            return RedirectToAction("IndexStore", "CashAndBankAccount");

        }



        // GET: Cashandbankaccounts/IndexStore
        public IActionResult IndexStore()
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get only "خزينة" accounts for the current user with related data
            var accounts = _context.Cashandbankaccounts
                .Include(c => c.Branch)
                .Include(c => c.CountryCurrency)
                .Include(c => c.Employee)
                .Where(c => c.UserId == userId.Value && c.AccountType == "خزينة")
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            // Calculate total balance
            var totalBalance = accounts.Sum(a => a.Balance);

            ViewBag.TotalBalance = totalBalance;

            return View(accounts);
        }

        // GET: Cashandbankaccounts/JournalEntries/{accountId}
        public async Task<IActionResult> JournalEntries(int accountId)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Fetch the account to ensure it belongs to the user
            var account = await _context.Cashandbankaccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId.Value);

            if (account == null)
            {
                return NotFound("الحساب غير موجود أو لا ينتمي إلى المستخدم الحالي.");
            }

            // Fetch journal entries related to the account
            var journalEntries = await _context.JournalEntries
                .AsNoTracking()
                .Where(j => j.UserId == userId.Value && j.ChildAccountId == account.ChildAccountId)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();

            ViewBag.AccountName = account.AccountName;
            ViewBag.AccountId = account.AccountId;

            return View(journalEntries);
        }

        // GET: Cashandbankaccounts/DownloadJournalEntriesPdf/{accountId}
        public async Task<IActionResult> DownloadJournalEntriesPdf(int accountId)
        {
            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Fetch the account to ensure it belongs to the user
            var account = await _context.Cashandbankaccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId.Value);

            if (account == null)
            {
                return NotFound("الحساب غير موجود أو لا ينتمي إلى المستخدم الحالي.");
            }

            // Fetch journal entries related to the account
            var journalEntries = await _context.JournalEntries
                .AsNoTracking()
                .Where(j => j.UserId == userId.Value && j.ChildAccountId == account.ChildAccountId)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();

            // Generating LaTeX content for the PDF
            var latexBuilder = new StringBuilder();
            latexBuilder.AppendLine(@"\documentclass[12pt]{article}");
            latexBuilder.AppendLine(@"\usepackage{geometry}");
            latexBuilder.AppendLine(@"\geometry{a4paper, margin=1in}");
            latexBuilder.AppendLine(@"\usepackage[utf8]{inputenc}");
            latexBuilder.AppendLine(@"\usepackage{amsmath}");
            latexBuilder.AppendLine(@"\usepackage{booktabs}");
            latexBuilder.AppendLine(@"\usepackage{longtable}");
            latexBuilder.AppendLine(@"\usepackage{pdflscape}");
            latexBuilder.AppendLine(@"\usepackage{arabtex}");
            latexBuilder.AppendLine(@"\usepackage{utf8}");
            latexBuilder.AppendLine(@"\setcode{utf8}");
            latexBuilder.AppendLine(@"\usepackage[arabic]{babel}");
            latexBuilder.AppendLine(@"\usepackage{setspace}");
            latexBuilder.AppendLine(@"\usepackage{parskip}");
            latexBuilder.AppendLine(@"\usepackage{fontspec}");
            latexBuilder.AppendLine(@"\setmainfont{Amiri}");
            latexBuilder.AppendLine(@"\usepackage{fancyhdr}");
            latexBuilder.AppendLine(@"\pagestyle{fancy}");
            latexBuilder.AppendLine(@"\fancyhf{}");
            latexBuilder.AppendLine(@"\fancyhead[C]{\textbf{تقرير القيود اليومية - " + LatexEscape(account.AccountName) + @"}}");
            latexBuilder.AppendLine(@"\fancyfoot[C]{\thepage}");
            latexBuilder.AppendLine(@"\begin{document}");
            latexBuilder.AppendLine(@"\begin{landscape}");
            latexBuilder.AppendLine(@"\begin{longtable}{p{2cm}|p{6cm}|p{3cm}|p{3cm}|p{4cm}}");
            latexBuilder.AppendLine(@"\hline");
            latexBuilder.AppendLine(@"\textbf{التاريخ} & \textbf{الوصف} & \textbf{المدين} & \textbf{الدائن} & \textbf{نوع القيد} \\ \hline");
            latexBuilder.AppendLine(@"\endhead");

            foreach (var entry in journalEntries)
            {
                var debit = entry.DebitAmount > 0 ? entry.DebitAmount.ToString("C") : "-";
                var credit = entry.CreditAmount > 0 ? entry.CreditAmount.ToString("C") : "-";
                latexBuilder.AppendLine($@"{entry.EntryDate:yyyy-MM-dd} & {LatexEscape(entry.Description)} & {LatexEscape(debit)} & {LatexEscape(credit)} & {LatexEscape(entry.InvoiceType)} \\ \hline");
            }

            latexBuilder.AppendLine(@"\end{longtable}");
            latexBuilder.AppendLine(@"\end{landscape}");
            latexBuilder.AppendLine(@"\end{document}");

            // The LaTeX content will be processed by latexmk to generate the PDF
            var pdfBytes = GeneratePdfFromLatex(latexBuilder.ToString()); // Assume this method exists to compile LaTeX to PDF
            return File(pdfBytes, "application/pdf", $"JournalEntries_{account.AccountName}.pdf");
        }

        // Helper method to escape special characters for LaTeX
        private string LatexEscape(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Replace(@"\", @"\\")
                        .Replace("&", @"\&")
                        .Replace("%", @"\%")
                        .Replace("$", @"\$")
                        .Replace("#", @"\#")
                        .Replace("_", @"\_")
                        .Replace("{", @"\{")
                        .Replace("}", @"\}")
                        .Replace("~", @"\textasciitilde")
                        .Replace("^", @"\textasciicircum");
        }

        // Placeholder for PDF generation (to be implemented based on your setup)
        private byte[] GeneratePdfFromLatex(string latexContent)
        {
            // This is a placeholder. In a real application, you would:
            // 1. Save the LaTeX content to a .tex file
            // 2. Use latexmk to compile it to PDF
            // 3. Read the resulting PDF bytes
            // For this example, assume the LaTeX content is compiled and returned as bytes
            throw new NotImplementedException("Implement LaTeX to PDF conversion using latexmk");
        }
    }
}
