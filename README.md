# Shop Inventory Management System

A production-ready ASP.NET Core 8 MVC application for managing inventory, sales, and purchases with role-based authentication.

## Features

- Full inventory management system
- Sales and purchase invoice management
- Stock movement tracking
- Role-based authorization (Admin, Cashier, StoreKeeper)
- Activity logging
- Customer and supplier management
- Financial ledger
- Low stock and expiry alerts

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB or higher)

## Getting Started

1. Clone the repository
2. Update the connection string in `appsettings.json` if needed:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShopInventoryDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. Open a terminal in the project directory and run:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Default users are seeded with the following credentials:

   | Role        | Email                | Password     |
   |------------|---------------------|--------------|
   | Admin      | admin@shop.com      | Admin@123    |
   | Cashier    | cashier@shop.com    | Cashier@123  |
   | StoreKeeper| storekeeper@shop.com| StoreKeeper@123 |

## Project Structure

```
shop-inventory/
├─ Controllers/         # MVC Controllers
├─ Data/               # DbContext and database migrations
├─ Middleware/         # Custom middleware (Activity logging)
├─ Models/             # Domain models and view models
├─ Services/           # Business logic services
├─ Views/              # Razor views
├─ wwwroot/           # Static files (CSS, JS, etc.)
```

## Role Permissions

- **Admin**: Full access to all features
- **Cashier**: Sales management, customer management
- **StoreKeeper**: Inventory management, purchase management

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request