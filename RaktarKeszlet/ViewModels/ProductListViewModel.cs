using Microsoft.AspNetCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaktarKeszlet.Models;

namespace RaktarKeszlet.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }

        // Lapozáshoz
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Szűrési értékek, amiket a felhasználó beírt/kiválasztott
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedCompanyId { get; set; }
        public int? SelectedBuildingId { get; set; }
        public int? SelectedContainerId { get; set; }

        // Legördülő listák a felülethez
        public SelectList Categories { get; set; }
        public IEnumerable<Company> Companies { get; set; }
        public IEnumerable<Building> Buildings { get; set; }
        public IEnumerable<StorageContainer> Containers { get; set; }

    }
}
