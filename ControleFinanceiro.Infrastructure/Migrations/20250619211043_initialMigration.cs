using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacoes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Transacoes",
                columns: new[] { "Id", "Data", "Descricao", "Tipo", "Valor" },
                values: new object[,]
                {
                    { new Guid("08b5d3c3-ae1f-4f3e-9d5b-f9d5c5d5c5d9"), new DateTime(2022, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Combustível", 0, 800.25m },
                    { new Guid("18b5d3c3-be1f-4f3e-9d5b-f9d5c5d5c5da"), new DateTime(2022, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Financiamento Carro", 0, 900.00m },
                    { new Guid("28b5d3c3-ce1f-4f3e-9d5b-f9d5c5d5c5db"), new DateTime(2022, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Financiamento Casa", 0, 1200.00m },
                    { new Guid("38b5d3c3-de1f-4f3e-9d5b-f9d5c5d5c5dc"), new DateTime(2022, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freelance Projeto XPTO", 1, 2500.00m },
                    { new Guid("c1c6a98a-5ff2-4d1e-a158-be2861fde84b"), new DateTime(2022, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cartão de Crédito", 0, 825.82m },
                    { new Guid("c9b5d3c3-6e1f-4f3e-9d5b-f9d5c5d5c5d5"), new DateTime(2022, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Curso C#", 0, 200.00m },
                    { new Guid("d8b5d3c3-7e1f-4f3e-9d5b-f9d5c5d5c5d6"), new DateTime(2022, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Salário", 1, 7000.00m },
                    { new Guid("e8b5d3c3-8e1f-4f3e-9d5b-f9d5c5d5c5d7"), new DateTime(2022, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mercado", 0, 3000.00m },
                    { new Guid("f8b5d3c3-9e1f-4f3e-9d5b-f9d5c5d5c5d8"), new DateTime(2022, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Farmácia", 0, 300.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transacoes");
        }
    }
}
