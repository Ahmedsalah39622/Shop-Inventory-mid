using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInventory.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallmentPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstallmentPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstallmentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstallmentPayments_Installments_InstallmentId",
                        column: x => x.InstallmentId,
                        principalTable: "Installments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Installments_CustomerId",
                table: "Installments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentPayments_InstallmentId",
                table: "InstallmentPayments",
                column: "InstallmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Customers_CustomerId",
                table: "Installments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Customers_CustomerId",
                table: "Installments");

            migrationBuilder.DropTable(
                name: "InstallmentPayments");

            migrationBuilder.DropIndex(
                name: "IX_Installments_CustomerId",
                table: "Installments");
        }
    }
}
