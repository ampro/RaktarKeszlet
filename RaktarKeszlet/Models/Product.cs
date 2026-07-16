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

        // Kapcsolat a fizikai raktárral: Melyik tárolóeszközben (doboz/raklap) van?
        // (A kérdőjel miatt ez is lehet üres, ha a termék épp nincs betárolva sehová)
        public int? StorageContainerId { get; set; }
        public StorageContainer? StorageContainer { get; set; }

        public ICollection<TransactionLog> TransactionLogs { get; set; }
    }
}
