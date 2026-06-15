using Buscador.Domain.Animais;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Buscador.Infrastructure.Persistencia.Configuracoes;

public class AnimalConfiguracao : IEntityTypeConfiguration<Animal>
{
	public void Configure(EntityTypeBuilder<Animal> builder)
	{
		builder.ToTable("animais");

		builder.HasKey(a => a.Id);

		builder.Property(a => a.Id)
			.HasConversion(id => id.Valor, valor => AnimalId.De(valor))
			.HasColumnName("id");

		builder.Property(a => a.NomeComum)
			.IsRequired()
			.HasColumnName("nome_comum")
			.HasColumnType("text");

		builder.Property(a => a.NomeCientifico)
			.IsRequired()
			.HasColumnName("nome_cientifico")
			.HasColumnType("text");

		builder.Property(a => a.Descricao)
			.IsRequired()
			.HasColumnName("descricao")
			.HasColumnType("text");

		builder.Property(a => a.Caracteristicas)
			.IsRequired()
			.HasColumnName("caracteristicas")
			.HasColumnType("text");

		builder.Property(a => a.Dieta)
			.IsRequired()
			.HasColumnName("dieta")
			.HasConversion<string>();

		builder.Property(a => a.Habitat)
			.IsRequired()
			.HasColumnName("habitat")
			.HasConversion<string>();

		builder.Property(a => a.DistribuicaoGeografica)
			.IsRequired()
			.HasColumnName("distribuicao_geografica")
			.HasColumnType("text");

		builder.Property(a => a.StatusConservacao)
			.IsRequired()
			.HasColumnName("status_conservacao")
			.HasConversion<string>();

		builder.Property(a => a.Tags)
			.IsRequired()
			.HasColumnName("tags")
			.HasColumnType("text[]");

		builder.Property(a => a.Curiosidades)
			.IsRequired()
			.HasColumnName("curiosidades")
			.HasColumnType("text");

		// Shadow properties (não aparecem na classe Animal)
		builder.Property<NpgsqlTsVector>("VetorBusca")
			.HasColumnName("search_vector")
			.HasColumnType("tsvector");
	}
}
