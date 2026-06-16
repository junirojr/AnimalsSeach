using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buscador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecallFtsUnaccentMaisCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_atualizar_vetor_busca()
RETURNS trigger AS $$
BEGIN
  NEW.search_vector :=
    to_tsvector('portuguese', unaccent(
      coalesce(NEW.nome_comum, '') || ' ' ||
      coalesce(NEW.nome_cientifico, '') || ' ' ||
      coalesce(NEW.descricao, '') || ' ' ||
      coalesce(NEW.caracteristicas, '') || ' ' ||
      coalesce(NEW.curiosidades, '') || ' ' ||
      coalesce(NEW.distribuicao_geografica, '') || ' ' ||
      coalesce(array_to_string(NEW.tags, ' '), '')
    ));
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;
");
            // Reindexa os animais existentes (dispara o trigger BEFORE UPDATE)
            migrationBuilder.Sql("UPDATE animais SET nome_comum = nome_comum;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION fn_atualizar_vetor_busca()
RETURNS trigger AS $$
BEGIN
  NEW.search_vector :=
    to_tsvector('portuguese',
      coalesce(NEW.nome_comum, '') || ' ' ||
      coalesce(NEW.nome_cientifico, '') || ' ' ||
      coalesce(NEW.descricao, '') || ' ' ||
      coalesce(NEW.caracteristicas, '') || ' ' ||
      coalesce(NEW.curiosidades, '')
    );
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;
");
            migrationBuilder.Sql("UPDATE animais SET nome_comum = nome_comum;");
        }
    }
}
