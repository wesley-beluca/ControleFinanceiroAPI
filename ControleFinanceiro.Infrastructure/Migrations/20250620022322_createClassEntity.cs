using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createClassEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataCriacao",
                table: "Transacoes",
                newName: "DataInclusao");

            migrationBuilder.RenameColumn(
                name: "DataAtualizacao",
                table: "Transacoes",
                newName: "DataAlteracao");

            migrationBuilder.AddColumn<bool>(
                name: "Excluido",
                table: "Transacoes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Excluido",
                table: "Transacoes");

            migrationBuilder.RenameColumn(
                name: "DataInclusao",
                table: "Transacoes",
                newName: "DataCriacao");

            migrationBuilder.RenameColumn(
                name: "DataAlteracao",
                table: "Transacoes",
                newName: "DataAtualizacao");
        }
    }
}
