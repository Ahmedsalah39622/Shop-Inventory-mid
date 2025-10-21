using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public enum StockTakingType
    {
        Daily,
        Monthly
    }

    public class StockTaking
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public decimal ExpectedQty { get; set; }
        public decimal ActualQty { get; set; }
        public decimal Difference { get; set; }
        public StockTakingType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
