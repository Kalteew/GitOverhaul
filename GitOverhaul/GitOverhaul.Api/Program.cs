using GitOverhaul.Api.Features.Git;
using GitOverhaul.Api.Middleware;
using GitOverhaul.Domain.Services;
using GitOverhaul.Infra.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGitService, GitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseMiddleware<ErrorMiddleware>();
app.UseSwaggerUI();

var gitGroup = app.MapGroup("/git");
gitGroup.MapGetStructure()
    .MapReadFile()
    .MapPushChanges();

app.Run();