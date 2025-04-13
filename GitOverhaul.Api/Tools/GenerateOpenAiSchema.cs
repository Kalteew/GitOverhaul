using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
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
        // Configuration de l'URL publique basée sur l'environnement
        var env = Environment.GetEnvironmentVariable("env")?.ToLowerInvariant();
        var publicUrl = env == "rec"
            ? "https://gitoverhaulrec.onrender.com"
            : "https://gitoverhaul.onrender.com";
        doc.Servers = new List<OpenApiServer>
        {
            new() { Url = publicUrl },
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

        // Inject parameter descriptions from XML docs
        TryInjectXmlComments(root);

        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (!Directory.Exists(wwwroot))
            Directory.CreateDirectory(wwwroot);

        var finalPath = Path.Combine(wwwroot, "openai-actions.json");
        File.WriteAllText(finalPath, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string GenerateOperationId(string path, string method)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        return method[..1].ToUpper() + method[1..] + string.Join("", segments.Select(Capitalize));
    }

    private static string Capitalize(string input)
        => input.Length == 0 ? input : char.ToUpper(input[0]) + input[1..];

    private static void TryInjectXmlComments(JsonObject root)
    {
        var basePath = AppContext.BaseDirectory;
        var xmlFile = Directory.GetFiles(basePath, "*.xml").FirstOrDefault();
        if (xmlFile == null || root["paths"] is not JsonObject paths) return;

        var doc = XDocument.Load(xmlFile);
        var summaries = doc.Descendants("member")
            .Where(m => m.Attribute("name")?.Value.StartsWith("P:") == true)
            .ToDictionary(
                m => m.Attribute("name")!.Value.Split(':')[1],
                m => m.Element("summary")?.Value.Trim()
            );

        foreach (var (_, pathVal) in paths)
        {
            if (pathVal is not JsonObject methods) continue;
            foreach (var (_, details) in methods)
            {
                if (details is not JsonObject obj || obj["parameters"] is not JsonArray parameters) continue;
                foreach (var param in parameters.OfType<JsonObject>())
                {
                    var name = param["name"]?.ToString();
                    if (name != null && summaries.TryGetValue(name, out var desc))
                        param["description"] = desc;
                }
            }
        }
    }
}