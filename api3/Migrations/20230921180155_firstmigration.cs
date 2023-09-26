using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api3.Migrations
{
    /// <inheritdoc />
    public partial class firstmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    ID_Employee = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Employee__D9EE4F362C37AD8C", x => x.ID_Employee);
                });

            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    ID_Store = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Store__99D83D2C0FA271F8", x => x.ID_Store);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    ID_Inventory = table.Column<int>(type: "integer", nullable: false),
                    ID_Store = table.Column<int>(type: "integer", nullable: true),
                    ID_Employee = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTime>(type: "date", nullable: true),
                    Flavor = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Is_season_flavor = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Inventor__2210F49E6563AFD3", x => x.ID_Inventory);
                    table.ForeignKey(
                        name: "FK__Inventory__ID_Em__412EB0B6",
                        column: x => x.ID_Employee,
                        principalTable: "Employee",
                        principalColumn: "ID_Employee");
                    table.ForeignKey(
                        name: "FK__Inventory__ID_St__403A8C7D",
                        column: x => x.ID_Store,
                        principalTable: "Store",
                        principalColumn: "ID_Store");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ID_Employee",
                table: "Inventory",
                column: "ID_Employee");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ID_Store",
                table: "Inventory",
                column: "ID_Store");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "Store");
        }
    }
}
