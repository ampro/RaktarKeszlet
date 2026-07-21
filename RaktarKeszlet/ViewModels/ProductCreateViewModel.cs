using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using RaktarKeszlet.Models;

namespace RaktarKeszlet.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "A név megadása kötelező!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Az ár megadása kötelező!")]
        public int Price { get; set; }

      
        public string? Barcode { get; set; }

        //Valójában QR kód, de a rövidítés miatt RCode-nak hívjuk
        public string? RCode { get; set; }

        // Opcionális képfeltöltés (IFormFile fogadja a fájlt a böngészőből)
        public IFormFile? Photo { get; set; }

        // Kategória kiválasztása
        public int? CategoryId { get; set; }
        public SelectList? Categories { get; set; }

        // Tárolóeszköz (hova rakjuk a raktárban)
        public int? StorageContainerId { get; set; }
        public SelectList? StorageContainers { get; set; }


        // Hierarchia azonosítók az űrlaphoz
        public int CompanyId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public int? ShelfId { get; set; }

        // --- ÚJ MEZŐK A HELYBEN LÉTREHOZÁSHOZ ---
        public string? NewCompanyName { get; set; }
        public string? NewBuildingName { get; set; }
        public string? NewRoomName { get; set; }
        public string? NewShelfIdentifier { get; set; }
        public string? NewContainerName { get; set; }

    }
}
