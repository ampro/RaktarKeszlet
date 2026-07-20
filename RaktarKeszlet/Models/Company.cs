
using System.ComponentModel.DataAnnotations.Schema;

namespace RaktarKeszlet.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // ÚJ: A cég tulajdonosának (a belépett felhasználónak) az azonosítója
        public string UserId { get; set; }

        // MÓDOSÍTOTT NAVIGÁCIÓS TULAJDONSÁG FELFELÉ:
        // Az ICollection helyett csak egyetlen User objektumra hivatkozunk,
        // hiszen a hierarchia szerint a cég tartozik a felhasználóhoz.
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // NAVIGÁCIÓS TULAJDONSÁG LEFELÉ:
        // A céghez tartozó épületek (ezt már valószínűleg beállítottad korábban)
        public ICollection<Building> Buildings { get; set; }

        public ICollection<StorageContainer> StorageContainers { get; set; }
    }
}
