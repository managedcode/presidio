using System.Reflection;

namespace ManagedCode.Presidio.PythonBridge;

/// <summary>
/// Discovers the checked-out Python reference implementation used during the migration.
/// </summary>
public static class PythonReferenceRepository
{
    private const string SubmoduleFolderName = "external";
    private const string RepositoryFolderName = "microsoft-presidio";

    public static DirectoryInfo LocateRoot(string? startDirectory = null)
    {
        var probe = ResolveStartingDirectory(startDirectory);

        while (probe is not null)
        {
            var candidate = Path.Combine(probe.FullName, SubmoduleFolderName, RepositoryFolderName);
            if (Directory.Exists(candidate))
            {
                return new DirectoryInfo(candidate);
            }

            probe = probe.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Unable to locate the microsoft/presidio submodule under '{SubmoduleFolderName}/{RepositoryFolderName}'.");
    }

    private static DirectoryInfo? ResolveStartingDirectory(string? startDirectory)
    {
        if (!string.IsNullOrWhiteSpace(startDirectory))
        {
            return new DirectoryInfo(startDirectory);
        }

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        return Directory.GetParent(assemblyLocation);
    }
}
