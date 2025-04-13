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
        if (string.IsNullOrWhiteSpace(repoUrl) || string.IsNullOrWhiteSpace(branch))
            return BadRequest("Le repoUrl et la branche sont obligatoires.");

        try
        {
            var result = await gitService.GetRepositoryStructureAsync(repoUrl, branch, token);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("read-multiple")]
    [SwaggerOperation(Summary = "Lire plusieurs fichiers", Description = "Lit le contenu de plusieurs fichiers dans un dépôt Git.")]
    public async Task<IActionResult> ReadMultiple([FromBody] ReadMultipleFilesRequest request)
    {
        if (request.FilePaths == null || request.FilePaths.Count == 0)
            return BadRequest("La liste de fichiers ne peut pas être vide.");

        var results = new Dictionary<string, string>();
        var notFound = new List<string>();

        foreach (var filePath in request.FilePaths)
        {
            try
            {
                var content = await gitService.ReadFileAsync(request.RepoUrl, request.Branch, filePath, request.Token);
                results[filePath] = content;
            }
            catch
            {
                notFound.Add(filePath);
            }
        }

        if (notFound.Any())
        {
            return NotFound(new
            {
                Message = "Certains fichiers sont introuvables.",
                Missing = notFound,
                Found = results
            });
        }

        return Ok(results);
    }

    [HttpPost("push-multiple")]
    [SwaggerOperation(Summary = "Pousser plusieurs changements", Description = "Crée, modifie ou supprime plusieurs fichiers dans un dépôt Git.")]
    public async Task<IActionResult> PushMultiple([FromBody] GitPushMultipleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AuthorName) || string.IsNullOrWhiteSpace(request.AuthorEmail))
            return BadRequest("Le nom et l'email de l'auteur sont obligatoires.");

        if (string.IsNullOrWhiteSpace(request.CommitMessage))
            return BadRequest("Un message de commit est requis.");

        var changes = request.Files
            .Select(f => (f.FilePath, f.Content, f.IsDeletion))
            .ToList();

        try
        {
            await gitService.PushMultipleChangesAsync(request.RepoUrl, request.Branch, changes, request.AuthorName, request.AuthorEmail, request.CommitMessage, request.Token);
            return Ok(new
            {
                Message = "Modifications poussées avec succès.",
                Files = request.Files
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("build")]
    [SwaggerOperation(Summary = "Builder le dépôt", Description = "Exécute une commande dotnet build sur le dépôt.")]
    public async Task<IActionResult> Build([FromBody] GitRequest request)
    {
        try
        {
            await gitService.Build(request.RepoUrl, request.Branch, request.Token);
            return Ok(new { Message = "Build effectué avec succès." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("create-branch")]
    [SwaggerOperation(Summary = "Créer une branche", Description = "Crée une nouvelle branche à partir d'une autre.")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewBranch))
            return BadRequest("Le nom de la nouvelle branche est requis.");

        if (request.NewBranch.Any(c => " @#~!?&$%".Contains(c)))
            return BadRequest("Le nom de la branche contient des caractères invalides.");

        try
        {
            await gitService.CreateBranchAsync(request.RepoUrl!, request.SourceBranch!, request.NewBranch!, request.Token);
            return Ok(new { Message = "Branche créée avec succès.", Branch = request.NewBranch });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("run-git-commands")]
    [SwaggerOperation(Summary = "Exécuter des commandes Git", Description = "Exécute une liste de commandes Git dans l'ordre donné. Retourne les logs ou une erreur détaillée.")]
    public async Task<IActionResult> RunGitCommands([FromBody] RunGitCommandsRequest request)
    {
        try
        {
            var results = await gitService.RunGitCommandsAsync(request.RepoUrl, request.Branch, request.Commands, request.Token);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}