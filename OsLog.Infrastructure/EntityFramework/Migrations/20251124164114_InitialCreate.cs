using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsLog.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "OrdemServico");

            migrationBuilder.EnsureSchema(
                name: "Clientes");

            migrationBuilder.EnsureSchema(
                name: "Empresa");

            migrationBuilder.CreateTable(
                name: "Cliente",
                schema: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                schema: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RazaoSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tecnico",
                schema: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Apelido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tecnico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Unidade",
                schema: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    InscricaoEstadual = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InscricaoMunicipal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unidade_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "Empresa",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdemServico",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<int>(type: "int", nullable: true),
                    DescricaoProblema = table.Column<string>(type: "varchar(max)", maxLength: 1000, nullable: false),
                    StatusOs = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusOrcamento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SinalObrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    SinalPago = table.Column<bool>(type: "bit", nullable: false),
                    ValorSinal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorOrcamentoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServico_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "Clientes",
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdemServico_Empresa_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "Empresa",
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdemServico_Tecnico_TecnicoId",
                        column: x => x.TecnicoId,
                        principalSchema: "Empresa",
                        principalTable: "Tecnico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdemServico_Unidade_UnidadeId",
                        column: x => x.UnidadeId,
                        principalSchema: "Empresa",
                        principalTable: "Unidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Acessorio",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Devolvido = table.Column<bool>(type: "bit", nullable: false),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acessorio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acessorio_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComissaoTecnico",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    TecnicoId = table.Column<int>(type: "int", nullable: false),
                    TipoBase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Percentual = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ValorBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorComissao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Liquidado = table.Column<bool>(type: "bit", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataLiquidacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComissaoTecnico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComissaoTecnico_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComissaoTecnico_Tecnico_TecnicoId",
                        column: x => x.TecnicoId,
                        principalSchema: "Empresa",
                        principalTable: "Tecnico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrcamentoItem",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    TipoItem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescricaoItem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentoItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrcamentoItem_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdemServicoFoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Foto = table.Column<byte[]>(type: "VARBINARY(MAX)", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlteradoPor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoFoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoFoto_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagamentoOs",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    TipoPagamento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusRegistro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlExcluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentoOs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentoOs_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusHistorico",
                schema: "OrdemServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdemServicoId = table.Column<int>(type: "int", nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StatusOsAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusOsNovo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DescricaoEvento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataEvento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusHistorico_OrdemServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalSchema: "OrdemServico",
                        principalTable: "OrdemServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acessorio_OrdemServicoId",
                schema: "OrdemServico",
                table: "Acessorio",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComissaoTecnico_OrdemServicoId",
                schema: "OrdemServico",
                table: "ComissaoTecnico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComissaoTecnico_TecnicoId",
                schema: "OrdemServico",
                table: "ComissaoTecnico",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItem_OrdemServicoId",
                schema: "OrdemServico",
                table: "OrcamentoItem",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_ClienteId",
                schema: "OrdemServico",
                table: "OrdemServico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_EmpresaId",
                schema: "OrdemServico",
                table: "OrdemServico",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_TecnicoId",
                schema: "OrdemServico",
                table: "OrdemServico",
                column: "TecnicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServico_UnidadeId",
                schema: "OrdemServico",
                table: "OrdemServico",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoFoto_OrdemServicoId_FlExcluido",
                table: "OrdemServicoFoto",
                columns: new[] { "OrdemServicoId", "FlExcluido" });

            migrationBuilder.CreateIndex(
                name: "IX_PagamentoOs_OrdemServicoId",
                schema: "OrdemServico",
                table: "PagamentoOs",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistorico_OrdemServicoId",
                schema: "OrdemServico",
                table: "StatusHistorico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Unidade_EmpresaId_Cnpj",
                schema: "Empresa",
                table: "Unidade",
                columns: new[] { "EmpresaId", "Cnpj" },
                unique: true,
                filter: "[Cnpj] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acessorio",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "ComissaoTecnico",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "OrcamentoItem",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "OrdemServicoFoto");

            migrationBuilder.DropTable(
                name: "PagamentoOs",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "StatusHistorico",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "OrdemServico",
                schema: "OrdemServico");

            migrationBuilder.DropTable(
                name: "Cliente",
                schema: "Clientes");

            migrationBuilder.DropTable(
                name: "Tecnico",
                schema: "Empresa");

            migrationBuilder.DropTable(
                name: "Unidade",
                schema: "Empresa");

            migrationBuilder.DropTable(
                name: "Empresa",
                schema: "Empresa");
        }
    }
}
