namespace RaktarKeszlet.ViewModels
{
    public class AggregateCardsViewModel
    {
        // Az első kártya (pl. "Nyilvántartott Épületek") változó adatai
        public string FirstCardTitle { get; set; }
        public string FirstCardIcon { get; set; }
        public int FirstCardCount { get; set; }

        // Termék statisztikák
        public int TotalProductsCount { get; set; }
        public decimal TotalProductsValue { get; set; }

        // Szűrési paraméterek a kattintáshoz (Ami null, aszerint nem szűrünk)
        public int? CompanyId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomId { get; set; }
        public int? ShelfId { get; set; }
    }
}
