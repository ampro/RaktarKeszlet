using System.Collections.Generic;

namespace RaktarKeszlet.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }

        // Kategóriánkénti bontás (Kategória neve -> Érték)
        public Dictionary<string, int> ValueByCategory { get; set; }

        // Cégenkénti bontás (Cég neve -> Érték)
        public Dictionary<string, int> ValueByCompany { get; set; }
    }
}