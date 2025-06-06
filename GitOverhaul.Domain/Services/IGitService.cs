namespace GitOverhaul.Domain.Services;

public interface IGitService
{
    Task<object> GetRepositoryStructureAsync(string repoUrl, string branch, string? token = null);
    Task<string> ReadFileAsync(string repoUrl, string branch, string filePath, string? token = null);

    // [REMOVE] Méthode obsolète : PushChangesAsync
    // Task PushChangesAsync(
    //     string repoUrl,
    //     string branch,
    //     string filePath,
    //     string? content,
    //     string authorName,
    //     string authorEmail,
    //     string commitMessage,
    //     string? token = null);

    Task PushMultipleChangesAsync(
        string repoUrl,
        string branch,
        List<(string FilePath, string? Content, bool IsDeletion)> changes,
        string authorName,
        string authorEmail,
        string commitMessage,
        string? token = null);

    Task CreateBranchAsync(string repoUrl, string sourceBranch, string newBranch, string? token = null);
    Task Build(string repoUrl, string branch, string? token = null);
}