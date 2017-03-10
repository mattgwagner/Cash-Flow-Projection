﻿using Cash_Flow_Projection.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cash_Flow_Projection.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly Database db;

        public HomeController(Database db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var last_balance = db.Entries.GetLastBalanceEntry();

            var entries = from entry in db.Entries
                          where entry.Date >= last_balance.Date
                          orderby entry.Date descending
                          select entry;

            return View(new Dashboard
            {
                Entries = entries
            });
        }

        public async Task<IActionResult> ByMonth(int month, int year)
        {
            // Cash at beginning of month

            // Projections for each day of the month?

            // Income vs Expenses

            // Ending balance (excess/deficit of cash)

            return View();
        }

        public IActionResult Add()
        {
            return View(new Entry { });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Balance(Decimal balance)
        {
            return await Add(new Entry
            {
                Amount = balance,
                IsBalance = true,
                Description = "BALANCE",
                Date = DateTime.UtcNow
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Postpone(string id)
        {
            var entry = db.Entries.Single(_ => _.id == id);

            entry.Date = entry.Date.AddDays(1);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Entry entry)
        {
            db.Entries.Add(entry);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(String id)
        {
            var entry = db.Entries.Single(_ => _.id == id);

            db.Entries.Remove(entry);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Login(String returnUrl = "/")
        {
            return new ChallengeResult("Auth0", new AuthenticationProperties { RedirectUri = returnUrl });
        }

        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Index))
            });

            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}