using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using ShopInventory.Models;
using System;
using System.Collections.Generic;

namespace ShopInventory.Documents
{
    public class SalesReportDocument : IDocument
    {
        private readonly List<PurchaseInvoiceItem> _sales;
        public SalesReportDocument(List<PurchaseInvoiceItem> sales)
        {
            _sales = sales;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text("Sales Report").FontSize(20).Bold().AlignCenter();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80); // Invoice #
                        columns.ConstantColumn(100); // Product Code
                        columns.RelativeColumn(); // Product Name
                        columns.ConstantColumn(60); // Quantity
                        columns.ConstantColumn(80); // Unit Price
                        columns.ConstantColumn(80); // Total
                        columns.ConstantColumn(60); // Items
                        columns.ConstantColumn(60); // Status
                        columns.ConstantColumn(80); // Created By
                        columns.ConstantColumn(120); // Time
                    });
                    // Header row
                    table.Cell().Element(CellStyle).Text("Invoice #").SemiBold();
                    table.Cell().Element(CellStyle).Text("Product Code").SemiBold();
                    table.Cell().Element(CellStyle).Text("Product Name").SemiBold();
                    table.Cell().Element(CellStyle).Text("Quantity").SemiBold();
                    table.Cell().Element(CellStyle).Text("Unit Price").SemiBold();
                    table.Cell().Element(CellStyle).Text("Total").SemiBold();
                    table.Cell().Element(CellStyle).Text("Items").SemiBold();
                    table.Cell().Element(CellStyle).Text("Status").SemiBold();
                    table.Cell().Element(CellStyle).Text("Created By").SemiBold();
                    table.Cell().Element(CellStyle).Text("Time").SemiBold();
                    // Data rows
                    foreach (var item in _sales)
                    {
                        table.Cell().Element(CellStyle).Text(item.InvoiceNumber);
                        table.Cell().Element(CellStyle).Text(item.ProductCode);
                        table.Cell().Element(CellStyle).Text(item.ItemName);
                        table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                        table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("F2"));
                        table.Cell().Element(CellStyle).Text(item.Total.ToString("F2"));
                        table.Cell().Element(CellStyle).Text((item.PurchaseInvoice?.Items?.Count() ?? 0).ToString());
                        table.Cell().Element(CellStyle).Text(item.Status);
                        table.Cell().Element(CellStyle).Text(item.CreatedByUserName);
                        table.Cell().Element(CellStyle).Text(item.Date.ToString("yyyy/MM/dd HH:mm"));
                    }
                });
            });
        }

        private IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(2).PaddingHorizontal(4).BorderBottom(1).BorderColor("#E0E0E0");
        }
    }
}
