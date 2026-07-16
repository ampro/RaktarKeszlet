using Microsoft.AspNetCore;

using Microsoft.AspNetCore.Mvc.Rendering;
using RaktarKeszlet.Models;

namespace RaktarKeszlet.ViewModels
{
    public class ProductListViewModel
    {
        // Maguk a megjelenítendő termékek
        public IEnumerable<Product> Products { get; set; }

        // A legördülő lista a kategória szűrőhöz
        public SelectList Categories { get; set; }

        // Az éppen kiválasztott kategória (szűrésnél)
        public int? SelectedCategoryId { get; set; }

        // A szabad szavas keresőbe beírt szöveg
        public string SearchTerm { get; set; }

        // Ide jöhetnek majd a lapozáshoz szükséges mezők (pl. CurrentPage, TotalPages)


    }
}
