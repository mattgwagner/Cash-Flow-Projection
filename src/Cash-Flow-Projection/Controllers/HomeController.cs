using Cash_Flow_Projection.Models;
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
            return View(new Dashboard(db.Entries.SinceBalance(thru ?? DateTime.Today.AddMonths(3))));
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
        public async Task<IActionResult> Balance(Decimal balance, Account account = Account.Cash)
        {
            return await Add(new Entry
            {
                Amount = balance,
                IsBalance = true,
                Description = "BALANCE",
                Date = DateTime.UtcNow,
                Account = account
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Postpone(string id)
        {
            var entry = db.Entries.Single(_ => _.id == id);

            if (entry.Date < DateTime.UtcNow)
            {
                entry.Date = DateTime.Today.AddDays(1);
            }
            else
            {
                entry.Date = entry.Date.AddDays(1);
            }

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
                    Date = current
                });

                switch (entry.Unit)
                {
                    case RepeatingEntry.RepeatUnit.Days:
                        current = current.AddDays(entry.RepeatInterval);
                        break;

                    case RepeatingEntry.RepeatUnit.Weeks:
                        current = current.AddDays(7 * entry.RepeatInterval);
                        break;

                    case RepeatingEntry.RepeatUnit.Months:
                        current = current.AddMonths(entry.RepeatInterval);
                        break;

                    default:
                        throw new Exception("Unknown repeat unit!");
                }
            }

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

        public async Task<IActionResult> DeleteMatching(String description, DateTime? after)
        {
            // Based on how we're doing repeating, this is the only way to clean up miskeyed data

            foreach (var e in db.Entries.Where(entry => entry.Description == description))
            {
                if (after.HasValue && after >= e.Date) continue;

                db.Entries.Remove(e);
            }

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous, Route("~/Calendar")]
        public async Task<IActionResult> Calendar(String apikey)
        {
            // Verify API key

            // Build model

            Func<Entry, String> to_ics = (entry) =>
            {
                return new StringBuilder()
                    .AppendLine("BEGIN:VEVENT")
                    .AppendLine($"SUMMARY:{entry.Description} ").AppendLine(entry.Amount != 0 ? $" {entry.Amount:c}" : string.Empty)
                    .AppendLine("DTSTART:" + entry.Date.ToString("yyyyMMdd"))
                    .AppendLine("LAST-MODIFIED:" + DateTime.UtcNow.ToUniversalTime().ToString(DateFormat))
                    .AppendLine("SEQUENCE:0")
                    .AppendLine("STATUS:CONFIRMED")
                    .AppendLine("TRANSP:OPAQUE")
                    .AppendLine("END:VEVENT")
                    .ToString();
            };

            var sb = new StringBuilder()
                .AppendLine("BEGIN:VCALENDAR")
                .AppendLine("PRODID:-//Red-Leg-Dev//Cash Flow Projections//EN")
                .AppendLine("VERSION:2.0")
                .AppendLine("METHOD:PUBLISH");

            foreach (var entry in db.Entries.SinceBalance(DateTime.Today.AddYears(1)))
            {
                sb = sb.Append(to_ics(entry));
            }

            sb =
                sb
                .Append(to_ics(new Entry
                {
                    // Add a fake entry for today with the estimated balance

                    Description = "BALANCE (Est)",
                    Amount = db.Entries.GetBalanceOn(DateTime.Today),
                    Date = DateTime.Today,
                    IsBalance = true
                }))
                .AppendLine("END:VCALENDAR");

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
            await HttpContext.Authentication.SignOutAsync("Auth0", new Microsoft.AspNetCore.Http.Authentication.AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Index))
            });

            await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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