namespace GitOverhaul.Api.Models.Git;

public record CreateBranchRequest(
    string RepoUrl,
    string SourceBranch,
    string NewBranch,
    string? Token
);