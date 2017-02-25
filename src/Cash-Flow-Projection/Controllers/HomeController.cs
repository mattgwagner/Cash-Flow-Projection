using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cash_Flow_Projection.Models;

namespace Cash_Flow_Projection.Controllers
{
    public class HomeController : Controller
    {
        static HomeController()
        {
            Balance.Entries.Add(new Entry { Date = new DateTime(2017, 2, 19), Amount = 2000, Description = "Balance", IsBalance = true });
            Balance.Entries.Add(new Entry { Date = new DateTime(2017, 2, 21), Amount = -75, Description = "Meh" });
            Balance.Entries.Add(new Entry { Date = new DateTime(2017, 2, 23), Amount = -225.34m, Description = "Mep" });
        }

        public IActionResult Index()
        {
            var results = Balance.Entries.GetDailyBalance(new DateTime(2017, 2, 20), DateTime.Today);

            return Json(results);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
