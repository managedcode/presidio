using Shouldly;
using Xunit;

namespace ManagedCode.Presidio.Analyzer.Tests;

public sealed class RecognizerRegistryProviderTests
{
    [Fact]
    public void CreateRecognizerRegistry_LoadsPredefinedRecognizersFromYaml()
    {
        var provider = new RecognizerRegistryProvider();
        var registry = provider.CreateRecognizerRegistry(nlpEngine: null, supportedLanguages: new[] { "en" });

        var recognizers = registry.GetRecognizers("en", allFields: true);

        recognizers.Any(recognizer => recognizer is CreditCardRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is IbanRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is AbaRoutingRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is CryptoRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is DateRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is EmailRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is IpRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is PhoneRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UrlRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UsSsnRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UsPassportRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UsItinRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UsBankRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UsLicenseRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is MedicalLicenseRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is NhsRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is UkNinoRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is AuAbnRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is AuAcnRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is AuTfnRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is AuMedicareRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InAadhaarRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InPanRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InPassportRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InVehicleRegistrationRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InGstinRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is InVoterRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ItDriverLicenseRecognizer { SupportedLanguage: "it" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ItFiscalCodeRecognizer { SupportedLanguage: "it" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ItIdentityCardRecognizer { SupportedLanguage: "it" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ItPassportRecognizer { SupportedLanguage: "it" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ItVatCodeRecognizer { SupportedLanguage: "it" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is SgFinRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is SgUenRecognizer { SupportedLanguage: "en" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is EsNieRecognizer { SupportedLanguage: "es" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is EsNifRecognizer { SupportedLanguage: "es" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is ThTninRecognizer { SupportedLanguage: "th" }).ShouldBeTrue();
        recognizers.Any(recognizer => recognizer is KrRrnRecognizer { SupportedLanguage: "ko" }).ShouldBeTrue();

        var creditCard = recognizers.Single(r => r is CreditCardRecognizer { SupportedLanguage: "en" });
        creditCard.Context.ShouldContain("credit");
        creditCard.Context.ShouldContain("visa");

        var passportRecognizer = recognizers.Single(r => r is UsPassportRecognizer { SupportedLanguage: "en" });
        passportRecognizer.Context.ShouldContain("passport");
    }
}
