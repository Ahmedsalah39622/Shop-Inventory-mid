using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }
    }
}