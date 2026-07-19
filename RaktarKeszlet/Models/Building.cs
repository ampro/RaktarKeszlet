namespace RaktarKeszlet.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; } // Pl. "Központi raktár" vagy "Telephely 1"

        // Melyik céghez tartozik ez az épület?
        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        // Egy épületben több helyiség lehet
        public ICollection<Room> Rooms { get; set; }
    }
}
