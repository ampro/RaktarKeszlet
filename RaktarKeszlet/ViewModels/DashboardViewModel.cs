using System.Collections.Generic;

namespace RaktarKeszlet.ViewModels
{
    // 1. Új kis segédosztály a cégadatoknak
    public class CompanyStat
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int TotalValue { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalValue { get; set; }
        public Dictionary<string, int> ValueByCategory { get; set; }

        // 2. A korábbi Dictionary helyett most a fenti osztály listáját használjuk
        public List<CompanyStat> ValueByCompany { get; set; }
    }
}