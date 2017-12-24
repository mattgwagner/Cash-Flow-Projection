using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CashFlowProjection.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Entries",
            //    columns: table => new
            //    {
            //        id = table.Column<string>(nullable: false),
            //        Amount = table.Column<decimal>(nullable: false),
            //        Date = table.Column<DateTime>(nullable: false),
            //        Description = table.Column<string>(nullable: true),
            //        IsBalance = table.Column<bool>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Entries", x => x.id);
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(name: "Entries");
        }
    }
}
