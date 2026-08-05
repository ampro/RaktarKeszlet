
using System;
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

    public class DashboardViewModell
    {
        // Fő statisztikák
        public int TotalCompanies { get; set; }
        public int TotalBuildings { get; set; }
        public int TotalRooms { get; set; }
        public int TotalShelves { get; set; }
        public int TotalContainers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }

        // Kategóriák megoszlása
        public List<CategoryCountViewModel> ProductsByCategory { get; set; }

        // Legutóbbi mozgások vetített listája (típusbiztos join-nal)
        public List<DashboardLogViewModel> RecentLogs { get; set; }
    }

    public class CategoryCountViewModel
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
        public int Percentage { get; set; }
    }

    public class DashboardLogViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ActionType { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FromContainerName { get; set; }
        public string ToContainerName { get; set; }
    }
}