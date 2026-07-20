namespace RaktarKeszlet.Models
{
    public class StorageContainer
    {
        public int Id { get; set; }
        public string Name { get; set; } // Pl. "123-as azonosítójú raklap"
        public string? Type { get; set; } // Pl. "doboz", "raklap" [1]

        // KÖTELEZŐ MEZŐ: A hierarchia csúcsa (Ha nincs polc, ide kerül)
        public int CompanyId { get; set; }
        public Company Company { get; set; }


        // Melyik polcon/sorban van?
        public int? ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        //  Mik vannak egy dobozban/raklapon? (1:N kapcsolat)
        public ICollection<Product> Products { get; set; }

    }
}
