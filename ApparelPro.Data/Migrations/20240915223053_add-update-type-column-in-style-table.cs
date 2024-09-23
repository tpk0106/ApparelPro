using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.Metrics;

#nullable disable

namespace ApparelPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class addupdatetypecolumninstyletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // added drop and add constaint by thusith on 16 Sep 20204 
            // The object 'PK_Styles' is dependent on column 'Type'.
            //ALTER TABLE ALTER COLUMN Type failed because one or more objects access this column.
            migrationBuilder.DropUniqueConstraint("PK_Styles","Styles"); 

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Styles",
                type: "int",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            // added by thusith on 16 Sep 20204
            migrationBuilder.AddUniqueConstraint("PK_Styles", "Styles", "Type");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // added drop and add constaint by thusith on 16 Sep 20204 

            migrationBuilder.DropUniqueConstraint("PK_Styles", "Styles");
            
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Styles",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 5);

            // added drop and add constaint by thusith on 16 Sep 20204 

            migrationBuilder.AddUniqueConstraint("PK_Styles", "Styles", "Type");
        }
    }
}
