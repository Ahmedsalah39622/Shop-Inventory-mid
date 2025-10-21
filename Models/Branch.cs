using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class Branch
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ManagerId { get; set; }
        public ApplicationUser? Manager { get; set; }

        public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    }
}