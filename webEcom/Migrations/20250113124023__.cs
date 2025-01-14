using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webEcom.Migrations
{
    /// <inheritdoc />
    public partial class _ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PRODUCT",
                columns: table => new
                {
                    IdProduct = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductPrice = table.Column<float>(type: "real", nullable: false),
                    ProductCount = table.Column<int>(type: "int", nullable: false),
                    IformfileProductInputImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProductImageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductUserTel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductUserAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductCreateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductUserCart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductSendBill = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductBill = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductSendTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductSendTimesuccess = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductSendNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT", x => x.IdProduct);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PRODUCT");
        }
    }
}
