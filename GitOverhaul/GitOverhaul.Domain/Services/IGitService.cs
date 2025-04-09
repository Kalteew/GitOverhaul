namespace GitOverhaul.Domain.Services;

public interface IGitService
{
    Task<object> GetRepositoryStructureAsync(string repoUrl, string branch);
    Task<string?> ReadFileAsync(string repoUrl, string branch, string path);
    Task<bool> PushChangesAsync(string repoUrl, string branch, string filePath, string newContent, string commitMessage);
}