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
using RaktarKeszlet.Services;

// ... (a Controller elején lévõ konstruktorhoz esetleg hozzá kell adnod az IWebHostEnvironment-et, ha képeket is mentesz)

[Authorize] // Csak bejelentkezett felhasználók érhetik el a termékek kezelését
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileUploadService _fileUploadService; // Új sor

    // A konstruktorban elkérjük a környezeti változókat is a képmentéshez
    public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IFileUploadService fileUploadService)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _fileUploadService = fileUploadService; // Új sor
    }



    // GET: PRODUCTS

    public async Task<IActionResult> Index(
      string? searchTerm,
      int? selectedCategoryId,
      string? categoryName,
      int? selectedCompanyId,  // Ezt használja majd a Telephely kártya!
      int? selectedBuildingId, // Ezt használja majd az Épület kártya!
      int? roomId,             // ÚJ: Ezt fogja használni a Helyiség kártya
      int? shelfId,            // ÚJ: Ezt fogja használni a Polc kártya
      int? selectedContainerId,
      string? CompanyName,
      int page = 1)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Alap lekérdezés + Jogosultság szûrés
        var productsQuery = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Where(p => p.Company.UserId == currentUserId)
            .AsQueryable();

        // 2. SZÛRÉSEK ALKALMAZÁSA

        // A) ID alapú kategória szûrés (A te meglévõ legördülõ listádból)
        if (selectedCategoryId.HasValue)
            productsQuery = productsQuery.Where(p => p.CategoryId == selectedCategoryId.Value);

        // B) Név alapú kategória szûrés (A Dashboardról történõ kattintásból)
        if (!string.IsNullOrEmpty(categoryName))
        {
            if (categoryName == "Nincs besorolva")
                productsQuery = productsQuery.Where(p => p.CategoryId == null);
            else
                productsQuery = productsQuery.Where(p => p.Category.Name == categoryName);
        }

        // B) Név alapú cég szûrés (A Dashboardról történõ kattintásból)
        if (!string.IsNullOrEmpty(CompanyName))
        {
            productsQuery = productsQuery.Where(p => p.Company.Name == CompanyName);
        }

        // --- AZ ÚJ KÁRTYÁKRÓL ÉS A LEGÖRDÜLÕKBÕL ÉRKEZÕ SZÛRÉSEK ---

        if (selectedCompanyId.HasValue)
            productsQuery = productsQuery.Where(p => p.CompanyId == selectedCompanyId.Value);

        if (selectedBuildingId.HasValue)
            productsQuery = productsQuery.Where(p => p.BuildingId == selectedBuildingId.Value);

        if (roomId.HasValue) // ÚJ: Helyiség szûrés
            productsQuery = productsQuery.Where(p => p.RoomId == roomId.Value);

        if (shelfId.HasValue) // ÚJ: Polc szûrés
            productsQuery = productsQuery.Where(p => p.ShelfId == shelfId.Value);

        if (selectedContainerId.HasValue)
            productsQuery = productsQuery.Where(p => p.StorageContainerId == selectedContainerId.Value);

        // --- SZABADSZAVAS KERESÕ ---
        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(searchTerm) ||
                (p.Barcode != null && p.Barcode.Contains(searchTerm)));
        }

        // 3. Lapozás beállítása
        int pageSize = 10;
        var totalItems = await productsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pagedProducts = await productsQuery
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 4. Adatok a legördülõkhöz
        var myCategories = await _context.Categories.ToListAsync();
        var myCompanies = await _context.Companies.Where(c => c.UserId == currentUserId).ToListAsync();
        var myBuildings = await _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToListAsync();
        var myContainers = await _context.StorageContainers.Where(c => c.Company.UserId == currentUserId).ToListAsync();

        // 5. ViewModel összeállítása
        var viewModel = new ProductListViewModel
        {
            Products = pagedProducts,
            CurrentPage = page,
            TotalPages = totalPages,
            SearchTerm = searchTerm,
            SelectedCategoryId = selectedCategoryId,
            SelectedCompanyId = selectedCompanyId,
            SelectedBuildingId = selectedBuildingId,
            SelectedContainerId = selectedContainerId,
            Categories = new SelectList(myCategories, "Id", "Name", selectedCategoryId),
            Companies = myCompanies,
            Buildings = myBuildings,
            Containers = myContainers
        };

        return View(viewModel);
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Lekérjük a terméket a teljes tárolási hierarchiájával
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Company)
            .Include(p => p.Building)
            .Include(p => p.Room)
            .Include(p => p.Shelf)
            .Include(p => p.StorageContainer)
            .FirstOrDefaultAsync(m => m.Id == id && m.Company.UserId == currentUserId);

        if (product == null) return NotFound();

        // 2. Lekérjük a termékhez tartozó LOG-okat (Mozgástörténet)
        var logs = await _context.TransactionLogs
            .Include(t => t.User) // Behozzuk a felhasználó adatait (hogy lássuk, KI csinálta)
            .Include(t => t.FromStorageContainer) // Honnan
            .Include(t => t.ToStorageContainer)   // Hová
            .Where(t => t.ProductId == id)
            .OrderByDescending(t => t.TransactionDate) // A legfrissebb legyen legfelül
            .ToListAsync();

        // 3. Összeállítjuk a ViewModel-t
        var vm = new RaktarKeszlet.ViewModels.ProductDetailsViewModel
        {
            Product = product,
            TransactionLogs = logs
        };

        return View(vm);
    }

    // GET: Products/Create
    // ÚJÍTÁS: Paraméterként várjuk a helyszín azonosítóit, ha "virtuálisan belépve" érkezik a felhasználó
    public async Task<IActionResult> Create(int? companyId, int? buildingId, int? roomId, int? shelfId, int? storageContainerId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var categories = await _context.Categories.ToListAsync();
        var companies = await _context.Companies.Where(c => c.UserId == currentUserId).ToListAsync();
        var buildings = await _context.Buildings.Where(b => b.Company.UserId == currentUserId).ToListAsync();
        var rooms = await _context.Rooms.Where(r => r.Building.Company.UserId == currentUserId).ToListAsync();
        var shelves = await _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserId).ToListAsync();
        var containers = await _context.StorageContainers.Where(c => c.Shelf.Room.Building.Company.UserId == currentUserId).ToListAsync();

        // SEGÉDFÜGGVÉNY: Sütik biztonságos kiolvasása
        int? GetCookieVal(string key) => Request.Cookies.ContainsKey(key) && int.TryParse(Request.Cookies[key], out int val) ? val : null;

        // OKOS INICIALIZÁLÁS
        var vm = new ProductCreateViewModel
        {
            // Logika: 
            // 1. URL paraméter (ha "fúrásból" érkezel) 
            // 2. Süti (az utolsó mentett érték) 
            // 3. Null/0 (ha még sosem csináltál semmit)
            CompanyId = companyId ?? GetCookieVal("LastCompanyId") ?? 0,
            CategoryId = GetCookieVal("LastCategoryId") ?? 0, // A Kategóriát is megjegyzi!
            BuildingId = buildingId ?? GetCookieVal("LastBuildingId"),
            RoomId = roomId ?? GetCookieVal("LastRoomId"),
            ShelfId = shelfId ?? GetCookieVal("LastShelfId"),
            StorageContainerId = storageContainerId ?? GetCookieVal("LastContainerId"),

            Categories = new SelectList(categories, "Id", "Name")
        };

        ViewBag.Companies = companies;
        ViewBag.Buildings = buildings;
        ViewBag.Rooms = rooms;
        ViewBag.Shelves = shelves;
        ViewBag.Containers = containers;

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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // --- 1. ÚJ HIERARCHIA SZINTEK LÉTREHOZÁSA (Ha a felhasználó az "Új hozzáadása" opciót választotta) ---
            if (vm.CompanyId == -1 && !string.IsNullOrWhiteSpace(vm.NewCompanyName))
            {
                var newCompany = new Company { Name = vm.NewCompanyName, UserId = currentUserId };
                _context.Companies.Add(newCompany);
                await _context.SaveChangesAsync();
                vm.CompanyId = newCompany.Id; // Az új ID-t átadjuk a ViewModelnek
            }
            // --- 0. ÚJ KATEGÓRIA LÉTREHOZÁSA ---
            if (vm.CategoryId == -1 && !string.IsNullOrWhiteSpace(vm.NewCategoryName))
            {
                var newCategory = new Category { Name = vm.NewCategoryName };
                _context.Categories.Add(newCategory);
                await _context.SaveChangesAsync();

                // Az újonnan létrejött kategória azonosítóját átadjuk a ViewModelnek, 
                // így a termék már ehhez fog kapcsolódni a mentéskor!
                vm.CategoryId = newCategory.Id;
            }
            if (vm.BuildingId == -1 && !string.IsNullOrWhiteSpace(vm.NewBuildingName) && vm.CompanyId > 0)
            {
                var newBuilding = new Building { Name = vm.NewBuildingName, CompanyId = vm.CompanyId };
                _context.Buildings.Add(newBuilding);
                await _context.SaveChangesAsync();
                vm.BuildingId = newBuilding.Id;
            }

            if (vm.RoomId == -1 && !string.IsNullOrWhiteSpace(vm.NewRoomName) && vm.BuildingId > 0)
            {
                var newRoom = new Room { Name = vm.NewRoomName, BuildingId = vm.BuildingId.Value };
                _context.Rooms.Add(newRoom);
                await _context.SaveChangesAsync();
                vm.RoomId = newRoom.Id;
            }

            if (vm.ShelfId == -1 && !string.IsNullOrWhiteSpace(vm.NewShelfIdentifier) && vm.RoomId > 0)
            {
                var newShelf = new Shelf { Identifier = vm.NewShelfIdentifier, RoomId = vm.RoomId.Value };
                _context.Shelves.Add(newShelf);
                await _context.SaveChangesAsync();
                vm.ShelfId = newShelf.Id;
            }

            if (vm.StorageContainerId == -1 && !string.IsNullOrWhiteSpace(vm.NewContainerName) && vm.CompanyId > 0)
            {
                var newContainer = new StorageContainer
                {
                    Name = vm.NewContainerName,
                    Type = "kartondoboz", // Alapértelmezés a terv szerint
                    CompanyId = vm.CompanyId,
                    ShelfId = vm.ShelfId > 0 ? vm.ShelfId : null
                };
                _context.StorageContainers.Add(newContainer);
                await _context.SaveChangesAsync();
                vm.StorageContainerId = newContainer.Id;
            }

            // --- 2. KÉPFELTÖLTÉS KEZELÉSE ---
            string uploadedPhotoUrl = null;
            if (vm.Photo != null)
            {
                // A képet a wwwroot/images/products mappába mentjük
                uploadedPhotoUrl = await _fileUploadService.UploadFileAsync(vm.Photo, "products");
            }
            // Itt az uploadedPhotoUrl értéket már hozzárendelheted az új Product objektum PhotoUrl mezõjéhez!

            // --- 3. PRODUCT ENTITÁS ÖSSZEÁLLÍTÁSA A VIEWMODEL ALAPJÁN ---

            // Biztosítjuk, hogy legalább 1 db létrejöjjön, még ha 0-t vagy negatívot is adtak meg
            int quantityToCreate = vm.Quantity > 0 ? vm.Quantity : 1;

            for (int i = 0; i < quantityToCreate; i++)
            {
                var product = new Product
                {
                    Name = vm.Name,
                    Price = vm.Price,
                    Barcode = vm.Barcode,
                    RCode = vm.RCode,
                    CategoryId = vm.CategoryId,
                    PhotoUrl = uploadedPhotoUrl,

                    // Hierarchia (Itt már a frissített, vagy a fentebb frissen létrehozott ID-k szerepelnek)
                    CompanyId = vm.CompanyId,
                    BuildingId = vm.BuildingId > 0 ? vm.BuildingId : null,
                    RoomId = vm.RoomId > 0 ? vm.RoomId : null,
                    ShelfId = vm.ShelfId > 0 ? vm.ShelfId : null,
                    StorageContainerId = vm.StorageContainerId > 0 ? vm.StorageContainerId : null
                };
                _context.Add(product);
            }

            
            await _context.SaveChangesAsync();
            // --- AZ UTOLSÓ KIVÁLASZTÁS MENTÉSE SÜTIBE (Pl. 30 napig érvényes) ---
            var cookieOptions = new CookieOptions { Expires = DateTime.Now.AddDays(30) };

            Response.Cookies.Append("LastCompanyId", vm.CompanyId.ToString(), cookieOptions);
            Response.Cookies.Append("LastCategoryId", vm.CategoryId.ToString(), cookieOptions);

            if (vm.BuildingId.HasValue) Response.Cookies.Append("LastBuildingId", vm.BuildingId.Value.ToString(), cookieOptions);
            else Response.Cookies.Delete("LastBuildingId");

            if (vm.RoomId.HasValue) Response.Cookies.Append("LastRoomId", vm.RoomId.Value.ToString(), cookieOptions);
            else Response.Cookies.Delete("LastRoomId");

            if (vm.ShelfId.HasValue) Response.Cookies.Append("LastShelfId", vm.ShelfId.Value.ToString(), cookieOptions);
            else Response.Cookies.Delete("LastShelfId");

            if (vm.StorageContainerId.HasValue) Response.Cookies.Append("LastContainerId", vm.StorageContainerId.Value.ToString(), cookieOptions);
            else Response.Cookies.Delete("LastContainerId");
            // ------------------------------------------------------------------
            return RedirectToAction(nameof(Index)); // Mentés után vissza a listához
        }

        // --- HA HIBA VAN, ÚJRATÖLTJÜK AZ ADATOKAT A VIEWMODEL-BE ---
        var currentUserIdErr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        vm.Categories = new SelectList(_context.Categories, "Id", "Name", vm.CategoryId);

        var myCompanies = _context.Companies.Where(c => c.UserId == currentUserIdErr).ToList();
        ViewData["CompanyId"] = new SelectList(myCompanies, "Id", "Name", vm.CompanyId);

        ViewBag.Buildings = _context.Buildings.Where(b => b.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Rooms = _context.Rooms.Where(r => r.Building.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Shelves = _context.Shelves.Where(s => s.Room.Building.Company.UserId == currentUserIdErr).ToList();
        ViewBag.Containers = _context.StorageContainers.Where(s => s.Company.UserId == currentUserIdErr).ToList();

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

            // --- 1. EREDETI ÁLLAPOT MENTÉSE A LOGOLÁSHOZ ---
            int? originalContainerId = product.StorageContainerId;
            int? newContainerId = vm.StorageContainerId;

            // --- 2. TRANZAKCIÓS NAPLÓ (LOG) LÉTREHOZÁSA ---
            var log = new TransactionLog
            {
                UserId = currentUserId,
                ProductId = product.Id,
                // Eldöntjük, hogy tényleges raktári áthelyezés történt-e, vagy csak adatot (pl. árat) módosítottak
                ActionType = originalContainerId != newContainerId ? "Termék áthelyezése" : "Termék adatainak módosítása",
                TransactionDate = DateTime.Now,
                FromStorageContainerId = originalContainerId,
                ToStorageContainerId = newContainerId
            };
            _context.TransactionLogs.Add(log);

            // 3. KÉPFELTÖLTÉS ÉS CSERE KEZELÉSE (Az eredeti kódod alapján)
            // 3. KÉPFELTÖLTÉS ÉS CSERE KEZELÉSE (Javítva: FileUploadService használatával)
            if (vm.Photo != null)
            {
                // Új kép mentése az általunk írt szolgáltatáson keresztül (ez végzi a 800px-es SkiaSharp átméretezést!)
                string uploadedPhotoUrl = await _fileUploadService.UploadFileAsync(vm.Photo, "products");

                // Régi kép törlése a szerverrõl, ha volt ilyen
                if (!string.IsNullOrEmpty(product.PhotoUrl))
                {
                    // Mivel a PhotoUrl most már "products/fajlnev.jpg" formátumban van, az "images" mappához fûzzük hozzá
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.PhotoUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Az adatbázis URL frissítése az új (már lekicsinyített) fájlra
                product.PhotoUrl = uploadedPhotoUrl;
            }

            // 4. MEGLÉVÕ ADATOK FELÜLÍRÁSA
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

                // A termék adatainak frissítése és az új log rögzítése EGYETLEN tranzakcióként fut le!
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id)) return NotFound();
                else throw;
            }

            // Sikerüzenet és visszairányítás a Részletek oldalra, hogy azonnal látható legyen a friss mozgástörténet
            TempData["SuccessMessage"] = "A termék adatai és mozgástörténete sikeresen frissítve!";
            return RedirectToAction(nameof(Details), new { id = product.Id });
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
