using System.ComponentModel.DataAnnotations;

namespace ShopInventory.Models
{
    public enum EntryType
    {
        Credit,
        Debit
    }

    public class LedgerEntry
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

    [Required(ErrorMessage = "يرجى إدخال الوصف")]
    [MaxLength(100, ErrorMessage = "يجب ألا يزيد الوصف عن 100 حرف")]
    public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public EntryType EntryType { get; set; }

    [MaxLength(100, ErrorMessage = "يجب ألا يزيد المرجع عن 100 حرف")]
    public string Reference { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }

        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}