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

// Start schema generation in background after app has started
Task.Run(() =>
{
    using var scope = app.Services.CreateScope();
    GenerateOpenAiSchema.Run(builder.Services);
});

app.Run();