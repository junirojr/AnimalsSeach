using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Buscador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "animais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_comum = table.Column<string>(type: "text", nullable: false),
                    nome_cientifico = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    caracteristicas = table.Column<string>(type: "text", nullable: false),
                    dieta = table.Column<string>(type: "text", nullable: false),
                    habitat = table.Column<string>(type: "text", nullable: false),
                    distribuicao_geografica = table.Column<string>(type: "text", nullable: false),
                    status_conservacao = table.Column<string>(type: "text", nullable: false),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    curiosidades = table.Column<string>(type: "text", nullable: false),
                    embedding = table.Column<string>(type: "vector(768)", nullable: true),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_animais", x => x.id);
                });

            // Índice GIN para busca full-text em search_vector
            migrationBuilder.Sql(
                "CREATE INDEX ix_animais_search_vector ON animais USING GIN (search_vector);");

            // Índice HNSW para busca semântica em embedding (cosine distance)
            migrationBuilder.Sql(
                "CREATE INDEX ix_animais_embedding ON animais USING hnsw (embedding vector_cosine_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_animais_embedding;");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS ix_animais_search_vector;");

            migrationBuilder.DropTable(
                name: "animais");
        }
    }
}
