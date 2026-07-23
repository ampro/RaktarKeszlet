
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Authorize] // 1. Módosítás: Csak belépett felhasználó használhatja
public class CompaniesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CompaniesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Companies
    // 2. Módosítás: A listázás csak a felhasználó saját cégeit mutatja
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var myCompanies = await _context.Companies
            .Include(c => c.Products)
            .ThenInclude(p => p.Category)
            .Where(c => c.UserId == currentUserId)
            .ToListAsync();

        return View(myCompanies);
    }
    public async Task<IActionResult> Details(int? id, int page = 1)
    {
        if (id == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUserId);

        if (company == null) return NotFound();

        // 1. Céghez közvetlenül regisztrált készlet statisztikája (pl. jármûvek az udvaron)
        var productsInCompany = _context.Products.Where(p => p.CompanyId == id);
        int totalProducts = await productsInCompany.CountAsync();
        decimal totalValue = totalProducts > 0 ? await productsInCompany.SumAsync(p => p.Price) : 0;

        // 2. Épületek lapozása
        var buildingsQuery = _context.Buildings.Where(b => b.CompanyId == id);
        int totalBuildings = await buildingsQuery.CountAsync();
        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(totalBuildings / (double)pageSize);

        var pagedBuildings = await buildingsQuery
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var vm = new RaktarKeszlet.ViewModels.CompanyDetailsViewModel
        {
            Company = company,
            PagedBuildings = pagedBuildings,
            TotalBuildingsCount = totalBuildings,
            TotalProductsCount = totalProducts,
            TotalProductsValue = totalValue,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(vm);
    }

    // GET: Companies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Companies/Create
    // 3. Módosítás: Automatikusan a bejelentkezett felhasználóhoz kötjük a céget
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Company company)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        company.UserId = currentUserId;

        // Ezeket nem az ûrlapról várjuk, így kivesszük a validációból
        ModelState.Remove("UserId");
        ModelState.Remove("User");
        ModelState.Remove("Buildings");
        ModelState.Remove("StorageContainers");
      
        ModelState.Remove("Products");
        ModelState.Remove("UserId");
       

        if (ModelState.IsValid)
        {
            _context.Add(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(company);
    }

    // GET: COMPANYS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        ModelState.Remove("User");

        var company = await _context.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound();
        }
        return View(company);
    }

    // POST: COMPANYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,UserId,User,Buildings")] Company company)
    {
        if (id != company.Id)
        {
            return NotFound();
        }


        ModelState.Remove("User");
        
        
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(company);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(company.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(company);
    }

    // GET: COMPANYS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var company = await _context.Companies
            .FirstOrDefaultAsync(m => m.Id == id);
        if (company == null)
        {
            return NotFound();
        }

        return View(company);
    }

    // POST: COMPANYS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company != null)
        {
            _context.Companies.Remove(company);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CompanyExists(int? id)
    {
        return _context.Companies.Any(e => e.Id == id);
    }
}
