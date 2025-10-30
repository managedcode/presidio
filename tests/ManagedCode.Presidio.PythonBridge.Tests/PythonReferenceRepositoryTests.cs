using Xunit;

namespace ManagedCode.Presidio.PythonBridge.Tests;

public sealed class PythonReferenceRepositoryTests
{
    [Fact]
    public void LocateRootFindsSubmoduleDirectory()
    {
        var repository = PythonReferenceRepository.LocateRoot();
        Assert.True(repository.Exists);
        Assert.Equal("microsoft-presidio", repository.Name);

        var analyzerFolder = Path.Combine(repository.FullName, "presidio-analyzer");
        Assert.True(Directory.Exists(analyzerFolder));
        Assert.True(File.Exists(Path.Combine(repository.FullName, "README.MD")));
    }
}
