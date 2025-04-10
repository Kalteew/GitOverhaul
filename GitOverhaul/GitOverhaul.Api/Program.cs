using GitOverhaul.Api.Features.Git;
using GitOverhaul.Api.Features.OpenAi;
using GitOverhaul.Api.Middleware;
using GitOverhaul.Api.Tools;
using GitOverhaul.Domain.Services;
using GitOverhaul.Infra.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<SwaggerGenerator>();

var app = builder.Build();

// Génération dynamique au runtime si le fichier n'existe pas
var schemaPath = Path.Combine(app.Environment.WebRootPath!, "openai-actions.json");
if (!File.Exists(schemaPath))
{
    GenerateOpenAiSchema.Run();
}

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
openaiGroup.MapOpenAiSchema();

app.Run();