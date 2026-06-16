using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buscador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFragmentosAnimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE fragmentos_animal (
    id uuid NOT NULL,
    animal_id uuid NOT NULL,
    texto text NOT NULL,
    embedding vector(768) NULL,
    CONSTRAINT pk_fragmentos_animal PRIMARY KEY (id),
    CONSTRAINT fk_fragmentos_animal_animais FOREIGN KEY (animal_id)
        REFERENCES animais(id) ON DELETE CASCADE
);");
            migrationBuilder.Sql("CREATE INDEX ix_fragmentos_animal_animal_id ON fragmentos_animal (animal_id);");
            migrationBuilder.Sql("CREATE INDEX ix_fragmentos_animal_embedding ON fragmentos_animal USING hnsw (embedding vector_cosine_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fragmentos_animal_embedding;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_fragmentos_animal_animal_id;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS fragmentos_animal;");
        }
    }
}
