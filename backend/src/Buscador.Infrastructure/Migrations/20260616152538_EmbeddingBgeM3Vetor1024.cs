using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buscador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmbeddingBgeM3Vetor1024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_animais_embedding;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fragmentos_animal_embedding;");
            // Limpa vetores antigos (768 dim) — serao regenerados com bge-m3 (1024 dim)
            migrationBuilder.Sql("DELETE FROM fragmentos_animal;");
            migrationBuilder.Sql("UPDATE animais SET embedding = NULL;");
            migrationBuilder.Sql("ALTER TABLE animais ALTER COLUMN embedding TYPE vector(1024);");
            migrationBuilder.Sql("ALTER TABLE fragmentos_animal ALTER COLUMN embedding TYPE vector(1024);");
            migrationBuilder.Sql("CREATE INDEX ix_animais_embedding ON animais USING hnsw (embedding vector_cosine_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_fragmentos_animal_embedding ON fragmentos_animal USING hnsw (embedding vector_cosine_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_animais_embedding;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fragmentos_animal_embedding;");
            migrationBuilder.Sql("DELETE FROM fragmentos_animal;");
            migrationBuilder.Sql("UPDATE animais SET embedding = NULL;");
            migrationBuilder.Sql("ALTER TABLE animais ALTER COLUMN embedding TYPE vector(768);");
            migrationBuilder.Sql("ALTER TABLE fragmentos_animal ALTER COLUMN embedding TYPE vector(768);");
            migrationBuilder.Sql("CREATE INDEX ix_animais_embedding ON animais USING hnsw (embedding vector_cosine_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_fragmentos_animal_embedding ON fragmentos_animal USING hnsw (embedding vector_cosine_ops);");
        }
    }
}
