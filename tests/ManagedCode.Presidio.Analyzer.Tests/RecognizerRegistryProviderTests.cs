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

        recognizers.ShouldContain(recognizer => recognizer is CreditCardRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is IbanRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is AbaRoutingRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is CryptoRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is DateRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is EmailRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is IpRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is PhoneRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UrlRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UsSsnRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UsPassportRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UsItinRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UsBankRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UsLicenseRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is MedicalLicenseRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is NhsRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is UkNinoRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is AuAbnRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is AuAcnRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is AuTfnRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is AuMedicareRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InAadhaarRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InPanRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InPassportRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InVehicleRegistrationRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InGstinRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is InVoterRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is ItDriverLicenseRecognizer { SupportedLanguage: "it" });
        recognizers.ShouldContain(recognizer => recognizer is ItFiscalCodeRecognizer { SupportedLanguage: "it" });
        recognizers.ShouldContain(recognizer => recognizer is ItIdentityCardRecognizer { SupportedLanguage: "it" });
        recognizers.ShouldContain(recognizer => recognizer is ItPassportRecognizer { SupportedLanguage: "it" });
        recognizers.ShouldContain(recognizer => recognizer is ItVatCodeRecognizer { SupportedLanguage: "it" });
        recognizers.ShouldContain(recognizer => recognizer is SgFinRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is SgUenRecognizer { SupportedLanguage: "en" });
        recognizers.ShouldContain(recognizer => recognizer is EsNieRecognizer { SupportedLanguage: "es" });
        recognizers.ShouldContain(recognizer => recognizer is EsNifRecognizer { SupportedLanguage: "es" });
        recognizers.ShouldContain(recognizer => recognizer is ThTninRecognizer { SupportedLanguage: "th" });
        recognizers.ShouldContain(recognizer => recognizer is KrRrnRecognizer { SupportedLanguage: "ko" });

        var creditCard = recognizers.Single(r => r is CreditCardRecognizer);
        creditCard.Context.ShouldContain("credit");
        creditCard.Context.ShouldContain("visa");

        var passportRecognizer = recognizers.Single(r => r is UsPassportRecognizer);
        passportRecognizer.Context.ShouldContain("passport");
    }
}
