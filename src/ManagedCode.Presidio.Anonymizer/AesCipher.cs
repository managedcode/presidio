using System.Security.Cryptography;
using System.Text;

namespace ManagedCode.Presidio.Anonymizer;

internal static class AesCipher
{
    public static string Encrypt(byte[] key, string text)
    {
        using var aes = CreateAes(key);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, payload, aes.IV.Length, encrypted.Length);

        return ToBase64Url(payload);
    }

    public static string Decrypt(byte[] key, string text)
    {
        using var aes = CreateAes(key);
        var payload = FromBase64Url(text ?? string.Empty);
        if (payload.Length < aes.BlockSize / 8)
        {
            throw new InvalidParamException("Ciphertext payload is too small");
        }

        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
        aes.IV = iv;

        var cipherText = new byte[payload.Length - ivLength];
        Buffer.BlockCopy(payload, ivLength, cipherText, 0, cipherText.Length);

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    public static bool IsValidKeySize(byte[] key)
    {
        if (key is null)
        {
            return false;
        }

        var bitLength = key.Length * 8;
        return bitLength == 128 || bitLength == 192 || bitLength == 256;
    }

    private static Aes CreateAes(byte[] key)
    {
        if (!IsValidKeySize(key))
        {
            throw new InvalidParamException("Invalid input, key must be of length 128, 192 or 256 bits");
        }

        var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private static string ToBase64Url(byte[] payload)
    {
        var base64 = Convert.ToBase64String(payload);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static byte[] FromBase64Url(string text)
    {
        var normalized = text.Replace('-', '+').Replace('_', '/');
        var padding = 4 - normalized.Length % 4;
        if (padding is > 0 and < 4)
        {
            normalized = normalized.PadRight(normalized.Length + padding, '=');
        }

        return Convert.FromBase64String(normalized);
    }
}
