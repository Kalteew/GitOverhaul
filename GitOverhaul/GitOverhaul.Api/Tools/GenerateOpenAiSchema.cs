using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;

namespace GitOverhaul.Api.Tools;

public static class GenerateOpenAiSchema
{
    public static void Run(ISwaggerProvider provider)
    {
        var doc = provider.GetSwagger("v1");

        doc.Info.Description = "API pour explorer et modifier des repositories Git à distance.";
        doc.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://gitoverhaul.onrender.com" }
        };

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