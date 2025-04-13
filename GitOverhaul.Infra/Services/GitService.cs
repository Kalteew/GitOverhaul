using System.Diagnostics;
using GitOverhaul.Domain.Services;

namespace GitOverhaul.Infra.Services;

public class GitService : IGitService
{
    public async Task<object> GetRepositoryStructureAsync(string repoUrl, string branch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        var repo = new PersistentGitRepo(repoUrl, branch);
        await repo.EnsureUpdatedAsync();
        return BuildStructure(repo.Path);
    }

    public async Task<string> ReadFileAsync(string repoUrl, string branch, string filePath, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        var repo = new PersistentGitRepo(repoUrl, branch);
        await repo.EnsureUpdatedAsync();
        return await File.ReadAllTextAsync(Path.Combine(repo.Path, filePath));
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
        var repo = new PersistentGitRepo(repoUrl, branch);
        await repo.EnsureUpdatedAsync();

        foreach (var (filePath, content, isDeletion) in changes)
        {
            var fullPath = Path.Combine(repo.Path, filePath);

            if (isDeletion)
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                await File.WriteAllTextAsync(fullPath, content!);
            }
        }

        await repo.CommitAndPushAsync(commitMessage, authorName, authorEmail);
    }

    public async Task Build(string repoUrl, string branch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        var repo = new PersistentGitRepo(repoUrl, branch);
        await repo.EnsureUpdatedAsync();

        var buildResult = RunDotnetBuild(repo.Path);

        if (buildResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"Build failed:\n{buildResult.Output}");
        }
    }

    public async Task CreateBranchAsync(string repoUrl, string sourceBranch, string newBranch, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        var repo = new PersistentGitRepo(repoUrl, sourceBranch);
        await repo.EnsureUpdatedAsync();

        await repo.CreateAndPushBranchAsync(newBranch);
    }

    public async Task<List<(string Command, string Output)>> RunGitCommandsAsync(string repoUrl, string branch, List<string> commands, string? token = null)
    {
        if (!string.IsNullOrWhiteSpace(token)) repoUrl = InjectTokenIntoUrl(repoUrl, token);
        var repo = new PersistentGitRepo(repoUrl, branch);
        await repo.EnsureUpdatedAsync();

        var results = new List<(string Command, string Output)>();

        foreach (var cmd in commands)
        {
            try
            {
                var output = await RunGit(cmd, repo.Path);
                results.Add((cmd, output));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Commande échouée: '{cmd}'\n{ex.Message}");
            }
        }

        return results;
    }

    private async Task<string> RunGit(string args, string workingDir)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDir,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        using var proc = Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        var error = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0)
            throw new Exception(error);

        return output;
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