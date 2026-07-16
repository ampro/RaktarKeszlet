using System;

namespace RaktarKeszlet.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } // Pl. "A-szektor" vagy "Hűtőkamra"

        // Melyik épületben van?
        public int BuildingId { get; set; }
        public Building Building { get; set; }

        // Egy helyiségben több polc/sor lehet
        public ICollection<Shelf> Shelves { get; set; }
    }
}
