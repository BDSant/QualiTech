using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OsLog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullEmpresaIdInUsuarioAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                schema: "Empresa",
                table: "UsuarioAcesso",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EmpresaId",
                schema: "Empresa",
                table: "UsuarioAcesso",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
