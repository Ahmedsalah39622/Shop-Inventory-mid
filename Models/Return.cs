using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopInventory.Models;

public class Return
{
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Type { get; set; } = "Sales"; // "Sales" or "Purchase" return

    [Required]
    public int OriginalInvoiceId { get; set; } // ID of original sales/purchase invoice

    public string? Notes { get; set; }

    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public decimal TotalAmount { get; set; }

    // Navigation properties
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    public List<ReturnItem> Items { get; set; } = [];
}

public class ReturnItem
{
    public int Id { get; set; }

    [Required]
    public int ReturnId { get; set; }
    public Return? Return { get; set; }

    [Required]
    public int ItemId { get; set; }
    public Item? Item { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public string? ReturnReason { get; set; }

    public decimal Total => Quantity * UnitPrice;
}