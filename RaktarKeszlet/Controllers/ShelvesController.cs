
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        // Végigmegyünk a hierarchián: Helyiség -> Épület -> Cég -> Felhasználó
        var myRooms = _context.Rooms
            .Where(r => r.Building.Company.UserId == currentUserId);

        ViewData["RoomId"] = new SelectList(myRooms, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Identifier,RoomId")] Shelf shelf)
    {
        // Kapcsolatok kivétele a validációból
        ModelState.Remove("Room");
        ModelState.Remove("StorageContainers");

        if (ModelState.IsValid)
        {
            _context.Add(shelf);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Hiba esetén a lista újratöltése a saját helyiségekkel
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var myRooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId);
        ViewData["RoomId"] = new SelectList(myRooms, "Id", "Name", shelf.RoomId);

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
