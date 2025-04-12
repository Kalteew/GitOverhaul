using GitOverhaul.Api.Models.Git;
using GitOverhaul.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GitOverhaul.Api.Controllers;

[ApiController]
[Route("api/git")]
public class GitController(IGitService gitService) : ControllerBase
{
    [HttpGet("structure")]
    [SwaggerOperation(Summary = "Récupère la structure d'un dépôt Git", Description = "Retourne l'arborescence du dépôt pour une branche donnée.")]
    public async Task<IActionResult> GetStructure(string repoUrl, string branch, string? token)
    {
        var result = await gitService.GetRepositoryStructureAsync(repoUrl, branch, token);
        return Ok(result);
    }

    [HttpPost("read-multiple")]
    [SwaggerOperation(Summary = "Lire plusieurs fichiers", Description = "Lit le contenu de plusieurs fichiers dans un dépôt Git.")]
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

    [HttpPost("push-multiple")]
    [SwaggerOperation(Summary = "Pousser plusieurs changements", Description = "Crée, modifie ou supprime plusieurs fichiers dans un dépôt Git.")]
    public async Task<IActionResult> PushMultiple([FromBody] GitPushMultipleRequest request)
    {
        var changes = request.Files
            .Select(f => (f.FilePath, f.Content, f.IsDeletion))
            .ToList();

        await gitService.PushMultipleChangesAsync(
            request.RepoUrl, request.Branch,
            changes,
            request.AuthorName, request.AuthorEmail,
            request.CommitMessage, request.Token
        );

        return Ok();
    }

    [HttpPost("build")]
    [SwaggerOperation(Summary = "Builder le dépôt", Description = "Exécute une commande dotnet build sur le dépôt.")]
    public async Task<IActionResult> Build([FromBody] GitRequest request)
    {
        await gitService.Build(request.RepoUrl, request.Branch, request.Token);
        return Ok();
    }

    [HttpPost("create-branch")]
    [SwaggerOperation(Summary = "Créer une branche", Description = "Crée une nouvelle branche à partir d'une autre.")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        await gitService.CreateBranchAsync(
            request.RepoUrl!, request.SourceBranch!, request.NewBranch!,
            request.Token
        );
        return Ok();
    }
}