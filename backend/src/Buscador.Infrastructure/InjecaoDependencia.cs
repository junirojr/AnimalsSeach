using Buscador.Application.Compartilhado;
using Buscador.Domain.Animais;
using Buscador.Infrastructure.Busca;
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

		servicos.AddScoped<IRepositorioAnimal, RepositorioAnimal>();
		servicos.AddScoped<IServicoBuscaTextual, ServicoBuscaTextual>();

		return servicos;
	}
}
