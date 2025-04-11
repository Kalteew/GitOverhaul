using GitOverhaul.Api.Features.Git;
using GitOverhaul.Api.Features.OpenAi;
using GitOverhaul.Api.Middleware;
using GitOverhaul.Api.Tools;
using GitOverhaul.Domain.Services;
using GitOverhaul.Infra.Services;
using Swashbuckle.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Construction de l'application (runtime pipeline, endpoints, DI, etc)
var app = builder.Build();

app.UseStaticFiles();
app.UseSwagger();
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
app.TriggerDelayedSchemaGeneration();

app.Run();