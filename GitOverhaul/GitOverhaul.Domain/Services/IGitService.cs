namespace GitOverhaul.Domain.Services;

public interface IGitService
{
    Task<object> GetRepositoryStructureAsync(string repoUrl, string branch, string? token = null);
    Task<string> ReadFileAsync(string repoUrl, string branch, string filePath, string? token = null);

    Task PushChangesAsync(string repoUrl,
        string branch,
        string filePath,
        string content,
        string authorName,
        string authorEmail,
        string commitMessage,
        string? token = null);

    Task CreateBranchAsync(string repoUrl, string sourceBranch, string newBranch, string? token = null);
}