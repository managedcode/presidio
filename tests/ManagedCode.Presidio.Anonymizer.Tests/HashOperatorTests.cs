using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class HashOperatorTests
{
    [Fact]
    public void HashOperatorDefaultsToSha256()
    {
        var op = new HashOperator();
        var output = op.Operate("hello", new Dictionary<string, object?>());

        Assert.Equal("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", output);
    }

    [Fact]
    public void HashOperatorSupportsSha512()
    {
        var op = new HashOperator();
        var parameters = new Dictionary<string, object?>
        {
            [HashOperator.HashTypeKey] = HashOperator.Sha512,
        };

        var output = op.Operate("hello", parameters);

        Assert.Equal(
            "9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadae2dff72519673ca72323c3d99ba5c11d7c7acc6e14b8c5da0c4663475c2e5c3adef46f73bcdec043",
            output);
    }

    [Fact]
    public void HashOperatorRejectsUnsupportedAlgorithm()
    {
        var op = new HashOperator();
        var parameters = new Dictionary<string, object?>
        {
            [HashOperator.HashTypeKey] = "md5",
        };

        var exception = Assert.Throws<InvalidParamException>(() => op.Validate(parameters));
        Assert.Equal("Parameter hash_type value md5 is not in range of values ['sha256', 'sha512']", exception.Message);
    }
}
