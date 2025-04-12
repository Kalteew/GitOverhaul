using System.Diagnostics;

namespace GitOverhaul.Infra.Services;

public class TempGitRepo(string repoUrl, string branch) : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public async Task CloneAsync()
    {
        await RunGit($"clone --branch {branch} --single-branch {repoUrl} \"{Path}\"");
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

        Console.WriteLine($"debug : {repoUrl} - {branch}");

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
            throw new InvalidOperationException($"Échec de la commande Git : git {args} {error}");
        }

        Console.WriteLine($"[Git OUTPUT] git {args}\n{output}");
        return proc.ExitCode == 0;
    }

    public void Dispose()
    {
        try {
            if (Directory.Exists(Path)) {
                Directory.Delete(Path, true);
            }
        }
        catch {
            /* rien */
        }
    }
}