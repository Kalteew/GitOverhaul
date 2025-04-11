using GitOverhaul.Domain.Services;
using GitOverhaul.Api.Models.Git;
using Microsoft.AspNetCore.Mvc;

namespace GitOverhaul.Api.Controllers;

[ApiController]
[Route("git")]
public class GitController(IGitService gitService) : ControllerBase
{
    [HttpGet("structure")]
    public IResult GetStructure([FromQuery] string repoUrl, [FromQuery] string branch, [FromQuery] string? token)
    {
        var result = gitService.GetStructure(repoUrl, branch, token);
        return Results.Ok(result);
    }

    [HttpGet("read")]
    public IResult Read([FromQuery] string repoUrl, [FromQuery] string branch, [FromQuery] string filePath, [FromQuery] string? token)
    {
        var result = gitService.ReadFile(repoUrl, branch, filePath, token);
        return Results.Ok(result);
    }

    [HttpPost("push")]
    public IResult Push([FromQuery] string? token, [FromBody] GitPushRequest request)
    {
        var result = gitService.PushFile(request with { Token = token });
        return Results.Ok(result);
    }

    [HttpPost("create-branch")]
    public IResult CreateBranch([FromBody] CreateBranchRequest request)
    {
        var result = gitService.CreateBranch(request);
        return Results.Ok(result);
    }
}