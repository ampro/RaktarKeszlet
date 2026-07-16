namespace RaktarKeszlet.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }

        // MIKOR?
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // MIT CSINÁLT? (pl. "Hozzáadás", "Kivétel", "Átmozgatás")
        public string ActionType { get; set; }

        // KI? (Az IdentityUser azonosítója alapból string típusú)
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // MIT? (Melyik terméket)
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // HONNAN HOVA? (Opcionális kapcsolatok a tárolóeszközökhöz átmozgatás esetén)
        public int? FromStorageContainerId { get; set; }
        public StorageContainer? FromStorageContainer { get; set; }

        public int? ToStorageContainerId { get; set; }
        public StorageContainer? ToStorageContainer { get; set; }
    }
}
