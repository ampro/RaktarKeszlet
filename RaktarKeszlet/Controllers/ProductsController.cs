using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using RaktarKeszlet.Data;
using RaktarKeszlet.ViewModels;
using Microsoft.AspNetCore.Hosting; // Ez kell a fájlmentéshez
using System.IO;

// ... (a Controller elején lévõ konstruktorhoz esetleg hozzá kell adnod az IWebHostEnvironment-et, ha képeket is mentesz)


public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: PRODUCTS

    public async Task<IActionResult> Index(string searchTerm, int? selectedCategoryId, int page = 1)
    {
        // 1. Alap lekérdezés: Vesszük az összes terméket, és rögtön hozzácsatoljuk a kategóriájukat is (Include), 
        // hogy a táblázatban ki tudjuk írni a nevüket.
        var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

        // 2. Szûrés kategóriára (ha a felhasználó kiválasztott egyet a legördülõbõl)
        if (selectedCategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == selectedCategoryId.Value);
        }

        // 3. Szabad szavas keresõ (keresünk a termék nevében vagy a vonalkódjában)
        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Barcode.Contains(searchTerm));
        }

        // 4. Lapozás (Paging) beállítása - pl. 10 elem oldalanként
        int pageSize = 10;

        // Kiszámoljuk, hány termék van összesen a szûrések után, hogy tudjuk a max oldalszámot
        // (Ezt a ViewModel-be is beleteheted majd, ha a HTML-ben lapozó gombokat akarsz rajzolni)
        var totalItems = await productsQuery.CountAsync();

        // Kiválasztjuk csak az adott oldalhoz tartozó elemeket (Skip és Take)
        var pagedProducts = await productsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 5. A ViewModel összeállítása
        var viewModel = new ProductListViewModel
        {
            Products = pagedProducts,
            // Legeneráljuk a legördülõ listát a kategóriákból, beállítva az aktuálisan kiválasztottat
            Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", selectedCategoryId),
            SelectedCategoryId = selectedCategoryId,
            SearchTerm = searchTerm
        };

        // Visszaadjuk a Nézetnek az összerakott csomagot
        return View(viewModel);
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // GET: PRODUCTS/Create
    public IActionResult Create()
    {
        var viewModel = new ProductCreateViewModel
        {
            Categories = new SelectList(_context.Categories, "Id", "Name"),
            StorageContainers = new SelectList(_context.StorageContainers, "Id", "Name")
        };
        return View(viewModel);
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            string uniqueFileName = null;

            // Kép mentése, ha töltöttek fel valamit
            if (viewModel.Photo != null)
            {
                // Ideális esetben a wwwroot/images mappába mentjük
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                Directory.CreateDirectory(uploadsFolder); // Létrehozza a mappát, ha nincs
                uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.Photo.CopyToAsync(fileStream);
                }
            }

            // Új termék összeállítása a ViewModel alapján
            var product = new Product
            {
                Name = viewModel.Name,
                Price = viewModel.Price,
                Barcode = viewModel.Barcode,
                RCode = viewModel.RCode,
                PhotoUrl = uniqueFileName, // Csak a fájl nevét mentjük az adatbázisba
                CategoryId = viewModel.CategoryId,
                StorageContainerId = viewModel.StorageContainerId
            };

            _context.Add(product);

            // Itt be lehetne szúrni a TransactionLog mentését is a logoláshoz!

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Ha valami hiba volt (pl. üresen hagytak egy kötelezõ mezõt), újratöltjük a legördülõket
        viewModel.Categories = new SelectList(_context.Categories, "Id", "Name", viewModel.CategoryId);
        viewModel.StorageContainers = new SelectList(_context.StorageContainers, "Id", "Name", viewModel.StorageContainerId);
        return View(viewModel);
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // POST: PRODUCTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Price,Barcode,RCode,PhotoUrl,StorageContainerId,StorageContainer,CategoryId,Category,TransactionLogs")] Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
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
        return View(product);
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int? id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
