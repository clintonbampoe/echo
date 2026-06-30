using System.Reflection;
using DotNetEnv;

namespace Echo.Shared.Extensions;

public static class EnvLoader
{
    public static void LoadFromRepoRoot()
    {
        // Start from the directory where the assembly is running (e.g., bin/Debug/net8.0)
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string? gitRoot = FindGitRoot(currentDir);

        if (gitRoot != null)
        {
            string envPath = Path.Combine(gitRoot, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                Console.WriteLine($".env loaded from repo root: {gitRoot}");
            }
            else
            {
                throw new FileNotFoundException(".env file not found in repo root.", envPath);
            }
        }
        else
        {
            throw new DirectoryNotFoundException("Git repository root not found. Is this folder initialized with git?");
        }
    }

    private static string? FindGitRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory != null)
        {
            // Check if .git exists in this directory
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        return null;
    }
}
