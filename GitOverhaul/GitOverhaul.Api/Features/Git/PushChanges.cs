using GitOverhaul.Api.Models.Git;
using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class PushChanges
{
    public static RouteGroupBuilder MapPushChanges(this RouteGroupBuilder group)
    {
        group.MapPost("/push", async (
            GitPushRequest request,
            string? token,
            IGitService gitService) =>
        {
            await gitService.PushChangesAsync(
                request.RepoUrl,
                request.Branch,
                request.FilePath,
                request.Content,
                request.AuthorName,
                request.AuthorEmail,
                request.CommitMessage,
                token
            );

            return Results.Ok();
        });

        return group;
    }
}