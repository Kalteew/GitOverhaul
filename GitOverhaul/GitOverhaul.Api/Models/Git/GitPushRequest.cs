namespace GitOverhaul.Api.Models.Git;

public record GitPushRequest(
    string RepoUrl,
    string Branch,
    string FilePath,
    string Content,
    string AuthorName,
    string AuthorEmail,
    string CommitMessage);