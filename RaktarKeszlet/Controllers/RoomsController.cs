
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.Models;
using RaktarKeszlet.ViewModels;
using System.Security.Claims;

public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Rooms
    public async Task<IActionResult> Index()
    {
        // Az Include parancs mondja meg az adatbázisnak, hogy a helyiséggel együtt hozza el a Building adatait is
        var roomsWithBuildings = _context.Rooms.Include(r => r.Building);

        return View(await roomsWithBuildings.ToListAsync());
    }

    // GET: Rooms/Details/5
    public async Task<IActionResult> Details(int? id, int page = 1)
    {
        if (id == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Lekérjük a Helyiséget (Biztonsági ellenõrzéssel a Céghez)
        var room = await _context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(m => m.Id == id && m.Building.Company.UserId == currentUserId);

        if (room == null) return NotFound();

        // 2. Aggregátumok kiszámítása SQL szinten a HELYISÉGBEN lévõ összes termékre
        // Mivel a termékek felvitelénél a RoomId közvetlenül is rögzítésre kerül (függetlenül attól, 
        // hogy van-e polcon), ezt nagyon egyszerûen le tudjuk kérdezni.
        var productsInRoomQuery = _context.Products.Where(p => p.RoomId == id);
        int totalProducts = await productsInRoomQuery.CountAsync();
        decimal totalValue = totalProducts > 0 ? await productsInRoomQuery.SumAsync(p => p.Price) : 0;

        // 3. Lapozás elõkészítése a Polcokhoz
        var shelvesQuery = _context.Shelves.Where(s => s.RoomId == id);
        int totalShelves = await shelvesQuery.CountAsync();

        int pageSize = 10;
        int totalPages = (int)Math.Ceiling(totalShelves / (double)pageSize);

        var pagedShelves = await shelvesQuery
            .OrderBy(s => s.Identifier)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 4. ViewModel összeállítása
        var vm = new RoomDetailsViewModel
        {
            Room = room,
            PagedShelves = pagedShelves,
            TotalShelvesCount = totalShelves,
            TotalProductsCount = totalProducts,
            TotalProductsValue = totalValue,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(vm);
    }
    // GET: ROOMS/Create
    public IActionResult Create()
    {
        ViewData["BuildingId"] = new SelectList(_context.Buildings, "Id", "Name");
        return View();
    }

    // POST: ROOMS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,BuildingId,Building,Shelves")] Room room)
    {
        // EZT A KÉT SORT SZÚRD BE: Kikapcsoljuk az ellenõrzést azokra a kapcsolatokra, amiket nem az ûrlap tölt ki!
        ModelState.Remove("Building");
        ModelState.Remove("Shelves");
        if (ModelState.IsValid)
        {
            _context.Add(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Name");
        return View(room);
    }

    // GET: ROOMS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    // POST: ROOMS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,BuildingId,Building,Shelves")] Room room)
    {
        if (id != room.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id))
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
        return View(room);
    }

    // GET: ROOMS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .Include(r => r.Building)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // POST: ROOMS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RoomExists(int? id)
    {
        return _context.Rooms.Any(e => e.Id == id);
    }
}
