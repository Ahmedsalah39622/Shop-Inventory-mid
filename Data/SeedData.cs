using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopInventory.Models;

namespace ShopInventory.Data
{
        public static class SeedData
        {
            public static async Task AssignAdminRoleToUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, string email)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    if (!await userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // Ensure Admin role exists
                        if (!await roleManager.RoleExistsAsync("Admin"))
                            await roleManager.CreateAsync(new IdentityRole("Admin"));
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
            await SeedInitialData(context);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Cashier", "StoreKeeper" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsers(UserManager<ApplicationUser> userManager)
        {
            // Seed Admin
            var adminUser = new ApplicationUser
            {
                UserName = "admin@shop.com",
                Email = "admin@shop.com",
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Cashier
            var cashierUser = new ApplicationUser
            {
                UserName = "cashier@shop.com",
                Email = "cashier@shop.com",
                FullName = "Shop Cashier",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(cashierUser.Email) == null)
            {
                var result = await userManager.CreateAsync(cashierUser, "Cashier@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(cashierUser, "Cashier");
                }
            }

            // Seed StoreKeeper
            var storeKeeperUser = new ApplicationUser
            {
                UserName = "storekeeper@shop.com",
                Email = "storekeeper@shop.com",
                FullName = "Store Keeper",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(storeKeeperUser.Email) == null)
            {
                var result = await userManager.CreateAsync(storeKeeperUser, "StoreKeeper@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(storeKeeperUser, "StoreKeeper");
                }
            }
        }

        private static async Task SeedInitialData(ApplicationDbContext context)
        {
            // Only seed if no data exists
            if (!context.Products.Any() && !context.StockMovements.Any() && !context.StockTakings.Any())
            {
                var products = new[]
                {
                    new Product { Name = "Test Product", SKU = "TEST001", Unit = "Piece", CurrentStock = 100 },
                    new Product { Name = "Monitor", SKU = "MON002", Unit = "Piece", CurrentStock = 50 },
                    new Product { Name = "Keyboard", SKU = "KEY003", Unit = "Piece", CurrentStock = 75 }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();

                var movements = new[]
                {
                    new StockMovement { ProductId = products[0].Id, MovementType = MovementType.In, Quantity = 100, Date = DateTime.Today.AddDays(-5) },
                    new StockMovement { ProductId = products[1].Id, MovementType = MovementType.In, Quantity = 50, Date = DateTime.Today.AddDays(-3) },
                    new StockMovement { ProductId = products[2].Id, MovementType = MovementType.In, Quantity = 75, Date = DateTime.Today.AddDays(-2) },
                    new StockMovement { ProductId = products[0].Id, MovementType = MovementType.Out, Quantity = 10, Date = DateTime.Today.AddDays(-1) },
                    new StockMovement { ProductId = products[1].Id, MovementType = MovementType.Out, Quantity = 5, Date = DateTime.Today.AddDays(-1) },
                    new StockMovement { ProductId = products[2].Id, MovementType = MovementType.Return, Quantity = 2, Date = DateTime.Today }
                };
                context.StockMovements.AddRange(movements);
                await context.SaveChangesAsync();

                var stockTakings = new[]
                {
                    new StockTaking { ProductId = products[0].Id, ExpectedQty = 90, ActualQty = 88, Difference = -2, Type = StockTakingType.Daily, Date = DateTime.Today },
                    new StockTaking { ProductId = products[1].Id, ExpectedQty = 45, ActualQty = 45, Difference = 0, Type = StockTakingType.Daily, Date = DateTime.Today },
                    new StockTaking { ProductId = products[2].Id, ExpectedQty = 77, ActualQty = 80, Difference = 3, Type = StockTakingType.Daily, Date = DateTime.Today }
                };
                context.StockTakings.AddRange(stockTakings);
                await context.SaveChangesAsync();
            }
        }
    }
}