using System.Numerics;
using System.Security.Cryptography;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Recognizes Bitcoin-style cryptocurrency wallet addresses (Base58 and Bech32/Bech32m).
/// </summary>
public sealed class CryptoRecognizer(
    IEnumerable<Pattern>? patterns = null,
    IEnumerable<string>? context = null,
    string supportedLanguage = "en",
    string supportedEntity = "CRYPTO") : PatternRecognizer(
        supportedEntity,
        patterns ?? DefaultPatterns,
        context: context ?? DefaultContext,
        supportedLanguage: supportedLanguage)
{
    private const string Bech32Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
    private const uint Bech32ChecksumConstant = 1;
    private const uint Bech32mChecksumConstant = 0x2BC830A3;
    private const int Bech32Encoding = 1;
    private const int Bech32mEncoding = 2;

    private static readonly Pattern[] DefaultPatterns =
    {
        new("Crypto (Medium)", "(bc1|[13])[a-zA-HJ-NP-Z0-9]{25,59}", 0.5),
    };

    private static readonly string[] DefaultContext =
    {
        "wallet",
        "btc",
        "bitcoin",
        "crypto",
    };

    private static readonly (string Search, string Replacement)[] Base58ReplacementPairs =
    {
        (" ", string.Empty),
    };

    private static readonly int[] Bech32CharMap = BuildBech32CharMap();
    private static readonly uint[] Bech32Generator = { 0x3B6A57B2u, 0x26508E6Du, 0x1EA119FAu, 0x3D4233DDu, 0x2A1462B3u };

    protected override bool? ValidateResult(string patternText)
    {
        if (string.IsNullOrWhiteSpace(patternText))
        {
            return false;
        }

        if (patternText[0] == '1' || patternText[0] == '3')
        {
            return ValidateBase58(patternText);
        }

        if (patternText.StartsWith("bc1", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateBech32(patternText);
        }

        return false;
    }

    private static bool ValidateBase58(string address)
    {
        try
        {
            var sanitized = EntityRecognizer.SanitizeValue(address, Base58ReplacementPairs);
            var decoded = DecodeBase58(sanitized);
            if (decoded.Length < 4)
            {
                return false;
            }

            var payload = decoded[..^4];
            var checksum = decoded[^4..];
            var hash = DoubleSha256(payload);
            return checksum.SequenceEqual(hash[..4]);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] DoubleSha256(ReadOnlySpan<byte> data)
    {
        var first = SHA256.HashData(data);
        return SHA256.HashData(first);
    }

    private static byte[] DecodeBase58(string input)
    {
        const string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        if (input.Any(ch => !alphabet.Contains(ch, StringComparison.Ordinal)))
        {
            throw new ArgumentException("Invalid Base58 character.", nameof(input));
        }

        var trimmed = input.TrimStart('1');
        var value = BigInteger.Zero;
        foreach (var ch in trimmed)
        {
            var index = alphabet.IndexOf(ch, StringComparison.Ordinal);
            value = value * 58 + index;
        }

        var valueBytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        var leadingZeros = input.Length - trimmed.Length;
        var result = new byte[leadingZeros + valueBytes.Length];
        valueBytes.CopyTo(result.AsSpan(leadingZeros));
        return result;
    }

    private static bool ValidateBech32(string address)
    {
        var (hrp, data, spec) = Bech32Decode(address);
        return hrp is not null && data is not null && spec.HasValue;
    }

    private static (string? Hrp, int[]? Data, int? Spec) Bech32Decode(string bech)
    {
        if (string.IsNullOrEmpty(bech) || bech.Any(ch => ch < 33 || ch > 126))
        {
            return (null, null, null);
        }

        if (!bech.Equals(bech.ToLowerInvariant(), StringComparison.Ordinal) &&
            !bech.Equals(bech.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return (null, null, null);
        }

        var normalized = bech.ToLowerInvariant();
        var pos = normalized.LastIndexOf('1');
        if (pos < 1 || pos + 7 > normalized.Length || normalized.Length > 90)
        {
            return (null, null, null);
        }

        var hrp = normalized[..pos];
        var dataLength = normalized.Length - pos - 1;
        var data = new int[dataLength];
        for (var i = 0; i < dataLength; i++)
        {
            var ch = normalized[pos + 1 + i];
            var index = ch < Bech32CharMap.Length ? Bech32CharMap[ch] : -1;
            if (index < 0)
            {
                return (null, null, null);
            }

            data[i] = index;
        }

        if (data.Length < 6)
        {
            return (null, null, null);
        }

        var spec = Bech32VerifyChecksum(hrp, data);
        if (spec is null)
        {
            return (null, null, null);
        }

        var payload = new int[data.Length - 6];
        Array.Copy(data, payload, payload.Length);
        return (hrp, payload, spec);
    }

    private static int? Bech32VerifyChecksum(string hrp, int[] data)
    {
        var hrpExpanded = Bech32HrpExpand(hrp);
        var combined = new int[hrpExpanded.Length + data.Length];
        Array.Copy(hrpExpanded, combined, hrpExpanded.Length);
        Array.Copy(data, 0, combined, hrpExpanded.Length, data.Length);

        var polymod = Bech32Polymod(combined);
        if (polymod == Bech32ChecksumConstant)
        {
            return Bech32Encoding;
        }

        if (polymod == Bech32mChecksumConstant)
        {
            return Bech32mEncoding;
        }

        return null;
    }

    private static int[] Bech32HrpExpand(string hrp)
    {
        var result = new int[hrp.Length * 2 + 1];
        for (var i = 0; i < hrp.Length; i++)
        {
            result[i] = hrp[i] >> 5;
        }

        result[hrp.Length] = 0;

        for (var i = 0; i < hrp.Length; i++)
        {
            result[hrp.Length + 1 + i] = hrp[i] & 31;
        }

        return result;
    }

    private static uint Bech32Polymod(int[] values)
    {
        var chk = 1u;
        foreach (var value in values)
        {
            var top = chk >> 25;
            chk = ((chk & 0x1FFFFFFu) << 5) ^ (uint)value;
            for (var i = 0; i < 5; i++)
            {
                if (((top >> i) & 1) != 0)
                {
                    chk ^= Bech32Generator[i];
                }
            }
        }

        return chk;
    }

    private static int[] BuildBech32CharMap()
    {
        var map = Enumerable.Repeat(-1, 128).ToArray();
        for (var i = 0; i < Bech32Charset.Length; i++)
        {
            map[Bech32Charset[i]] = i;
        }

        return map;
    }
}
