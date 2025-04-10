using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GitOverhaul.Api.Tools;

public static class GenerateOpenAiSchema
{
    public static void Run(IServiceProvider services)
    {
        var swaggerGen = services.GetRequiredService<SwaggerGenerator>();
        var doc = swaggerGen.GetSwagger("v1");

        // Ajout de la description et du serveur pour OpenAI
        doc.Info.Description = "API pour explorer et modifier des repositories Git à distance.";
        doc.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://gitoverhaul.onrender.com" }
        };

        // Sérialisation dans un JsonNode pour injecter manuellement openapi: "3.1.0"
        var stream = new MemoryStream();

        using (var writer = new StreamWriter(stream, leaveOpen: true)) {
            var openApiWriter = new OpenApiJsonWriter(writer);
            doc.SerializeAsV3(openApiWriter);
            writer.Flush();
        }

        stream.Position = 0;
        using var jsonDoc = JsonDocument.Parse(stream);
        var root = JsonNode.Parse(jsonDoc.RootElement.GetRawText())!.AsObject();
        root["openapi"] = "3.1.0";

        var finalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "openai-actions.json");
        File.WriteAllText(finalPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}