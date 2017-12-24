using Microsoft.EntityFrameworkCore.Migrations;

namespace CashFlowProjection.Migrations
{
    public partial class Account : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Account",
                table: "Entries",
                nullable: false,
                defaultValue: (byte)Cash_Flow_Projection.Models.Account.Cash);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Account",
                table: "Entries");
        }
    }
}