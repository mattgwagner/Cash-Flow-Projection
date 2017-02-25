using Cash_Flow_Projection.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cash_Flow_Projection.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View(new Dashboard(Balance.Entries));
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
            Balance.Entries.Add(entry);

            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(String id)
        {
            Entry entry = Balance.Entries.Single(_ => _.id == id);

            return Json(new { success = Balance.Entries.TryTake(out entry) });
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}