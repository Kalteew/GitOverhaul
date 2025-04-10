using System.Diagnostics;

namespace GitOverhaul.Infra.Services;

public class TempGitRepo(string repoUrl, string branch) : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public async Task CloneAsync()
    {
        await RunGit($"clone --branch {branch} --single-branch {repoUrl} \"{Path}\"");
    }

    public async Task<bool> CommitAndPushAsync(string commitMessage, string authorName, string authorEmail)
    {
        var cmds = new[]
        {
            $"config user.email \"{authorEmail}\"",
            $"config user.name \"{authorName}\"",
            "add .",
            $"commit -m \"{commitMessage}\"",
            "push",
        };

        foreach (var cmd in cmds) {
            var success = await RunGit(cmd, Path);

            if (!success) {
                return false;
            }
        }

        return true;
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

        using var proc = Process.Start(psi)!;
        await proc.WaitForExitAsync();
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