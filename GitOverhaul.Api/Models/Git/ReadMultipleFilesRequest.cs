namespace GitOverhaul.Api.Models.Git;

public record ReadMultipleFilesRequest(
    string RepoUrl,
    string Branch,
    string? Token,
    List<string> FilePaths
) : GitRequest(RepoUrl, Branch, Token);