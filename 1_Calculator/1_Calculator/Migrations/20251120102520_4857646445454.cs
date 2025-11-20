using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _1_Calculator.Migrations
{
    /// <inheritdoc />
    public partial class _4857646445454 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type_operation",
                table: "DataInputVariants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "Operand_2",
                table: "DataInputVariants",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "Operand_1",
                table: "DataInputVariants",
                type: "double",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "DataInputVariants",
                type: "varchar(128)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "DataInputVariants");

            migrationBuilder.AlterColumn<string>(
                name: "Type_operation",
                table: "DataInputVariants",
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Operand_2",
                table: "DataInputVariants",
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Operand_1",
                table: "DataInputVariants",
                type: "varchar(128)",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
