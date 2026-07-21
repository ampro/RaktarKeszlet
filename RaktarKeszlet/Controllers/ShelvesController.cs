
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Authorize]
public class ShelvesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShelvesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Shelves
    public async Task<IActionResult> Index()
    {
        // Az Include parancs mondja meg az adatbázisnak, hogy a polccal együtt hozza el a Room adatait is
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var myShelves = _context.Shelves
            .Include(s => s.Room)
            .Where(s => s.Room.Building.Company.UserId == currentUserId);

        return View(await myShelves.ToListAsync());
    }

    // GET: SHELFS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves.Include(s => s.Room)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shelf == null)
        {
            return NotFound();
        }

        return View(shelf);
    }

    // GET: Shelves/Create
    public IActionResult Create()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Cégek (Hierarchia csúcsa - csak UI szûréshez)
        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewBag.Companies = new SelectList(myCompanies, "Id", "Name");

        // 2. Épületek (Köztes szint - csak UI szûréshez)
        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();

        // 3. Helyiségek (Ez a polc közvetlen szülõje, ezt fogjuk elmenteni!)
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Identifier,RoomId")] Shelf shelf)
    {
        // Kivesszük a validációból a navigációs propertyket és az alsóbb szinteket
        ModelState.Remove("Room");
        ModelState.Remove("StorageContainers");

        if (ModelState.IsValid)
        {
            _context.Add(shelf);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- HIBA ESETÉN A LISTÁK ÚJRATÖLTÉSE ---
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewBag.Companies = new SelectList(myCompanies, "Id", "Name");
        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();

        return View(shelf);
    }

    // GET: SHELFS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves.FindAsync(id);
        if (shelf == null)
        {
            return NotFound();
        }
        return View(shelf);
    }

    // POST: SHELFS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Identifier,RoomId,Room,StorageContainers")] Shelf shelf)
    {
        ModelState.Remove("Room");
        ModelState.Remove("StorageContainers");
        if (id != shelf.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(shelf);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShelfExists(shelf.Id))
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
        return View(shelf);
    }

    // GET: SHELFS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves.Include(s => s.Room)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shelf == null)
        {
            return NotFound();
        }

        return View(shelf);
    }

    // POST: SHELFS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var shelf = await _context.Shelves.FindAsync(id);
        if (shelf != null)
        {
            _context.Shelves.Remove(shelf);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ShelfExists(int? id)
    {
        return _context.Shelves.Any(e => e.Id == id);
    }
}
