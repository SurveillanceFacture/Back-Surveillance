using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SurveillanceAPI.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipoUsuario",
                columns: table => new
                {
                    TipoUsuarioId = table.Column<int>(type: "int", nullable: false),
                    NomeTipoUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoUsuario", x => x.TipoUsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeUsuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    HashSenha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoAcesso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusUsuario = table.Column<bool>(type: "bit", nullable: false),
                    TipoUsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuario_TipoUsuario_TipoUsuarioId",
                        column: x => x.TipoUsuarioId,
                        principalTable: "TipoUsuario",
                        principalColumn: "TipoUsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Loja",
                columns: table => new
                {
                    LojaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusLoja = table.Column<bool>(type: "bit", nullable: false),
                    AuditorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GerenteUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loja", x => x.LojaId);
                    table.ForeignKey(
                        name: "FK_Loja_Usuario_AuditorUsuarioId",
                        column: x => x.AuditorUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loja_Usuario_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loja_Usuario_GerenteUsuarioId",
                        column: x => x.GerenteUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comparacao",
                columns: table => new
                {
                    ComparacaoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LojaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChaveDocumento = table.Column<string>(type: "nvarchar(44)", maxLength: 44, nullable: false),
                    HashXml = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataComparacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comparacao", x => x.ComparacaoId);
                    table.ForeignKey(
                        name: "FK_Comparacao_Loja_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Loja",
                        principalColumn: "LojaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comparacao_Usuario_AuditorUsuarioId",
                        column: x => x.AuditorUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Divergencia",
                columns: table => new
                {
                    DivergenciaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComparacaoId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroItem = table.Column<int>(type: "int", nullable: false),
                    CodigoProduto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ean = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampoComparado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValorXml = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValorJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gravidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataDeteccao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvidoPorUsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataResolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divergencia", x => x.DivergenciaId);
                    table.ForeignKey(
                        name: "FK_Divergencia_Comparacao_ComparacaoId",
                        column: x => x.ComparacaoId,
                        principalTable: "Comparacao",
                        principalColumn: "ComparacaoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Divergencia_Usuario_ResolvidoPorUsuarioId",
                        column: x => x.ResolvidoPorUsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoDivergencia",
                columns: table => new
                {
                    HistoricoDivergenciaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivergenciaId = table.Column<long>(type: "bigint", nullable: false),
                    StatusAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StatusNovo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoDivergencia", x => x.HistoricoDivergenciaId);
                    table.ForeignKey(
                        name: "FK_HistoricoDivergencia_Divergencia_DivergenciaId",
                        column: x => x.DivergenciaId,
                        principalTable: "Divergencia",
                        principalColumn: "DivergenciaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoricoDivergencia_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TipoUsuario",
                columns: new[] { "TipoUsuarioId", "NomeTipoUsuario" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Auditor" },
                    { 3, "Gerente" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comparacao_AuditorUsuarioId",
                table: "Comparacao",
                column: "AuditorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Comparacao_DataComparacao",
                table: "Comparacao",
                column: "DataComparacao");

            migrationBuilder.CreateIndex(
                name: "IX_Comparacao_LojaId_ChaveDocumento",
                table: "Comparacao",
                columns: new[] { "LojaId", "ChaveDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_Divergencia_ComparacaoId_CampoComparado",
                table: "Divergencia",
                columns: new[] { "ComparacaoId", "CampoComparado" });

            migrationBuilder.CreateIndex(
                name: "IX_Divergencia_ResolvidoPorUsuarioId",
                table: "Divergencia",
                column: "ResolvidoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Divergencia_Status",
                table: "Divergencia",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoDivergencia_DivergenciaId",
                table: "HistoricoDivergencia",
                column: "DivergenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoDivergencia_UsuarioId",
                table: "HistoricoDivergencia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Loja_AuditorUsuarioId",
                table: "Loja",
                column: "AuditorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Loja_Cnpj",
                table: "Loja",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loja_CriadoPorUsuarioId",
                table: "Loja",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Loja_GerenteUsuarioId",
                table: "Loja",
                column: "GerenteUsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoUsuario_NomeTipoUsuario",
                table: "TipoUsuario",
                column: "NomeTipoUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_TipoUsuarioId",
                table: "Usuario",
                column: "TipoUsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoDivergencia");

            migrationBuilder.DropTable(
                name: "Divergencia");

            migrationBuilder.DropTable(
                name: "Comparacao");

            migrationBuilder.DropTable(
                name: "Loja");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "TipoUsuario");
        }
    }
}
