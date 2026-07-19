
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;

public class ShelvesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShelvesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SHELFS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Shelves.ToListAsync());
    }

    // GET: SHELFS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var shelf = await _context.Shelves
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shelf == null)
        {
            return NotFound();
        }

        return View(shelf);
    }

    // GET: SHELFS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: SHELFS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Identifier,RoomId,Room,StorageContainers")] Shelf shelf)
    {
        if (ModelState.IsValid)
        {
            _context.Add(shelf);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(shelf);
    }

    // GET: SHELFS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
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

        var shelf = await _context.Shelves
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
