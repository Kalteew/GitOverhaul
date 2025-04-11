using GitOverhaul.Api.Features.Git;
using GitOverhaul.Api.Features.OpenAi;
using GitOverhaul.Api.Middleware;
using GitOverhaul.Domain.Services;
using GitOverhaul.Infra.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Construction de l'application (runtime pipeline, endpoints, DI, etc)
var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });
app.UseMiddleware<ErrorMiddleware>();
app.UseSwaggerUI();

var gitGroup = app.MapGroup("/git");
gitGroup.MapGetStructure()
    .MapReadFile()
    .MapPushChanges()
    .MapCreateBranch();

var openaiGroup = app.MapGroup("/openai");
openaiGroup.MapOpenAiSchemaGenerator();

// Déclenche une requête HTTP locale vers /openai/schema après 10s en dev
// app.TriggerDelayedSchemaGeneration();

app.Run();