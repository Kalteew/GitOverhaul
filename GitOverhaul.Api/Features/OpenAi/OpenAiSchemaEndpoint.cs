using GitOverhaul.Api.Tools;
using Swashbuckle.AspNetCore.Swagger;

namespace GitOverhaul.Api.Features.OpenAi;

public static class OpenAiSchemaEndpoint
{
    public static RouteGroupBuilder MapOpenAiSchemaGenerator(this RouteGroupBuilder group)
    {
        group.MapGet("/schema", (ISwaggerProvider provider) =>
        {
            GenerateOpenAiSchema.Run(provider);
            return Results.Ok();
        });

        return group;
    }
}