
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


[Authorize]
public class StorageContainersController : Controller
{
    private readonly ApplicationDbContext _context;

    public StorageContainersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: STORAGECONTAINERS
    public async Task<IActionResult> Index()    
    {
        // Lekérjük a bejelentkezett felhasználó azonosítóját
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lekérdezzük a tárolókat, és hozzácsatoljuk (.Include) a Polc és Cég adatokat is!
        var myContainers = _context.StorageContainers
            .Include(s => s.Company) // Betöltjük a cég adatait
            .Include(s => s.Shelf)   // EZ OLDJA MEG A HIBÁT: Betöltjük a polc adatait is!
            .Where(s => s.Company.UserId == currentUserId);

        return View(await myContainers.ToListAsync());
    }

    // GET: STORAGECONTAINERS/Details/5
    // GET: StorageContainers/Details/5
    public async Task<IActionResult> Details(int? id, int page = 1)
    {
        if (id == null) return NotFound();

        var container = await _context.StorageContainers
            .Include(s => s.Shelf)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (container == null) return NotFound();

        // 1. Lekérdezés elõkészítése (Még nem fut le az adatbázisban!)
        var productsQuery = _context.Products
            .Include(p => p.Category)
            .Where(p => p.StorageContainerId == id);

        // 2. Aggregátumok kiszámítása SQL szinten az oldal tetejére
        int totalCount = await productsQuery.CountAsync();
        decimal totalValue = totalCount > 0 ? await productsQuery.SumAsync(p => p.Price) : 0;

        // 3. Lapozás végrehajtása
        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var pagedProducts = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 4. ViewModel összeállítása
        var vm = new RaktarKeszlet.ViewModels.StorageContainerDetailsViewModel
        {
            Container = container,
            PagedProducts = pagedProducts,
            TotalProductsCount = totalCount,
            TotalProductsValue = totalValue,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(vm);
    }

    // GET: STORAGECONTAINERS/Create
    public IActionResult Create()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Csúcsszint (Kötelezõ)
        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name");

        // 2. Alárendelt szintek (Opcionálisak, a JavaScript fogja õket szûrni)
        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToList();

        return View();
    }

    // POST: StorageContainers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Type,CompanyId,ShelfId")] StorageContainer storageContainer)
    {
        ModelState.Remove("Company");
        ModelState.Remove("Shelf");
        ModelState.Remove("Products");

        if (ModelState.IsValid)
        {
            _context.Add(storageContainer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hiba esetén újratöltjük az alap listát
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name", storageContainer.CompanyId);


        // 2. Alárendelt szintek (Épület, Szoba, Polc) újratöltése
        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToList();
        // -------------------

        return View(storageContainer);
    }

    // GET: STORAGECONTAINERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["ShelfId"] = new SelectList(_context.Shelves, "Id", "Identifier");
        if (id == null)
        {
            return NotFound();
        }

        var storagecontainer = await _context.StorageContainers.FindAsync(id);
        if (storagecontainer == null)
        {
            return NotFound();
        }
        return View(storagecontainer);
    }

    // POST: STORAGECONTAINERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Type,ShelfId,Shelf,Products")] StorageContainer storagecontainer)
    {
        if (id != storagecontainer.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(storagecontainer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StorageContainerExists(storagecontainer.Id))
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
        return View(storagecontainer);
    }

    // GET: STORAGECONTAINERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var storagecontainer = await _context.StorageContainers
            .Include(s => s.Shelf)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (storagecontainer == null)
        {
            return NotFound();
        }

        return View(storagecontainer);
    }

    // POST: STORAGECONTAINERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var storagecontainer = await _context.StorageContainers.FindAsync(id);
        if (storagecontainer != null)
        {
            _context.StorageContainers.Remove(storagecontainer);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool StorageContainerExists(int? id)
    {
        return _context.StorageContainers.Any(e => e.Id == id);
    }
}
