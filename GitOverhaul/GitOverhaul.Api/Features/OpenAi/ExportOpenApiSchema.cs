using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GitOverhaul.Api.Features.OpenAi;

public static class ExportOpenApiSchema
{
    public static RouteGroupBuilder MapOpenAiSchema(this RouteGroupBuilder group)
    {
        group.MapGet("/schema", ([FromServices] SwaggerGenerator swaggerGen) =>
        {
            OpenApiDocument doc = swaggerGen.GetSwagger("v1");
            return Results.Ok(doc);
        });

        return group;
    }
}