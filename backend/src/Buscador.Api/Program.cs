using Buscador.Application;
using Buscador.Api.Endpoints;
using Buscador.Api.TratamentoErros;
using Buscador.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AdicionarAplicacao();
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ManipuladorGlobalExcecoes>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(opcoes =>
{
    opcoes.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

const string PoliticaCors = "frontend";
var origemFrontend = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:3000";
builder.Services.AddCors(opcoes => opcoes.AddPolicy(PoliticaCors, politica => politica
    .WithOrigins(origemFrontend).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(PoliticaCors);

app.MapOpenApi();
app.MapScalarApiReference();

app.MapearEndpointsAnimais();

app.Run();
