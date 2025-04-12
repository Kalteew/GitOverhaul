using Swashbuckle.AspNetCore.Annotations;

namespace GitOverhaul.Api.Models.Git;

public record FilePushData(
    [property: SwaggerSchema(Description = "Chemin relatif du fichier dans le dépôt.")]
    string FilePath,

    [property: SwaggerSchema(Description = "Contenu complet du fichier. Ce n'est pas un diff, mais le fichier entier.")]
    string? Content,

    [property: SwaggerSchema(Description = "Si vrai, le fichier sera supprimé.")]
    bool IsDeletion
);

public record GitPushMultipleRequest(
    string RepoUrl,
    string Branch,
    string? Token,
    string AuthorName,
    string AuthorEmail,
    string CommitMessage,
    List<FilePushData> Files
) : GitRequest(RepoUrl, Branch, Token);