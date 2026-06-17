using Buscador.Application.Comportamentos;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Buscador.Application;

public static class InjecaoDependencia
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(InjecaoDependencia).Assembly));
        services.AddValidatorsFromAssembly(typeof(InjecaoDependencia).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamentoValidacao<,>));

        return services;
    }
}
