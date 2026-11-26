using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveillanceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCorrecaoErp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrecaoErp",
                columns: table => new
                {
                    CorrecaoErpId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComparacaoId = table.Column<long>(type: "bigint", nullable: false),
                    LojaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLancamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IndiceItem = table.Column<int>(type: "int", nullable: false),
                    NumeroItem = table.Column<int>(type: "int", nullable: false),
                    Ean = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AlteracoesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explicacao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExplicacaoGeradaPorIa = table.Column<bool>(type: "bit", nullable: false),
                    HashErp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    HashErpDepois = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AplicadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AplicadaPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrecaoErp", x => x.CorrecaoErpId);
                    table.ForeignKey(
                        name: "FK_CorrecaoErp_Comparacao_ComparacaoId",
                        column: x => x.ComparacaoId,
                        principalTable: "Comparacao",
                        principalColumn: "ComparacaoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrecaoErp_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "LojaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrecaoErp_Usuario_AplicadaPorUsuarioId",
                        column: x => x.AplicadaPorUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrecaoErp_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrecaoErp_AplicadaPorUsuarioId",
                table: "CorrecaoErp",
                column: "AplicadaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecaoErp_ComparacaoId",
                table: "CorrecaoErp",
                column: "ComparacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecaoErp_LojaId",
                table: "CorrecaoErp",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecaoErp_UsuarioId_Status",
                table: "CorrecaoErp",
                columns: new[] { "UsuarioId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrecaoErp");
        }
    }
}
