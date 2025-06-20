using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changesInTheTransactionsClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("08b5d3c3-ae1f-4f3e-9d5b-f9d5c5d5c5d9"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("18b5d3c3-be1f-4f3e-9d5b-f9d5c5d5c5da"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("28b5d3c3-ce1f-4f3e-9d5b-f9d5c5d5c5db"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("38b5d3c3-de1f-4f3e-9d5b-f9d5c5d5c5dc"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("c1c6a98a-5ff2-4d1e-a158-be2861fde84b"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("c9b5d3c3-6e1f-4f3e-9d5b-f9d5c5d5c5d5"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("d8b5d3c3-7e1f-4f3e-9d5b-f9d5c5d5c5d6"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("e8b5d3c3-8e1f-4f3e-9d5b-f9d5c5d5c5d7"));

            migrationBuilder.DeleteData(
                table: "Transacoes",
                keyColumn: "Id",
                keyValue: new Guid("f8b5d3c3-9e1f-4f3e-9d5b-f9d5c5d5c5d8"));

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Transacoes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Transacoes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Transacoes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Transacoes");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Transacoes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

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
    }
}
