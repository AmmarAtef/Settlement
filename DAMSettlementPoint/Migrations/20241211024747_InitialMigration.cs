using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAMSettlementPoint.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettlementPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourEnding = table.Column<TimeSpan>(type: "time", nullable: false),
                    SettlementPointName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SettlementPointPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DSTFlag = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementPoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementPoints_DeliveryDate_HourEnding_SettlementPointName",
                table: "SettlementPoints",
                columns: new[] { "DeliveryDate", "HourEnding", "SettlementPointName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettlementPoints");
        }
    }
}
