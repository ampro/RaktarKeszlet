
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;

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
        return View(await _context.StorageContainers.ToListAsync());
    }

    // GET: STORAGECONTAINERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var storagecontainer = await _context.StorageContainers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (storagecontainer == null)
        {
            return NotFound();
        }

        return View(storagecontainer);
    }

    // GET: STORAGECONTAINERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: STORAGECONTAINERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Type,ShelfId,Shelf,Products")] StorageContainer storagecontainer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(storagecontainer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(storagecontainer);
    }

    // GET: STORAGECONTAINERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
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
