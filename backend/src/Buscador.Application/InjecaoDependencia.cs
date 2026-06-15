using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Application;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(InjecaoDependencia).Assembly));
        services.AddValidatorsFromAssembly(typeof(InjecaoDependencia).Assembly);

        return services;
    }
}
