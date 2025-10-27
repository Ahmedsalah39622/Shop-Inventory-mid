using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using ShopInventory.Data;
using ShopInventory.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShopInventory.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        static ReportsController()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today;

            var sales = await _context.SalesInvoices
                .Include(s => s.Customer)
                .Include(s => s.CreatedByUser)
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            
            return View(sales);
        }

        public async Task<IActionResult> InventoryReport()
        {
            var items = await _context.Items
                .Include(i => i.Supplier)
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public IActionResult ExportSales(DateTime? startDate, DateTime? endDate, string format = "excel")
        {
            // Export SalesInvoiceItems (sales) rather than PurchaseInvoiceItems
            var query = _context.SalesInvoiceItems
                .Include(si => si.SalesInvoice)
                    .ThenInclude(s => s.Customer)
                .Include(si => si.SalesInvoice)
                    .ThenInclude(s => s.CreatedByUser)
                .Include(si => si.Item)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(si => si.SalesInvoice != null && si.SalesInvoice.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(si => si.SalesInvoice != null && si.SalesInvoice.Date <= endDate.Value);

            var sales = query.OrderByDescending(si => si.SalesInvoice != null ? si.SalesInvoice.Date : DateTime.MinValue).ToList();

            if (format == "excel")
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sales Report");
                    worksheet.Cell(1, 1).Value = "Invoice #";
                    worksheet.Cell(1, 2).Value = "Product Code";
                    worksheet.Cell(1, 3).Value = "Product Name";
                    worksheet.Cell(1, 4).Value = "Quantity";
                    worksheet.Cell(1, 5).Value = "Unit Price";
                    worksheet.Cell(1, 6).Value = "Total";
                    worksheet.Cell(1, 7).Value = "Items";
                    worksheet.Cell(1, 8).Value = "Status";
                    worksheet.Cell(1, 9).Value = "Created By";
                    worksheet.Cell(1, 10).Value = "Time";

                    int row = 2;
                    foreach (var item in sales)
                    {
                        worksheet.Cell(row, 1).Value = item.SalesInvoice?.InvoiceNumber;
                        worksheet.Cell(row, 2).Value = item.Item?.Code ?? string.Empty;
                        worksheet.Cell(row, 3).Value = item.Item?.Name ?? string.Empty;
                        worksheet.Cell(row, 4).Value = item.Quantity;
                        worksheet.Cell(row, 5).Value = item.UnitPrice;
                        worksheet.Cell(row, 6).Value = item.Total;
                        worksheet.Cell(row, 7).Value = item.SalesInvoice?.SalesInvoiceItems?.Count() ?? 0;
                        var status = (item.SalesInvoice != null && (item.SalesInvoice.TotalAmount - item.SalesInvoice.PaidAmount) == 0) ? "Paid" : "Unpaid";
                        worksheet.Cell(row, 8).Value = status;
                        worksheet.Cell(row, 9).Value = item.SalesInvoice?.CreatedByUser?.UserName ?? string.Empty;
                        worksheet.Cell(row, 10).Value = item.SalesInvoice?.Date.ToString("yyyy/MM/dd HH:mm") ?? string.Empty;
                        row++;
                    }

                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var fileBytes = stream.ToArray();
                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
                    }
                }
            }
            else if (format == "pdf")
            {
                var document = new ShopInventory.Documents.SalesReportDocument(sales);
                using var stream = new System.IO.MemoryStream();
                document.GeneratePdf(stream);
                var pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", "SalesReport.pdf");
            }
            return NotFound();
        }

            [HttpGet]
            public IActionResult ExportInventory(DateTime? startDate, DateTime? endDate, string format = "excel")
        {
            var query = _context.Items.Include(i => i.Supplier).AsQueryable();
            if (startDate.HasValue)
                query = query.Where(i => i.ExpiryDate != null && i.ExpiryDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(i => i.ExpiryDate != null && i.ExpiryDate <= endDate.Value);
            var items = query.OrderBy(i => i.Category).ThenBy(i => i.Name).ToList();

            if (format == "excel")
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Inventory Report");
                    worksheet.Cell(1, 1).Value = "Item Name";
                    worksheet.Cell(1, 2).Value = "Category";
                    worksheet.Cell(1, 3).Value = "Supplier";
                    worksheet.Cell(1, 4).Value = "Quantity";
                    worksheet.Cell(1, 5).Value = "Purchase Price";
                    worksheet.Cell(1, 6).Value = "Sale Price";
                    worksheet.Cell(1, 7).Value = "Total Value";
                    worksheet.Cell(1, 8).Value = "Is Active";
                    worksheet.Cell(1, 9).Value = "Expiry Date";

                    int row = 2;
                    foreach (var item in items)
                    {
                        worksheet.Cell(row, 1).Value = item.Name;
                        worksheet.Cell(row, 2).Value = item.Category;
                        worksheet.Cell(row, 3).Value = item.Supplier != null ? item.Supplier.Name : "";
                        worksheet.Cell(row, 4).Value = item.Quantity;
                        worksheet.Cell(row, 5).Value = item.PurchasePrice;
                        worksheet.Cell(row, 6).Value = item.SalePrice;
                        worksheet.Cell(row, 7).Value = item.Quantity * item.PurchasePrice;
                        worksheet.Cell(row, 8).Value = item.IsActive ? "Active" : "Inactive";
                        worksheet.Cell(row, 9).Value = item.ExpiryDate.HasValue ? item.ExpiryDate.Value.ToString("yyyy/MM/dd") : "";
                        row++;
                    }

                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var fileBytes = stream.ToArray();
                        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryReport.xlsx");
                    }
                }
            }
            // TODO: PDF export implementation
            return NotFound();
        }

        public IActionResult Sales()
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (DateTime.TryParse(Request.Query["startDate"], out var s)) startDate = s.Date;
            if (DateTime.TryParse(Request.Query["endDate"], out var e)) endDate = e.Date.AddDays(1).AddTicks(-1);

            // Return sales invoice items (not purchase items)
            var query = _context.SalesInvoiceItems
                .Include(si => si.SalesInvoice)
                    .ThenInclude(s => s.Customer)
                .Include(si => si.SalesInvoice)
                    .ThenInclude(s => s.CreatedByUser)
                .Include(si => si.Item)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(si => si.SalesInvoice != null && si.SalesInvoice.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(si => si.SalesInvoice != null && si.SalesInvoice.Date <= endDate.Value);

            var salesItems = query.OrderByDescending(si => si.SalesInvoice != null ? si.SalesInvoice.Date : DateTime.MinValue).ToList();
            return View(salesItems);
        }

        public IActionResult Inventory()
        {
            var items = _context.Items
                .Include(i => i.Supplier)
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToList();
            return View(items);
        }

        public IActionResult Customers()
        {
            var customers = _context.Customers
                .OrderBy(c => c.Name)
                .ToList();
            return View(customers);
        }

        public IActionResult Suppliers()
        {
            var suppliers = _context.Suppliers
                .OrderBy(s => s.Name)
                .ToList();
            return View(suppliers);
        }

        public IActionResult Financial()
        {
            var entries = _context.LedgerEntries
                .OrderByDescending(e => e.Date)
                .ToList();
            return View(entries);
        }

        public IActionResult ActivityLogs()
        {
            var logs = _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
            return View(logs);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivityLog(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today;
            endDate ??= DateTime.Today;

            var logs = await _context.ActivityLogs
                .Include(l => l.User)
                .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            return View(logs);
        }
    }
}