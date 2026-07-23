using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaktarKeszlet.Models;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.ViewModels;
using System.Security.Claims;


namespace RaktarKeszlet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // 2. A KONSTRUKTOR: Itt kérjük el (Dependency Injection) és rendeljük hozzá a változóhoz!
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // <-- EZ A SOR HIÁNYZOTT VAGY NEM FUTOTT LE (Ez okozta a null hibát!)
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ha nincs bejelentkezve a felhasználó, egy üres Dashboardot mutatunk (vagy átirányíthatjuk a Loginra)
            if (currentUserId == null)
            {
                return View(new DashboardViewModel
                {
                    ValueByCategory = new Dictionary<string, int>(),
                    ValueByCompany = new List<CompanyStat>()
                });
            }

            // Nagyon gyors SQL lekérdezés: Csak az árat és a neveket kérjük le a memóriába
            var rawData = await _context.Products
      .Include(p => p.Category)
      .Include(p => p.Company)
      .Where(p => p.Company.UserId == currentUserId)
      .Select(p => new {
          Price = p.Price,
          CategoryName = p.Category != null ? p.Category.Name : "Nincs besorolva",
          CompanyName = p.Company != null ? p.Company.Name : "Ismeretlen cég",
          CompanyId = p.CompanyId // Ezt az új mezõt lekérjük az adatbázisból
      })
      .ToListAsync();

            // Aggregálások kiszámítása LINQ segítségével
            var vm = new DashboardViewModel
            {
                TotalProducts = rawData.Count,
                TotalValue = rawData.Sum(x => x.Price),

                ValueByCategory = rawData
         .GroupBy(x => x.CategoryName)
         .OrderByDescending(g => g.Sum(x => x.Price))
         .ToDictionary(g => g.Key, g => g.Sum(x => x.Price)),

                // Cég szerinti csoportosítás kiegészítve az ID-val
                ValueByCompany = rawData
         .GroupBy(x => new { x.CompanyId, x.CompanyName }) // ID és név alapján csoportosítunk
         .OrderByDescending(g => g.Sum(x => x.Price))
         .Select(g => new CompanyStat
         {
             CompanyId = g.Key.CompanyId,
             CompanyName = g.Key.CompanyName,
             TotalValue = g.Sum(x => x.Price)
         })
         .ToList()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
