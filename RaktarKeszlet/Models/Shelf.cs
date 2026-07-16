namespace RaktarKeszlet.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public string Identifier { get; set; } // Pl. "B-sor, 3. polc"

        // Melyik helyiségben van?
        public int RoomId { get; set; }
        public Room Room { get; set; }

        // Egy polcon több tárolóeszköz lehet
        public ICollection<StorageContainer> StorageContainers { get; set; }
    }
}
