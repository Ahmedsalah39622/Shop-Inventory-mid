namespace ShopInventory.Models;

public class DashboardViewModel
{
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public int TotalOrders { get; set; }
    public int SalesCount { get; set; }
    public int LowStockItems { get; set; }
    public int LowStockCount => LowStockItems;
    // Percent changes compared to previous day
    public decimal SalesAmountPercentChange { get; set; }
    public bool SalesAmountIncreased { get; set; }
    public decimal SalesCountPercentChange { get; set; }
    public bool SalesCountIncreased { get; set; }
    public List<TopSellingItem> TopSellingItems { get; set; } = new List<TopSellingItem>();
    public List<PurchaseInvoice> RecentPurchases { get; set; } = new List<PurchaseInvoice>();
    // Recent sales to show cash / card / installment sales on dashboard
    public List<SalesInvoice> RecentSales { get; set; } = new List<SalesInvoice>();
    public List<Item> Items { get; set; } = new List<Item>();
    public List<Customer> Customers { get; set; } = new List<Customer>();
    public List<Customer> Clients { get; set; } = new List<Customer>();
}

public class TopSellingItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}