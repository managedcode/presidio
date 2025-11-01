using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class InAadhaarRecognizerTests
{
    [Theory]
    [InlineData("123456789012", false)]
    [InlineData("312345678909", true)]
    [InlineData("399876543211", true)]
    [InlineData("1234 5678 9012", false)]
    [InlineData("3123 4567 8909", true)]
    [InlineData("3998 7654 3211", true)]
    [InlineData("1234-5678-9012", false)]
    [InlineData("3123-4567-8909", true)]
    [InlineData("3998-7654-3211", true)]
    [InlineData("1234:5678:9012", false)]
    [InlineData("3123:4567:8909", true)]
    [InlineData("3998:7654:3211", true)]
    [InlineData("My Aadhaar number is 400123456787 with a lot of text beyond it", true)]
    public void RecognizerValidatesAadhaarNumbers(string text, bool expected)
    {
        var recognizer = new InAadhaarRecognizer();
        var results = recognizer.Analyze(text, new[] { "IN_AADHAAR" }, new NlpArtifacts("en")).ToList();

        if (expected)
        {
            results.ShouldHaveSingleItem();
        }
        else
        {
            results.ShouldBeEmpty();
        }
    }

    [Theory]
    [InlineData("abMA", false, false)]
    [InlineData("abCba", false, true)]
    [InlineData("ABBA", false, true)]
    [InlineData("aBba", true, true)]
    [InlineData("NotAPalindrome", true, false)]
    public void PalindromeHelperBehavesAsPython(string input, bool caseInsensitive, bool expected) =>
        InAadhaarRecognizer.IsPalindrome(input, caseInsensitive).ShouldBe(expected);

    [Theory]
    [InlineData(312345678909L, true)]
    [InlineData(400123456787L, true)]
    [InlineData(123456789012L, false)]
    public void VerhoeffHelperMatchesPython(long value, bool expected) =>
        InAadhaarRecognizer.IsVerhoeffNumber(value).ShouldBe(expected);
}
