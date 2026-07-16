namespace RaktarKeszlet.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        
        // Kapcsolat a termékekhez
        public ICollection<Product> Products { get; set; }
    }
}
