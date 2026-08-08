using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedDateTimeToPurchaseOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PurchaseOrderHeader had no creation Date/Time at all - both the Purchase
            // Order List Report and List of Outstanding P/O's need to filter/display by
            // it. Nullable so existing rows are left NULL rather than fabricated with a
            // fake date; date-range reports will correctly exclude them until/unless
            // backfilled. New P/Os get a real timestamp going forward (see
            // SupplierPurchaseOrderService.cs).
            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedDate",
                table: "PurchaseOrderHeaders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "CreatedTime",
                table: "PurchaseOrderHeaders",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "PurchaseOrderHeaders");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "PurchaseOrderHeaders");
        }
    }
}
