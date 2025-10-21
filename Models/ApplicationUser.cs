using Microsoft.AspNetCore.Identity;

namespace ShopInventory.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}