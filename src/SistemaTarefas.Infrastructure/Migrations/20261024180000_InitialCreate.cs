using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SistemaTarefas.Infrastructure.Persistencia;

#nullable disable

namespace SistemaTarefas.Infrastructure.Migrations;

[DbContext(typeof(TarefasDbContext))]
[Migration("20261024180000_InitialCreate")]
public class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase().Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "Usuarios",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                NomeUsuario = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                SenhaHash = table.Column<string>(type: "longtext", nullable: false),
                Papel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Usuarios_NomeUsuario", table: "Usuarios", column: "NomeUsuario", unique: true);

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                TokenHash = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                ExpiraEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                RevogadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey("FK_RefreshTokens_Usuarios_UsuarioId", x => x.UsuarioId, "Usuarios", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_TokenHash", table: "RefreshTokens", column: "TokenHash");
        migrationBuilder.CreateIndex(name: "IX_RefreshTokens_UsuarioId", table: "RefreshTokens", column: "UsuarioId");

        migrationBuilder.CreateTable(
            name: "Tarefas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Titulo = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                Descricao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                DataVencimento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                DataConclusao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Versao = table.Column<uint>(type: "int unsigned", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Tarefas", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Tarefas_UsuarioId_Status_DataVencimento",
            table: "Tarefas",
            columns: new[] { "UsuarioId", "Status", "DataVencimento" });

        migrationBuilder.CreateTable(
            name: "AuditoriaRegistros",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                TarefaId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                UsuarioId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                UsuarioNome = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                Acao = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                Detalhe = table.Column<string>(type: "longtext", nullable: true),
                OcorridoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AuditoriaRegistros", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_AuditoriaRegistros_TarefaId", table: "AuditoriaRegistros", column: "TarefaId");

        migrationBuilder.CreateTable(
            name: "ChavesIdempotencia",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                Chave = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                HashCorpo = table.Column<string>(type: "longtext", nullable: false),
                RespostaJson = table.Column<string>(type: "longtext", nullable: false),
                CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ExpiraEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ChavesIdempotencia", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ChavesIdempotencia_UsuarioId_Chave",
            table: "ChavesIdempotencia",
            columns: new[] { "UsuarioId", "Chave" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ChavesIdempotencia");
        migrationBuilder.DropTable("AuditoriaRegistros");
        migrationBuilder.DropTable("Tarefas");
        migrationBuilder.DropTable("RefreshTokens");
        migrationBuilder.DropTable("Usuarios");
    }
}
