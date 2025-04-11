using GitOverhaul.Api.Models.Git;
using GitOverhaul.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace GitOverhaul.Api.Controllers;

[ApiController]
[Route("api/git")]
public class GitController(IGitService gitService) : ControllerBase
{
    [HttpGet("structure")]
    public async Task<IActionResult> GetStructure(string repoUrl, string branch, string? token)
    {
        var result = await gitService.GetRepositoryStructureAsync(repoUrl, branch, token);
        return Ok(result);
    }

    [HttpGet("read")]
    public async Task<IActionResult> Read(string repoUrl, string branch, string filePath, string? token)
    {
        var result = await gitService.ReadFileAsync(repoUrl, branch, filePath, token);
        return Ok(result);
    }

    [HttpPost("read-multiple")]
    public async Task<IActionResult> ReadMultiple([FromBody] ReadMultipleFilesRequest request)
    {
        var results = new Dictionary<string, string>();

        foreach (var filePath in request.FilePaths) {
            var content = await gitService.ReadFileAsync(
                request.RepoUrl, request.Branch, filePath, request.Token
            );
            results[filePath] = content;
        }

        return Ok(results);
    }

    [HttpPost("push")]
    public async Task<IActionResult> Push([FromQuery] string? token, [FromBody] GitPushRequest request)
    {
        await gitService.PushChangesAsync(
            request.RepoUrl!, request.Branch!, request.FilePath!,
            request.Content!, request.AuthorName!, request.AuthorEmail!,
            request.CommitMessage!, token
        );
        return Ok();
    }


    [HttpPost("build")]
    public async Task<IActionResult> Build([FromBody] GitRequest request)
    {
        await gitService.Build(request.RepoUrl, request.Branch, request.Token);

        return Ok();
    }

    [HttpPost("push-multiple")]
    public async Task<IActionResult> PushMultiple([FromBody] GitPushMultipleRequest request)
    {
        var changes = request.Files
            .Select(f => (f.FilePath, f.Content))
            .ToList();

        await gitService.PushMultipleChangesAsync(
            request.RepoUrl, request.Branch,
            changes,
            request.AuthorName, request.AuthorEmail,
            request.CommitMessage, request.Token
        );

        return Ok();
    }

    [HttpPost("create-branch")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        await gitService.CreateBranchAsync(
            request.RepoUrl!, request.SourceBranch!, request.NewBranch!,
            request.Token
        );
        return Ok();
    }
}