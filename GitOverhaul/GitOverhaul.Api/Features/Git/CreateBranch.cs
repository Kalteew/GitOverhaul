using GitOverhaul.Api.Models.Git;
using GitOverhaul.Domain.Services;

namespace GitOverhaul.Api.Features.Git;

public static class CreateBranch
{
    public static RouteGroupBuilder MapCreateBranch(this RouteGroupBuilder group)
    {
        group.MapPost("/create-branch", async (
            IGitService gitService,
            CreateBranchRequest request) =>
        {
            await gitService.CreateBranchAsync(request.RepoUrl, request.SourceBranch, request.NewBranch, request.Token);
            return Results.Ok();
        });

        return group;
    }
}