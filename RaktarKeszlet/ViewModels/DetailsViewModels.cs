using RaktarKeszlet.Models;
using System.Collections.Generic;

namespace RaktarKeszlet.ViewModels
{
    // 1. Cég (Company) részleteihez
    public class CompanyDetailsViewModel
    {
        public Company Company { get; set; }
        public IEnumerable<Building> PagedBuildings { get; set; }

        public int TotalBuildingsCount { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // 2. Épület (Building) részleteihez
    public class BuildingDetailsViewModel
    {
        public Building Building { get; set; }
        public IEnumerable<Room> PagedRooms { get; set; }

        public int TotalRoomsCount { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // 3. Helyiség (Room) részleteihez
    public class RoomDetailsViewModel
    {
        public Room Room { get; set; }
        public IEnumerable<Shelf> PagedShelves { get; set; }

        public int TotalShelvesCount { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // 4. Polc / Sor (Shelf) részleteihez
    public class ShelfDetailsViewModel
    {
        public Shelf Shelf { get; set; }
        public IEnumerable<StorageContainer> PagedContainers { get; set; }

        public int TotalContainersCount { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    // 5. Tárolóeszköz (StorageContainer) részleteihez
    public class StorageContainerDetailsViewModel
    {
        public StorageContainer Container { get; set; }
        public IEnumerable<Product> PagedProducts { get; set; }

        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}