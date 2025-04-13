using Swashbuckle.AspNetCore.Annotations;

namespace GitOverhaul.Api.Models.Git;

public record RunGitCommandsRequest(
    string RepoUrl,
    string Branch,
    string? Token,

    [property: SwaggerSchema(Description = "Liste des commandes Git à exécuter dans l'ordre")] 
    List<string> Commands
) : GitRequest(RepoUrl, Branch, Token);