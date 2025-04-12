namespace GitOverhaul.Api.Models.Git;

public record GitRequest(
    string RepoUrl,
    string Branch,
    string? Token
);