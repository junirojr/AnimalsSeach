using Buscador.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AdicionarInfraestrutura(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
