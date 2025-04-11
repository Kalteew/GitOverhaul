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
        doc.Info.Version = "3.1.0";

        var stream = new MemoryStream();

        using (var writer = new StreamWriter(stream, leaveOpen: true)) {
            var openApiWriter = new OpenApiJsonWriter(writer);
            doc.SerializeAsV3(openApiWriter);
            writer.Flush();
        }

        stream.Position = 0;
        using var jsonDoc = JsonDocument.Parse(stream);
        var root = JsonNode.Parse(jsonDoc.RootElement.GetRawText())!.AsObject();

        // Fix version & cleanup
        root.Remove("swagger");
        root["openapi"] = "3.1.0";

        // Patch operationId per endpoint
        if (root["paths"] is JsonObject paths)
        {
            foreach (var (pathKey, pathVal) in paths)
            {
                if (pathVal is JsonObject methods)
                {
                    foreach (var (method, details) in methods)
                    {
                        if (details is JsonObject obj && !obj.ContainsKey("operationId"))
                        {
                            var opId = GenerateOperationId(pathKey, method);
                            obj["operationId"] = opId;
                        }
                    }
                }
            }
        }

        var finalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "openai-actions.json");
        File.WriteAllText(finalPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string GenerateOperationId(string path, string method)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return method[..1].ToUpper() + method[1..] + string.Join("", segments.Select(Capitalize));
    }

    private static string Capitalize(string input)
        => input.Length == 0 ? input : char.ToUpper(input[0]) + input[1..];
}