using GitOverhaul.Domain.Services;

namespace GitOverhaul.Infra.Services;

public class GitService : IGitService
{
    public async Task<object> GetRepositoryStructureAsync(string repoUrl, string branch)
    {
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();
        return BuildStructure(temp.Path);
    }

    public async Task<string?> ReadFileAsync(string repoUrl, string branch, string path)
    {
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();

        var fullPath = Path.Combine(temp.Path, path);
        return File.Exists(fullPath) ? await File.ReadAllTextAsync(fullPath) : null;
    }

    public async Task<bool> PushChangesAsync(string repoUrl, string branch, string filePath, string newContent, string commitMessage)
    {
        using var temp = new TempGitRepo(repoUrl, branch);
        await temp.CloneAsync();

        var fullPath = Path.Combine(temp.Path, filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, newContent);

        return await temp.CommitAndPushAsync(commitMessage);
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
}
