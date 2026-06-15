using Buscador.Domain.Animais;
using Microsoft.EntityFrameworkCore;

namespace Buscador.Infrastructure.Persistencia;

public class ContextoBanco : DbContext
{
	public required DbSet<Animal> Animais { get; init; }

	public ContextoBanco(DbContextOptions<ContextoBanco> opcoes) : base(opcoes)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.HasPostgresExtension("vector");

		modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
	}
}
