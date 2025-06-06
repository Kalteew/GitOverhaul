using System.Diagnostics;
using GitOverhaul.Domain.Services;

namespace GitOverhaul.Infra.Services;

public class GitService : IGitService
{
    public async Task<object> GetRepositoryStructureAsync(string repoUrl, string branch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();
        return BuildStructure(temp.Path);
    }

    public async Task<string> ReadFileAsync(string repoUrl, string branch, string filePath, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();
        return await File.ReadAllTextAsync(Path.Combine(temp.Path, filePath));
    }

    public async Task PushMultipleChangesAsync(
        string repoUrl,
        string branch,
        List<(string FilePath, string? Content, bool IsDeletion)> changes,
        string authorName,
        string authorEmail,
        string commitMessage,
        string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();

        foreach (var (filePath, content, isDeletion) in changes)
        {
            var fullPath = Path.Combine(temp.Path, filePath);

            if (isDeletion) {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            } else {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, content!);
            }
        }

        await temp.CommitAndPushAsync(commitMessage, authorName, authorEmail);
    }

    public async Task Build(string repoUrl, string branch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);

        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();
        var buildResult = RunDotnetBuild(temp.Path);

        if (buildResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"Build failed:\n{buildResult.Output}");
        }
    }

    public async Task CreateBranchAsync(string repoUrl, string sourceBranch, string newBranch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);

        using var temp = new TempGitRepo(repoUrl, sourceBranch);
        await temp.CloneAsync();

        await temp.CreateAndPushBranchAsync(newBranch);
    }

    private string InjectTokenIntoUrl(string url, string token)
    {
        var uri = new Uri(url);
        return $"https://x-access-token:{token}@{uri.Host}{uri.PathAndQuery}";
    }

    private object BuildStructure(string rootPath)
    {
        return WalkDir(new DirectoryInfo(rootPath));
    }

    private List<object> WalkDir(DirectoryInfo dir)
    {
        var list = new List<object>();

        foreach (var subDir in dir.GetDirectories())
        {
            if (subDir.Name == ".git") continue;

            list.Add(new
            {
                subDir.Name,
                IsDirectory = true,
                Children = WalkDir(subDir),
            });
        }

        foreach (var file in dir.GetFiles())
        {
            list.Add(new
            {
                Name = file.Name,
                IsDirectory = false,
                Children = (object?)null
            });
        }

        return list;
    }

    private (int ExitCode, string Output) RunDotnetBuild(string rootPath)
    {
        var sln = Directory.GetFiles(rootPath, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
        var csproj = Directory.GetFiles(rootPath, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
        var target = sln ?? csproj;

        if (target == null)
        {
            return (1, "No .sln or .csproj file found for build.");
        }

        var psi = new ProcessStartInfo("dotnet", $"build \"{target}\"")
        {
            WorkingDirectory = rootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, output);
    }
}