
namespace RaktarKeszlet.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } 



        //NAvigációs tulajdonságok
        public ICollection<ApplicationUser> Users { get; set; }
    }
}
