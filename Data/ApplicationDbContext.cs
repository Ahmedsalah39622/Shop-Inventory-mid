using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Models;

namespace ShopInventory.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    public DbSet<Item> Items { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<StockTaking> StockTakings { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Installment> Installments { get; set; }
    public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
    public DbSet<Return> Returns { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Identity tables to use shorter key lengths
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(m => m.Id).HasMaxLength(128);
                entity.Property(m => m.Email).HasMaxLength(128);
                entity.Property(m => m.NormalizedEmail).HasMaxLength(128);
                entity.Property(m => m.NormalizedUserName).HasMaxLength(128);
                entity.Property(m => m.UserName).HasMaxLength(128);
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(m => m.Id).HasMaxLength(128);
                entity.Property(m => m.Name).HasMaxLength(128);
                entity.Property(m => m.NormalizedName).HasMaxLength(128);
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(m => m.LoginProvider).HasMaxLength(128);
                entity.Property(m => m.ProviderKey).HasMaxLength(128);
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(128);
                entity.Property(m => m.RoleId).HasMaxLength(128);
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(128);
                entity.Property(m => m.LoginProvider).HasMaxLength(128);
                entity.Property(m => m.Name).HasMaxLength(128);
            });

            // Configure decimal precision for Product
            builder.Entity<Product>().Property(e => e.CurrentStock).HasColumnType("decimal(18,3)");

            // Configure decimal precision for StockTaking
            builder.Entity<StockTaking>(entity =>
            {
                entity.Property(e => e.ExpectedQty).HasColumnType("decimal(18,3)");
                entity.Property(e => e.ActualQty).HasColumnType("decimal(18,3)");
                entity.Property(e => e.Difference).HasColumnType("decimal(18,3)");
            });

            // Configure unique index for Item.Code
            builder.Entity<Item>()
                .HasIndex(i => i.Code)
                .IsUnique();

            // Configure decimal precision for Item
            builder.Entity<Item>(entity =>
            {
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
                entity.Property(e => e.ReorderLevel).HasColumnType("decimal(18,3)");
            });

            // Configure decimal precision for Customer and Supplier
            builder.Entity<Customer>().Property(e => e.Balance).HasColumnType("decimal(18,2)");
            builder.Entity<Supplier>().Property(e => e.Balance).HasColumnType("decimal(18,2)");

            // Configure decimal precision for PurchaseInvoice
            builder.Entity<PurchaseInvoice>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for SalesInvoice
            builder.Entity<SalesInvoice>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for PurchaseInvoiceItem
            builder.Entity<PurchaseInvoiceItem>(entity =>
            {
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for SalesInvoiceItem
            builder.Entity<SalesInvoiceItem>(entity =>
            {
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for StockMovement
            builder.Entity<StockMovement>()
                .Property(e => e.Quantity)
                .HasColumnType("decimal(18,3)");

            // Configure decimal precision for Installment
            builder.Entity<ShopInventory.Models.Installment>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DownPayment).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MonthlyAmount).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for InstallmentPayment
            builder.Entity<InstallmentPayment>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });

            // Configure decimal precision for LedgerEntry
            builder.Entity<LedgerEntry>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            builder.Entity<PurchaseInvoiceItem>()
                .HasOne(pi => pi.PurchaseInvoice)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PurchaseInvoiceId);

            builder.Entity<SalesInvoiceItem>()
                .HasOne(si => si.SalesInvoice)
                .WithMany(s => s.SalesInvoiceItems)
                .HasForeignKey(si => si.SalesInvoiceId);
        }
    }
}