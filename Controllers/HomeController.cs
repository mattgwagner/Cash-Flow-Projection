using Cash_Flow_Projection.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cash_Flow_Projection.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const string DateFormat = "yyyyMMddTHHmmssZ";

        private readonly Database db;

        public HomeController(Database db)
        {
            this.db = db;
        }

        public IActionResult Index(DateTime? thru)
        {
            return View(new Dashboard(db, thru));
        }

        [Route("~/Add")]
        public IActionResult Add()
        {
            return View(new Entry { });
        }

        [Route("~/Repeating")]
        public IActionResult Repeating()
        {
            return View(new RepeatingEntry { });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Balance(Decimal balance, AccountType account = AccountType.Cash)
        {
            var last =
                db
                .Entries
                .GetLastBalanceEntry(account)?
                .Date
                .AddSeconds(1);

            return await Add(new Entry
            {
                Amount = balance,
                IsBalance = true,
                Description = "BALANCE",
                Date = last ?? DateTime.Today,
                Account = account
            });
        }

        [HttpPost, ValidateAntiForgeryToken, Route("~/Add")]
        public async Task<IActionResult> Add(Entry entry)
        {
            db.Entries.Add(entry);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Route("~/Repeating")]
        public async Task<IActionResult> Repeating(RepeatingEntry entry)
        {
            DateTime current = entry.FirstDate;

            foreach (var iteration in Enumerable.Range(1, entry.RepeatIterations))
            {
                db.Entries.Add(new Entry
                {
                    Amount = entry.Amount,
                    Description = entry.Description,
                    Account = entry.Account,
                    Date = current
                });

                current = entry.Unit switch
                {
                    RepeatingEntry.RepeatUnit.Days => current.AddDays(entry.RepeatInterval),
                    RepeatingEntry.RepeatUnit.Weeks => current.AddDays(7 * entry.RepeatInterval),
                    RepeatingEntry.RepeatUnit.Months => current.AddMonths(entry.RepeatInterval),
                    _ => throw new Exception("Unknown repeat unit!"),
                };
            }

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous, Route("~/Calendar"), ResponseCache(Duration = 0)]
        public async Task<IActionResult> Calendar(String apikey)
        {
            // Verify API key

            // Build model

            Func<Entry, String> to_ics = (entry) =>
            {
                return new StringBuilder()
                    .AppendLine("BEGIN:VEVENT")
                    .AppendLine($"SUMMARY:{entry.Description} ").AppendLine($"{entry.Amount:c}")
                    .AppendLine("DTSTART:" + entry.Date.ToString("yyyyMMdd"))
                    .AppendLine("LAST-MODIFIED:" + DateTime.UtcNow.ToString(DateFormat))
                    .AppendLine("UID:" + entry.id)
                    .AppendLine("DTSTAMP:" + DateTime.UtcNow.ToString(DateFormat))
                    .AppendLine("SEQUENCE:0")
                    .AppendLine("STATUS:CONFIRMED")
                    .AppendLine("TRANSP:OPAQUE")
                    .AppendLine("END:VEVENT")
                    .ToString();
            };

            var sb = new StringBuilder()
                .AppendLine("BEGIN:VCALENDAR")
                .AppendLine("VERSION:2.0")
                .AppendLine("PRODID:-//Red-Leg-Dev//Cash Flow Projections//EN")
                .AppendLine("METHOD:PUBLISH");

            foreach (var entry in db.Entries.SinceBalance(DateTime.Today.AddYears(1)))
            {
                sb = sb.Append(to_ics(entry));
            }

            sb = sb.AppendLine("END:VCALENDAR");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());

            // Return iCal Feed

            return File(bytes, "text/calendar");
        }

        [AllowAnonymous]
        public IActionResult Login(String returnUrl = "/")
        {
            return new ChallengeResult("Auth0", new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = returnUrl });
        }

        [Route("~/Logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Index))
            });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Backup()
        {
            var data = System.IO.File.ReadAllBytes("Data.db");

            var mimeType = "application/octet-stream";

            return File(data, mimeType);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}