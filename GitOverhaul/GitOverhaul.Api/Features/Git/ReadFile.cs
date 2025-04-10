using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class ReadFile
{
    public static RouteGroupBuilder MapReadFile(this RouteGroupBuilder group)
    {
        group.MapGet("/read", async (
            string repoUrl,
            string branch,
            string filePath,
            string? token,
            IGitService gitService) =>
        {
            var content = await gitService.ReadFileAsync(repoUrl, branch, filePath, token);
            return Results.Ok(content);
        });

        return group;
    }
}