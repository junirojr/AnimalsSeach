using Buscador.Application;
using Buscador.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
