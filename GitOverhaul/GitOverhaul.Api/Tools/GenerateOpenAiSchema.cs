using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace GitOverhaul.Api.Tools;

public static class GenerateOpenAiSchema
{
    public static void Run()
    {
        var services = new ServiceCollection();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var provider = services.BuildServiceProvider();
        var swaggerGen = provider.GetRequiredService<SwaggerGenerator>();

        var doc = swaggerGen.GetSwagger("v1");

        // Adaptation OpenAI: version, server, etc.
        doc.OpenApi = new(new Version(3, 1, 0));
        doc.Info.Description = "API pour explorer et modifier des repositories Git à distance.";
        doc.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://gitoverhaul.onrender.com" }
        };

        // Write to file
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "openai-actions.json");
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        var openApiWriter = new OpenApiJsonWriter(writer);
        doc.SerializeAsV3(openApiWriter);
    }
}