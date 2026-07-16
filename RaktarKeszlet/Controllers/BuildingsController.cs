
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;

public class BuildingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BuildingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: BUILDINGS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Buildings.ToListAsync());
    }

    // GET: BUILDINGS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var building = await _context.Buildings
            .FirstOrDefaultAsync(m => m.Id == id);
        if (building == null)
        {
            return NotFound();
        }

        return View(building);
    }

    // GET: BUILDINGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: BUILDINGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,CompanyId,Company,Rooms")] Building building)
    {
        if (ModelState.IsValid)
        {
            _context.Add(building);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(building);
    }

    // GET: BUILDINGS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var building = await _context.Buildings.FindAsync(id);
        if (building == null)
        {
            return NotFound();
        }
        return View(building);
    }

    // POST: BUILDINGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,CompanyId,Company,Rooms")] Building building)
    {
        if (id != building.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(building);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BuildingExists(building.Id))
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
        return View(building);
    }

    // GET: BUILDINGS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var building = await _context.Buildings
            .FirstOrDefaultAsync(m => m.Id == id);
        if (building == null)
        {
            return NotFound();
        }

        return View(building);
    }

    // POST: BUILDINGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building != null)
        {
            _context.Buildings.Remove(building);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BuildingExists(int? id)
    {
        return _context.Buildings.Any(e => e.Id == id);
    }
}
