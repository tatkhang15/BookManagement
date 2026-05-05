using Microsoft.EntityFrameworkCore.Migrations;

namespace BookManagement.Infrastructure.Data.Migrations;

public partial class AddWalletAndInstallment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "Balance",
            table: "AspNetUsers",
            type: "numeric(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AlterColumn<int>(
            name: "BookId",
            table: "Transactions",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.CreateTable(
            name: "InstallmentPlans",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                BookId = table.Column<int>(type: "integer", nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                PaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalTerms = table.Column<int>(type: "integer", nullable: false),
                MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InstallmentPlans", x => x.Id);
                table.ForeignKey(
                    name: "FK_InstallmentPlans_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_InstallmentPlans_Books_BookId",
                    column: x => x.BookId,
                    principalTable: "Books",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InstallmentPlans_BookId",
            table: "InstallmentPlans",
            column: "BookId");

        migrationBuilder.CreateIndex(
            name: "IX_InstallmentPlans_UserId",
            table: "InstallmentPlans",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InstallmentPlans");

        migrationBuilder.DropColumn(
            name: "Balance",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<int>(
            name: "BookId",
            table: "Transactions",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);
    }
}
