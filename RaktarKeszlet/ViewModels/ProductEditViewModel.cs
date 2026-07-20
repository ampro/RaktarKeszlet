using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RaktarKeszlet.ViewModels
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A név megadása kötelező!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Az ár megadása kötelező!")]
        public int Price { get; set; }

        public string? Barcode { get; set; }
        public string? RCode { get; set; }

        // Opcionális képfeltöltés és a jelenlegi kép útvonalának tárolása
        public IFormFile? Photo { get; set; }
        public string? ExistingPhotoUrl { get; set; }

        // Kategória
        public int? CategoryId { get; set; }
        public SelectList? Categories { get; set; }

        // Hierarchia (Cég kötelező, a többi opcionális)
        public int CompanyId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public int? ShelfId { get; set; }
        public int? StorageContainerId { get; set; }
    }
}
