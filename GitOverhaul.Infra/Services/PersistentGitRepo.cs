using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace GitOverhaul.Infra.Services;

public class PersistentGitRepo
{
    public string Path { get; }
    private readonly string _repoUrl;
    private readonly string _branch;

    public PersistentGitRepo(string repoUrl, string branch)
    {
        _repoUrl = repoUrl;
        _branch = branch;

        var hash = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes($"{repoUrl}_{branch}")));
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "reposCache", hash);
    }

    public async Task EnsureUpdatedAsync()
    {
        if (!Directory.Exists(Path))
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            await RunGit($"clone --branch {_branch} --single-branch {_repoUrl} \"{Path}\"");
        }
        else
        {
            await RunGit("fetch", Path);
            await RunGit($"checkout {_branch}", Path);
            await RunGit("pull", Path);
        }
    }

    public async Task CommitAndPushAsync(string commitMessage, string authorName, string authorEmail)
    {
        var cmds = new[]
        {
            $"config user.email \"{authorEmail}\"",
            $"config user.name \"{authorName}\"",
            "add -A",
            $"commit -am \"{commitMessage}\"",
            "push -u origin",
        };

        foreach (var cmd in cmds)
            await RunGit(cmd, Path);
    }

    public async Task CreateAndPushBranchAsync(string newBranch)
    {
        await RunGit($"checkout -b {newBranch}", Path);
        await RunGit($"push -u origin {newBranch}", Path);
    }

    private async Task<bool> RunGit(string args, string? workingDir = null)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory(),
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        Console.WriteLine($"[Running] git {args}");
        using var proc = Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        var error = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0) {
            await Console.Error.WriteLineAsync($"[Git ERROR] git {args}\n{error}");
            throw new InvalidOperationException($"Git command failed: git {args} {error}");
        }

        Console.WriteLine($"[Git OUTPUT] git {args}\n{output}");
        return proc.ExitCode == 0;
    }
}