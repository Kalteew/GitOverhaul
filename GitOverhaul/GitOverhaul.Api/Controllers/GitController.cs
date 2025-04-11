using System.Threading.Tasks;
using GitOverhaul.Domain.Services;
using GitOverhaul.Api.Models.Git;
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