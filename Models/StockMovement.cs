using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public enum MovementType
    {
        In,
        Out,
        Return
    }

    public class StockMovement
    {
        public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

        public MovementType MovementType { get; set; }

        public decimal Quantity { get; set; }

        public DateTime Date { get; set; }

    [MaxLength(100, ErrorMessage = "يجب ألا يزيد المرجع عن 100 حرف")]
    public string Reference { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }
    }
}