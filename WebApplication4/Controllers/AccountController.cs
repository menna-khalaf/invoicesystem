using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WebApplication4.Controllers
{
    public class AccountController : Controller
    {
        private readonly InvoiceSystemrahtkContext _context;

        public AccountController(InvoiceSystemrahtkContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetAccountHierarchy()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            var query = from at in _context.AccountTypes
                        join pa in _context.ParentAccounts on at.Id equals pa.AccountTypeId into paGroup
                        from pa in paGroup.DefaultIfEmpty()
                        join ca in _context.ChildAccounts on pa.Id equals ca.ParentAccountId into caGroup
                        from ca in caGroup.DefaultIfEmpty()
                        join cb in _context.Childbalances on new { ca.Id, UserId = userId.Value } equals new { Id = cb.ChildAccountId, cb.UserId } into cbGroup
                        from cb in cbGroup.DefaultIfEmpty()
                        where pa.UserId == null || pa.UserId == userId
                        orderby at.Id, pa.Id, ca.Id
                        select new
                        {
                            AccountTypeId = at.Id,
                            AccountTypeName = at.Name,
                            ParentAccountId = pa != null ? pa.Id : (int?)null,
                            ParentAccountName = pa != null ? pa.Name : null,
                            ChildAccountId = ca != null ? ca.Id : (int?)null,
                            ChildAccountName = ca != null ? ca.Name : null,
                            Balance = cb != null ? cb.Balance : 0m
                        };

            var results = await query.ToListAsync();

            var hierarchy = results
                .GroupBy(r => new { r.AccountTypeId, r.AccountTypeName })
                .Select(at => new AccountHierarchyViewModel
                {
                    AccountTypeId = at.Key.AccountTypeId,
                    AccountTypeName = at.Key.AccountTypeName,
                    ParentAccounts = at
                        .Where(r => r.ParentAccountName != null)
                        .GroupBy(r => new { r.ParentAccountId, r.ParentAccountName })
                        .Select(pa => new ParentAccountViewModel
                        {
                            ParentAccountId = pa.Key.ParentAccountId,
                            ParentAccountName = pa.Key.ParentAccountName,
                            ChildAccounts = pa
                                .Where(r => r.ChildAccountName != null)
                                .Select(r => new ChildAccountViewModel
                                {
                                    Name = r.ChildAccountName,
                                    Balance = r.Balance
                                })
                                .Distinct()
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return View(hierarchy);
        }

        // GET: Add Parent Account
        public IActionResult AddParentAccount(int? accountTypeId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            ViewBag.AccountTypes = _context.AccountTypes.ToList();
            ViewBag.SelectedAccountTypeId = accountTypeId; // Preselect account type
            return View();
        }

        // POST: Add Parent Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParentAccount(ParentAccount model)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            if (ModelState.IsValid)
            {
                // Set UserID to the logged-in user's ID (user-specific parent account)
                model.UserId = userId.Value;
                _context.ParentAccounts.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GetAccountHierarchy));
            }

            ViewBag.AccountTypes = _context.AccountTypes.ToList();
            ViewBag.SelectedAccountTypeId = model.AccountTypeId;
            return View(model);
        }

        // GET: Add Child Account
        public IActionResult AddChildAccount(int? accountTypeId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Get shared parent accounts (UserID is null) and user-specific parent accounts
            // Filter by account type if provided
            var parentAccountsQuery = _context.ParentAccounts
                .Where(pa => pa.UserId == null || pa.UserId == userId);
            if (accountTypeId.HasValue)
            {
                parentAccountsQuery = parentAccountsQuery.Where(pa => pa.AccountTypeId == accountTypeId.Value);
            }
            ViewBag.ParentAccounts = parentAccountsQuery.ToList();
            ViewBag.SelectedAccountTypeId = accountTypeId;

            return View(new AddChildAccountViewModel());
        }

        // POST: Add Child Account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChildAccount(AddChildAccountViewModel model, int? accountTypeId)
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Usered");
            }

            // Validate that the selected parent account is either shared or belongs to the user
            var validParent = _context.ParentAccounts
                .Any(pa => pa.Id == model.ParentAccountId && (pa.UserId == null || pa.UserId == userId));
            if (!validParent)
            {
                ModelState.AddModelError("ParentAccountId", "You don't have permission to use this parent account");
            }

            if (ModelState.IsValid)
            {
                // Create new ChildAccount
                var childAccount = new ChildAccount
                {
                    Name = model.Name,
                    ParentAccountId = model.ParentAccountId,
                    UserId = userId.Value
                };

                _context.ChildAccounts.Add(childAccount);
                await _context.SaveChangesAsync();

                // Insert initial balance into childbalances if provided
                if (model.InitialBalance != 0)
                {
                    var childBalance = new Childbalance
                    {
                        UserId = userId.Value,
                        ChildAccountId = childAccount.Id,
                        Balance = model.InitialBalance
                    };
                    _context.Childbalances.Add(childBalance);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(GetAccountHierarchy));
            }

            // If validation fails, reload parent accounts
            var parentAccountsQuery = _context.ParentAccounts
                .Where(pa => pa.UserId == null || pa.UserId == userId);
            if (accountTypeId.HasValue)
            {
                parentAccountsQuery = parentAccountsQuery.Where(pa => pa.AccountTypeId == accountTypeId.Value);
            }
            ViewBag.ParentAccounts = parentAccountsQuery.ToList();
            ViewBag.SelectedAccountTypeId = accountTypeId;

            return View(model);
        }
    }

    public class AccountHierarchyViewModel
    {
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public List<ParentAccountViewModel> ParentAccounts { get; set; }
    }

    public class ParentAccountViewModel
    {
        public int? ParentAccountId { get; set; }
        public string ParentAccountName { get; set; }
        public List<ChildAccountViewModel> ChildAccounts { get; set; } 
    }




    public class ChildAccountViewModel
    {
        public string Name { get; set; }
        public decimal Balance { get; set; } // Added to store the balance
    }



    public class AddChildAccountViewModel
    {
        public string Name { get; set; }
        public int ParentAccountId { get; set; }
        public int? UserId { get; set; }
        public decimal InitialBalance { get; set; }
    }




}