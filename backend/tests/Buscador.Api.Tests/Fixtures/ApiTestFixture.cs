using Buscador.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Buscador.Api.Tests.Fixtures;

public class ApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithPassword("buscador")
        .WithUsername("buscador")
        .WithDatabase("buscador")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ContextoBanco>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ContextoBanco>(opcoes =>
            {
                opcoes.UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseVector());
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task ApplyMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoBanco>();
        await contexto.Database.MigrateAsync();
    }
}
