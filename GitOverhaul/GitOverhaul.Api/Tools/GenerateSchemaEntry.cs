using GitOverhaul.Api.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;

Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    })
    .Build()
    .Services
    .GetRequiredService<ISwaggerProvider>()
    .Let(GenerateOpenAiSchema.Run);