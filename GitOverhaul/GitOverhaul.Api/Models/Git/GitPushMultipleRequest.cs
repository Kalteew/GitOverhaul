namespace GitOverhaul.Api.Models.Git;

public record FilePushData(
    string FilePath,
    string? Content // null = delete
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