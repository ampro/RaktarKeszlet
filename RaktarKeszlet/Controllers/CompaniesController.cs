
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
            .Where(c => c.UserId == currentUserId)
            .ToListAsync();

        return View(myCompanies);
    }
    // GET: Companies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var company = await _context.Companies
            .FirstOrDefaultAsync(m => m.Id == id);

        if (company == null) return NotFound();

        // Biztonsági ellenõrzés: csak a sajátját nézheti meg
        if (company.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            return Forbid();

        return View(company);
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
