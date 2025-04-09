using GitOverhaul.Api.Models.Git;
using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class PushChanges
{
    public static RouteGroupBuilder MapPushChanges(this RouteGroupBuilder group)
    {
        group.MapPost("/push", async (
            GitPushRequest req,
            IGitService gitService) =>
        {
            var result = await gitService.PushChangesAsync(
                req.RepoUrl,
                req.Branch,
                req.FilePath,
                req.NewContent,
                req.CommitMessage);

            return result ? Results.Ok("Changes pushed.") : Results.Problem("Push failed.");
        });
        return group;
    }
}