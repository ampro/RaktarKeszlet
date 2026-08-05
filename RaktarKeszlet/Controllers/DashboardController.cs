using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;



namespace RaktarKeszlet.Controllers
{
    [Authorize] // Csak bejelentkezett felhasználók érhetik el [7]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Csak egy IQueryable lekérdezést hozunk létre a Cégekre (MÉG NEM fut le az adatbázisban!)
            var myCompanyQuery = _context.Companies
                .Where(c => c.UserId == currentUserId);

            // Ezt az IQueryable listát használjuk az al-lekérdezésekben
            var myCompanyIdsQuery = myCompanyQuery.Select(c => c.Id);

            // 2. Szintek darabszámainak lekérdezése tisztán ADATBÁZIS-OLDALI al-lekérdezésekkel (Standard SQL IN)
            // Ezek garantáltan NEM generálnak CTE-t (WITH-et)!
            int totalCompanies = await myCompanyQuery.CountAsync();

            int totalBuildings = await _context.Buildings
                .Where(b => myCompanyIdsQuery.Contains(b.CompanyId))
                .CountAsync();

            int totalRooms = await _context.Rooms
                .Where(r => myCompanyIdsQuery.Contains(r.Building.CompanyId))
                .CountAsync();

            int totalShelves = await _context.Shelves
                .Where(s => myCompanyIdsQuery.Contains(s.Room.Building.CompanyId))
                .CountAsync();

            int totalContainers = await _context.StorageContainers
                .Where(sc => myCompanyIdsQuery.Contains(sc.CompanyId))
                .CountAsync();

            // 3. Termékek lekérdezése szintén tiszta al-lekérdezéssel
            var myProductsQuery = _context.Products
                .Where(p => myCompanyIdsQuery.Contains(p.CompanyId));

            int totalProducts = await myProductsQuery.CountAsync();
            decimal totalValue = totalProducts > 0 ? await myProductsQuery.SumAsync(p => p.Price) : 0;

            // 4. Kategóriák megoszlása (Memóriában csoportosítva a biztonság kedvéért)
            var productData = await myProductsQuery
                .Select(p => new {
                    CategoryName = p.Category != null ? p.Category.Name : "Nincs besorolva",
                    Price = p.Price
                })
                .ToListAsync();

            var categoryDistribution = productData
                .GroupBy(p => p.CategoryName)
                .Select(g => new CategoryCountViewModel
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(p => p.Price),
                    Percentage = totalProducts > 0 ? (int)Math.Round((double)g.Count() / totalProducts * 100) : 0
                })
                .OrderByDescending(c => c.ProductCount)
                .ToList();

            // 5. Legutóbbi mozgások betöltése eager-loadinggal (Include), manuális JOIN-ok nélkül
            var rawLogs = await _context.TransactionLogs
                .Include(t => t.Product)
                .Include(t => t.FromStorageContainer)
                .Include(t => t.ToStorageContainer)
                .Where(t => t.UserId == currentUserId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            var recentLogs = rawLogs.Select(log => new DashboardLogViewModel
            {
                Id = log.Id,
                ProductName = log.Product?.Name ?? "Ismeretlen termék",
                ActionType = log.ActionType,
                TransactionDate = log.TransactionDate,
                FromContainerName = log.FromStorageContainer?.Name ?? "Nincs",
                ToContainerName = log.ToStorageContainer?.Name ?? "Nincs"
            }).ToList();

            var viewModel = new DashboardViewModell
            {
                TotalCompanies = totalCompanies,
                TotalBuildings = totalBuildings,
                TotalRooms = totalRooms,
                TotalShelves = totalShelves,
                TotalContainers = totalContainers,
                TotalProducts = totalProducts,
                TotalInventoryValue = totalValue,
                ProductsByCategory = categoryDistribution,
                RecentLogs = recentLogs
            };

            return View(viewModel);
        }
    }
}
