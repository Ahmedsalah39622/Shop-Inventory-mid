namespace ShopInventory.Models;

public class DashboardViewModel
{
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public int TotalOrders { get; set; }
    public int LowStockItems { get; set; }
    public int LowStockCount => LowStockItems;
    public List<TopSellingItem> TopSellingItems { get; set; } = [];
    public List<PurchaseInvoice> RecentPurchases { get; set; } = [];
    // Recent sales to show cash / card / installment sales on dashboard
    public List<SalesInvoice> RecentSales { get; set; } = [];
    public List<Item> Items { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<Customer> Clients { get; set; } = [];
}

public class TopSellingItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}