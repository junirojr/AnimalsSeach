using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buscador.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GatilhoVetorBusca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql(@"
CREATE TRIGGER tg_atualizar_vetor_busca
BEFORE INSERT OR UPDATE ON animais
FOR EACH ROW EXECUTE FUNCTION fn_atualizar_vetor_busca();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tg_atualizar_vetor_busca ON animais;");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_atualizar_vetor_busca;");
        }
    }
}
