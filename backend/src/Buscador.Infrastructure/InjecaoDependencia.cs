using Buscador.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Infrastructure;

public static class InjecaoDependencia
{
	public static IServiceCollection AdicionarInfraestrutura(
		this IServiceCollection servicos,
		IConfiguration configuracao)
	{
		var cadeiaConexao = configuracao.GetConnectionString("Postgres")
			?? throw new InvalidOperationException("Connection string 'Postgres' não foi configurada.");

		servicos.AddDbContext<ContextoBanco>((_, opcoes) =>
		{
			opcoes.UseNpgsql(cadeiaConexao, npgsql => npgsql.UseVector());
		});

		// TODO: Descomentar após criar RepositorioAnimal em T2.7
		// servicos.AddScoped<IRepositorioAnimal, RepositorioAnimal>();

		return servicos;
	}
}
