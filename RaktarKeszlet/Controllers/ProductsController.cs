using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // Ez kell a fájlfeltöltéshez
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Data;
using RaktarKeszlet.Models;
using RaktarKeszlet.ViewModels;


// ... (a Controller elején lévõ konstruktorhoz esetleg hozzá kell adnod az IWebHostEnvironment-et, ha képeket is mentesz)

[Authorize] // Csak bejelentkezett felhasználók érhetik el a termékek kezelését
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // A konstruktorban elkérjük a környezeti változókat is a képmentéshez
    public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }



    // GET: PRODUCTS

    public async Task<IActionResult> Index(string? searchTerm, int? selectedCategoryId, int? selectedBuildingId, int? selectedContainerId, int page = 1)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Alap lekérdezés + JOGOSULTSÁG SZÛRÉS (Csak a saját cég termékei!)
        var productsQuery = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Where(p => p.Company.UserId == currentUserId)
            .AsQueryable();

        // 2. Szûrés Kategóriára
        if (selectedCategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == selectedCategoryId.Value);
        }

        // 3. Szûrés Épületre (Raktárra)
        if (selectedBuildingId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.BuildingId == selectedBuildingId.Value);
        }

        // 4. Szûrés Tárolóeszközre (Doboz/Raklap)
        if (selectedContainerId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.StorageContainerId == selectedContainerId.Value);
        }

        // 5. Szabad szavas keresõ (Név vagy Vonalkód)
        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm)));
        }

        // 6. Lapozás beállítása
        int pageSize = 10;
        var totalItems = await productsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pagedProducts = await productsQuery
            .OrderByDescending(p => p.Id) // Legújabbak elöl
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 7. Legördülõ listák lekérése a saját adatokból
        var myCategories = await _context.Categories.ToListAsync();
        var myBuildings = await _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToListAsync();
        var myContainers = await _context.StorageContainers.Where(c => c.Company.UserId == currentUserId).ToListAsync();

        // 8. ViewModel összeállítása
        var viewModel = new ProductListViewModel
        {
            Products = pagedProducts,
            CurrentPage = page,
            TotalPages = totalPages,
            SearchTerm = searchTerm,
            SelectedCategoryId = selectedCategoryId,
            SelectedBuildingId = selectedBuildingId,
            SelectedContainerId = selectedContainerId,
            Categories = new SelectList(myCategories, "Id", "Name", selectedCategoryId),
            Buildings = new SelectList(myBuildings, "Id", "Name", selectedBuildingId),
            Containers = new SelectList(myContainers, "Id", "Name", selectedContainerId)
        };

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
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Include(p => p.Building)
            .Include(p => p.Room)
            .Include(p => p.Shelf)
            .Include(p => p.StorageContainer).FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // GET: PRODUCTS/Create
    public IActionResult Create()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var vm = new ProductCreateViewModel();

        // Kategóriák betöltése a ViewModelbe
        // (Feltételezem, hogy létrehoztad a _context.Categories DbSet-et a Data mappában)
        vm.Categories = new SelectList(_context.Categories, "Id", "Name");

        // --- HIERARCHIA BETÖLTÉSE ---
        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name");

        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToList();

        // A tárolóeszközöket kézzel adjuk át, hogy a HTML data-parent attribútumokat generálhassunk a JS szûréshez
        ViewBag.Containers = _context.StorageContainers.Where(s => s.Company.UserId == currentUserId).ToList();

        return View(vm);
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel vm)
    {
        // A SelectList-eket kivesszük a validációból, mivel azokat a form nem küldi vissza POST-kor
        ModelState.Remove("Categories");
        ModelState.Remove("StorageContainers");

        if (ModelState.IsValid)
        {
            string uploadedPhotoUrl = null;

            // 1. KÉPFELTÖLTÉS KEZELÉSE
            if (vm.Photo != null)
            {
                // Létrehozzuk a wwwroot/images mappát, ha még nem létezne
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Egyedi fájlnév generálása, hogy ne írják felül egymást a képek
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Fájl mentése a szerverre
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.Photo.CopyToAsync(fileStream);
                }

                // Az adatbázisba csak az URL-t mentjük (PhotoUrl)
                uploadedPhotoUrl = "/images/" + uniqueFileName;
            }

            // 2. PRODUCT ENTITÁS ÖSSZEÁLLÍTÁSA A VIEWMODEL ALAPJÁN
            var product = new Product
            {
                Name = vm.Name,
                Price = vm.Price,
                Barcode = vm.Barcode,
                RCode = vm.RCode,
                CategoryId = vm.CategoryId,
                PhotoUrl = uploadedPhotoUrl,

                // Hierarchia (lehet bennük null is, ha a felhasználó csak Céget adott meg)
                CompanyId = vm.CompanyId,
                BuildingId = vm.BuildingId,
                RoomId = vm.RoomId,
                ShelfId = vm.ShelfId,
                StorageContainerId = vm.StorageContainerId
            };

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Mentés után vissza a listához
        }

        // --- HA HIBA VAN, ÚJRATÖLTJÜK AZ ADATOKAT A VIEWMODEL-BE ---
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        vm.Categories = new SelectList(_context.Categories, "Id", "Name", vm.CategoryId);

        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserId).ToList();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name", vm.CompanyId);

        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToList();
        ViewBag.Containers = _context.StorageContainers.Where(s => s.Company.UserId == currentUserId).ToList();

        return View(vm);
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lekérjük a terméket, de CSAK HA a bejelentkezett felhasználó cégéhez tartozik
        var product = await _context.Products
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id && p.Company.UserId == currentUserId);

        if (product == null) return NotFound();

        // A ViewModel feltöltése a meglévõ adatokkal
        var vm = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Barcode = product.Barcode,
            RCode = product.RCode,
            CategoryId = product.CategoryId,
            ExistingPhotoUrl = product.PhotoUrl, // Meglévõ kép átadása a nézetnek

            CompanyId = product.CompanyId,
            BuildingId = product.BuildingId,
            RoomId = product.RoomId,
            ShelfId = product.ShelfId,
            StorageContainerId = product.StorageContainerId
        };

        // A legördülõ listák feltöltése
        vm.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

        var myCompanies = await _context.Companies.Where(c => c.UserId == currentUserId).ToListAsync();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name", product.CompanyId);

        ViewBag.Buildings = await _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToListAsync();
        ViewBag.Rooms = await _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToListAsync();
        ViewBag.Shelves = await _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToListAsync();
        ViewBag.Containers = await _context.StorageContainers.Where(s => s.Company.UserId == currentUserId).ToListAsync();

        return View(vm);
    }

    // POST: PRODUCTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        ModelState.Remove("Categories");

        if (ModelState.IsValid)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _context.Products
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.Id == id && p.Company.UserId == currentUserId);

            if (product == null) return NotFound();

            // 1. KÉPFELTÖLTÉS ÉS CSERE KEZELÉSE
            if (vm.Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Új kép mentése
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.Photo.CopyToAsync(fileStream);
                }

                // Régi kép törlése a szerverrõl, ha volt
                if (!string.IsNullOrEmpty(product.PhotoUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, product.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Az adatbázis URL frissítése
                product.PhotoUrl = "/images/" + uniqueFileName;
            }

            // 2. MEGLÉVÕ ADATOK FELÜLÍRÁSA
            product.Name = vm.Name;
            product.Price = vm.Price;
            product.Barcode = vm.Barcode;
            product.RCode = vm.RCode;
            product.CategoryId = vm.CategoryId;

            product.CompanyId = vm.CompanyId;
            product.BuildingId = vm.BuildingId;
            product.RoomId = vm.RoomId;
            product.ShelfId = vm.ShelfId;
            product.StorageContainerId = vm.StorageContainerId;

            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // --- HIBA ESETÉN A LISTÁK ÚJRATÖLTÉSE ---
        var currentUserIdErr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        vm.Categories = new SelectList(_context.Categories, "Id", "Name", vm.CategoryId);

        var myCompaniesErr = _context.Companies.Where(c => c.UserId == currentUserIdErr).ToList();
        ViewData["CompanyId"] = new SelectList(myCompaniesErr, "Id", "Name", vm.CompanyId);

        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Containers = _context.StorageContainers.Where(s => s.Company.UserId == currentUserIdErr).ToList();

        return View(vm);
    }

    // Segédfüggvény (Ha még nincs benne a fájl legalján)
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Include(p => p.Building)
            .Include(p => p.Room)
            .Include(p => p.Shelf)
            .Include(p => p.StorageContainer).FirstOrDefaultAsync(m => m.Id == id);
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
