using GitOverhaul.Api.Middleware;
using GitOverhaul.Api.Tools;
using GitOverhaul.Domain.Services;
using GitOverhaul.Infra.Services;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

var app = builder.Build();

app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
app.UseMiddleware<ErrorMiddleware>();
app.UseSwaggerUI();

app.MapControllers();

app.UseStaticFiles();

// Génération automatique du fichier openai-actions.json au startup
var swaggerProvider = app.Services.GetRequiredService<ISwaggerProvider>();
GenerateOpenAiSchema.Run(swaggerProvider);

app.Run();