namespace Buscador.Application;

public static class DependencyInjection
{
    public static IServiceCollection AdicionarAplicacao(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamentoValidacao<,>));

        return services;
    }
}
