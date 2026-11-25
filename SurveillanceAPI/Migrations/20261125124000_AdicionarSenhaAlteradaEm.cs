using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveillanceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSenhaAlteradaEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SenhaAlteradaEm",
                table: "Usuario",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenhaAlteradaEm",
                table: "Usuario");
        }
    }
}
