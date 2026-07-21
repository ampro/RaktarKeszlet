using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace RaktarKeszlet.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A kategória nevének megadása kötelező!")] 
        public string Name { get; set; } // pl. könyv, szerszám, alkatrész 



        // Kapcsolat a termékekhez
        public ICollection<Product> Products { get; set; }
    }
}
