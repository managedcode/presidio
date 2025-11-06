using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

public sealed class EncryptOperator : Operator
{
    public const string KeyParameter = "key";

    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var key = ExtractKey(parameters);
        return AesCipher.Encrypt(key, text ?? string.Empty);
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var key = ExtractKey(parameters);
        if (!AesCipher.IsValidKeySize(key))
        {
            throw new InvalidParamException("Invalid input, key must be of length 128, 192 or 256 bits");
        }
    }

    public override string OperatorName => "encrypt";

    public override OperatorType Type => OperatorType.Anonymize;

    private static byte[] ExtractKey(IDictionary<string, object?>? parameters)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        if (!effectiveParams.TryGetValue(KeyParameter, out var keyValue))
        {
            throw new InvalidParamException($"Expected parameter {KeyParameter}");
        }

        return keyValue switch
        {
            string keyText => Encoding.UTF8.GetBytes(keyText),
            byte[] keyBytes => keyBytes,
            _ => throw new InvalidParamException($"Expected parameter {KeyParameter}"),
        };
    }
}
