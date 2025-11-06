using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

public sealed class DecryptOperator : Operator
{
    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var key = ExtractKey(parameters);
        return AesCipher.Decrypt(key, text ?? string.Empty);
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var key = ExtractKey(parameters);
        if (!AesCipher.IsValidKeySize(key))
        {
            throw new InvalidParamException("Invalid input, key must be of length 128, 192 or 256 bits");
        }
    }

    public override string OperatorName => "decrypt";

    public override OperatorType Type => OperatorType.Deanonymize;

    private static byte[] ExtractKey(IDictionary<string, object?>? parameters)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        if (!effectiveParams.TryGetValue(EncryptOperator.KeyParameter, out var keyValue))
        {
            throw new InvalidParamException($"Expected parameter {EncryptOperator.KeyParameter}");
        }

        return keyValue switch
        {
            string keyText => Encoding.UTF8.GetBytes(keyText),
            byte[] keyBytes => keyBytes,
            _ => throw new InvalidParamException($"Expected parameter {EncryptOperator.KeyParameter}"),
        };
    }
}
