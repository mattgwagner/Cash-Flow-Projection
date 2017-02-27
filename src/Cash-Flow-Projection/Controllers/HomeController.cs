using Cash_Flow_Projection.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cash_Flow_Projection.Controllers
{
    public class HomeController : Controller
    {
        private readonly Database db;

        public HomeController(Database db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(new Dashboard { Entries = db.Entries });
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

        [HttpPost]
        public async Task<IActionResult> Add(Entry entry)
        {
            db.Entries.Add(entry);

            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(String id)
        {
            var entry = db.Entries.Single(_ => _.id == id);

            db.Entries.Remove(entry);

            db.SaveChanges();

            return Json(new { success = true });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}