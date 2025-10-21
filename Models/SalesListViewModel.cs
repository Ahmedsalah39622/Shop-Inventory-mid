using System;
using System.Collections.Generic;

namespace ShopInventory.Models
{
    public class SalesListViewModel
    {
        public List<SalesInvoice> Sales { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalSales { get; set; }
    }
}