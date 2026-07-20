namespace RaktarKeszlet.Models
{
    public class Product
    {
        public int Id { get; set; }

        // A tervben előírt kötelező adatok:
        public string Name { get; set; } // Név
        public int Price { get; set; } // Ár
        public string? Barcode { get; set; } // Vonalkód
        public string? RCode { get; set; } // R kód

        // Opcionálisan fénykép (pl. a kép elérési útvonala a szerveren)
        public string? PhotoUrl { get; set; }

        // TÁROLÁSI HIERARCHIA KAPCSOLATOK

        // KÖTELEZŐ: A hierarchia csúcsa (Cég / Tulajdonos)
        public int CompanyId { get; set; }
        public Company Company { get; set; }

        // OPCIONÁLIS: A köztes szintek és a legalsó szint
        public int? BuildingId { get; set; }
        public Building? Building { get; set; }

        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        public int? ShelfId { get; set; }
        public Shelf? Shelf { get; set; }

        public int? StorageContainerId { get; set; }
        public StorageContainer? StorageContainer { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<TransactionLog> TransactionLogs { get; set; }
    }
}
