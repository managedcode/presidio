using Xunit;

namespace ManagedCode.Presidio.Anonymizer.Tests;

public sealed class EncryptDecryptOperatorTests
{
    private const string Key = "12345678901234567890123456789012"; // 256-bit key

    [Fact]
    public void EncryptAndDecryptAreInverseOperations()
    {
        var encrypt = new EncryptOperator();
        var decrypt = new DecryptOperator();

        var parameters = new Dictionary<string, object?>
        {
            [EncryptOperator.KeyParameter] = Key,
        };

        encrypt.Validate(parameters);
        var cipher = encrypt.Operate("sensitive", parameters);

        decrypt.Validate(parameters);
        var plain = decrypt.Operate(cipher, parameters);

        Assert.NotEqual("sensitive", cipher);
        Assert.Equal("sensitive", plain);
    }

    [Fact]
    public void EncryptRejectsInvalidKeySize()
    {
        var encrypt = new EncryptOperator();
        var parameters = new Dictionary<string, object?>
        {
            [EncryptOperator.KeyParameter] = "short",
        };

        var exception = Assert.Throws<InvalidParamException>(() => encrypt.Validate(parameters));
        Assert.Equal("Invalid input, key must be of length 128, 192 or 256 bits", exception.Message);
    }
}
