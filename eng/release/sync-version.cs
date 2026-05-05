// Single-file C# program to sync the repository-wide release version.
//
// Usage:
//   dotnet run eng/release/sync-version.cs -- <version>
//
// Updates:
//   - <Version>...</Version> in eng/skill-validator/src/SkillValidator.csproj
//   - "version": "..." in every plugins/*/plugin.json
//
// Uses targeted line-level regex replacements to preserve existing file
// formatting (indentation, line endings, trailing newlines).
//
// Owned by the .github/workflows/release.yml workflow. Do not run by hand
// for the purpose of bumping versions on main; that's the workflow's job.

using System.Text.RegularExpressions;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run eng/release/sync-version.cs -- <version>");
    Console.Error.WriteLine("Example: dotnet run eng/release/sync-version.cs -- 1.0.1");
    return 2;
}

string version = args[0].TrimStart('v');

if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+(-[A-Za-z0-9.\-]+)?$"))
{
    Console.Error.WriteLine($"Error: '{version}' is not a valid semver version (expected MAJOR.MINOR.PATCH[-prerelease]).");
    return 2;
}

string repoRoot = FindRepoRoot();
int changes = 0;

string csproj = Path.Combine(repoRoot, "eng", "skill-validator", "src", "SkillValidator.csproj");
if (UpdateCsprojVersion(csproj, version))
{
    Console.WriteLine($"Updated {Path.GetRelativePath(repoRoot, csproj)}");
    changes++;
}

string pluginsDir = Path.Combine(repoRoot, "plugins");
foreach (string pluginJson in Directory.EnumerateFiles(pluginsDir, "plugin.json", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
{
    if (UpdatePluginJsonVersion(pluginJson, version))
    {
        Console.WriteLine($"Updated {Path.GetRelativePath(repoRoot, pluginJson)}");
        changes++;
    }
}

Console.WriteLine($"Done. {changes} file(s) changed; target version {version}.");
return 0;

static string FindRepoRoot()
{
    string? dir = Directory.GetCurrentDirectory();
    while (!string.IsNullOrEmpty(dir))
    {
        if (Directory.Exists(Path.Combine(dir, ".git")) &&
            Directory.Exists(Path.Combine(dir, "plugins")) &&
            Directory.Exists(Path.Combine(dir, "eng", "skill-validator")))
        {
            return dir;
        }
        dir = Path.GetDirectoryName(dir);
    }
    throw new InvalidOperationException("Unable to locate repository root from current directory. Run this tool from inside the repository.");
}

static bool UpdateCsprojVersion(string path, string version)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"csproj not found: {path}");
    }

    string original = File.ReadAllText(path);
    var regex = new Regex(@"<Version>[^<]*</Version>");
    if (!regex.IsMatch(original))
    {
        throw new InvalidOperationException($"No <Version> element found in {path}.");
    }

    string updated = regex.Replace(original, $"<Version>{version}</Version>", count: 1);
    if (updated == original)
    {
        return false;
    }

    File.WriteAllText(path, updated);
    return true;
}

static bool UpdatePluginJsonVersion(string path, string version)
{
    string original = File.ReadAllText(path);
    var regex = new Regex(@"""version""\s*:\s*""[^""]*""");
    if (!regex.IsMatch(original))
    {
        throw new InvalidOperationException($"No \"version\" property found in {path}.");
    }

    string updated = regex.Replace(original, $"\"version\": \"{version}\"", count: 1);
    if (updated == original)
    {
        return false;
    }

    File.WriteAllText(path, updated);
    return true;
}
