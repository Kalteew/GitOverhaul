using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class ReadFile
{
    public static RouteGroupBuilder MapReadFile(this RouteGroupBuilder group)
    {
        group.MapGet("/file", async (
            string repoUrl,
            string branch,
            string path,
            IGitService gitService) =>
        {
            var content = await gitService.ReadFileAsync(repoUrl, branch, path);
            return content is null ? Results.NotFound() : Results.Ok(content);
        });
        return group;
    }
}