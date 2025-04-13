using Swashbuckle.AspNetCore.Annotations;

namespace GitOverhaul.Api.Models.Git;

public record RunGitCommandsRequest(
    [property: SwaggerSchema(Description = "URL du dépôt Git à utiliser")] string RepoUrl,
    [property: SwaggerSchema(Description = "Branche cible pour l'exécution des commandes")] string Branch,
    [property: SwaggerSchema(Description = "Jeton d'accès (si nécessaire)")] string? Token,
    [property: SwaggerSchema(Description = "Liste des commandes Git à exécuter dans l'ordre")] List<string> Commands
);