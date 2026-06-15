using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Application;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
