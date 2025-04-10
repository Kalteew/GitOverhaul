using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class GetStructure
{
    public static RouteGroupBuilder MapGetStructure(this RouteGroupBuilder group)
    {
        group.MapGet("/structure", async (
            string repoUrl,
            string branch,
            string? token,
            IGitService gitService) =>
        {
            var structure = await gitService.GetRepositoryStructureAsync(repoUrl, branch, token);
            return Results.Ok(structure);
        });


        return group;
    }
}