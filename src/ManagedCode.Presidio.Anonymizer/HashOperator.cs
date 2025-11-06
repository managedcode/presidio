using System.Security.Cryptography;
using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

public sealed class HashOperator : Operator
{
    public const string HashTypeKey = "hash_type";
    public const string Sha256 = "sha256";
    public const string Sha512 = "sha512";

    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        var hashType = GetHashTypeOrDefault(effectiveParams);
        var payload = Encoding.UTF8.GetBytes(text ?? string.Empty);

        return hashType switch
        {
            Sha256 => ComputeHash(SHA256.Create(), payload),
            Sha512 => ComputeHash(SHA512.Create(), payload),
            _ => throw new InvalidParamException($"Unsupported hash type '{hashType}'."),
        };
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        var hashType = GetHashTypeOrDefault(effectiveParams);
        Validators.ValidateParameterInRange(new[] { Sha256, Sha512 }, hashType, HashTypeKey, typeof(string));
    }

    public override string OperatorName => "hash";

    public override OperatorType Type => OperatorType.Anonymize;

    private static string GetHashTypeOrDefault(IDictionary<string, object?> parameters)
    {
        return parameters.TryGetValue(HashTypeKey, out var value) && value is string text
            ? text
            : Sha256;
    }

    private static string ComputeHash(HashAlgorithm algorithm, byte[] payload)
    {
        using (algorithm)
        {
            var hash = algorithm.ComputeHash(payload);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
