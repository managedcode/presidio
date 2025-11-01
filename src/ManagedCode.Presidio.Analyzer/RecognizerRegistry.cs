namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Holds the collection of recognizers available to the analyzer engine.
/// </summary>
public sealed class RecognizerRegistry
{
    private readonly List<EntityRecognizer> _recognizers;
    private readonly HashSet<string> _supportedLanguages;

    public RecognizerRegistry(
        IEnumerable<EntityRecognizer>? recognizers = null,
        IEnumerable<string>? supportedLanguages = null)
    {
        _recognizers = recognizers?.ToList() ?? new List<EntityRecognizer>();
        _supportedLanguages = supportedLanguages is null
            ? new HashSet<string>(_recognizers.Select(r => r.SupportedLanguage), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(supportedLanguages, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<EntityRecognizer> RegisteredRecognizers => _recognizers;

    public IReadOnlyCollection<string> SupportedLanguages => _supportedLanguages;

    public void AddRecognizer(EntityRecognizer recognizer)
    {
        ArgumentNullException.ThrowIfNull(recognizer);
        _recognizers.Add(recognizer);
        _supportedLanguages.Add(recognizer.SupportedLanguage);
    }

    public void RemoveRecognizer(string recognizerName, string? language = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recognizerName);

        _recognizers.RemoveAll(recognizer =>
            string.Equals(recognizer.Name, recognizerName, StringComparison.Ordinal)
            && (language is null || string.Equals(recognizer.SupportedLanguage, language, StringComparison.Ordinal)));
    }

    public IReadOnlyCollection<EntityRecognizer> GetRecognizers(
        string language,
        IReadOnlyCollection<string>? entities = null,
        bool allFields = false,
        IReadOnlyCollection<EntityRecognizer>? adHocRecognizers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        if (!allFields && (entities is null || entities.Count == 0))
        {
            throw new ArgumentException("No entities provided.", nameof(entities));
        }

        var candidates = new List<EntityRecognizer>(_recognizers);
        if (adHocRecognizers is not null)
        {
            candidates.AddRange(adHocRecognizers);
        }

        var filtered = allFields
            ? candidates.Where(recognizer =>
                string.Equals(recognizer.SupportedLanguage, language, StringComparison.Ordinal))
            : candidates.Where(recognizer =>
                string.Equals(recognizer.SupportedLanguage, language, StringComparison.Ordinal)
                && recognizer.SupportedEntities.Intersect(entities ?? Array.Empty<string>(), StringComparer.Ordinal).Any());

        var result = filtered
            .Distinct()
            .ToList();

        if (result.Count == 0)
        {
            throw new InvalidOperationException("No matching recognizers were found to serve the request.");
        }

        return result;
    }

    public IReadOnlyCollection<string> GetSupportedEntities(IReadOnlyCollection<string>? languages = null)
    {
        var targetLanguages = (languages is null || languages.Count == 0) ? _supportedLanguages : languages;

        return _recognizers
            .Where(recognizer => targetLanguages.Contains(recognizer.SupportedLanguage))
            .SelectMany(recognizer => recognizer.SupportedEntities)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public void LoadPredefinedRecognizers(INlpEngine nlpEngine, IReadOnlyCollection<string> languages)
    {
        // Placeholder for the YAML-backed loader present in the Python implementation.
        // Predefined recognizers will be populated as part of the ongoing migration.
        ArgumentNullException.ThrowIfNull(languages);

        _supportedLanguages.UnionWith(languages);

        if (nlpEngine is not null)
        {
            AddNlpRecognizer(nlpEngine);
        }

        foreach (var language in languages)
        {
            if (string.Equals(language, "en", StringComparison.OrdinalIgnoreCase))
            {
                if (!_recognizers.Any(r => r is CreditCardRecognizer existing && string.Equals(existing.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new CreditCardRecognizer());
                }

                if (!_recognizers.Any(r => r is IbanRecognizer existingIban && string.Equals(existingIban.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new IbanRecognizer());
                }

                if (!_recognizers.Any(r => r is AbaRoutingRecognizer existingAba && string.Equals(existingAba.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new AbaRoutingRecognizer());
                }

                if (!_recognizers.Any(r => r is CryptoRecognizer existingCrypto && string.Equals(existingCrypto.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new CryptoRecognizer());
                }

                if (!_recognizers.Any(r => r is EmailRecognizer existingEmail && string.Equals(existingEmail.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new EmailRecognizer());
                }

                if (!_recognizers.Any(r => r is DateRecognizer existingDate && string.Equals(existingDate.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new DateRecognizer());
                }

                if (!_recognizers.Any(r => r is IpRecognizer existingIp && string.Equals(existingIp.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new IpRecognizer());
                }

                if (!_recognizers.Any(r => r is UrlRecognizer existingUrl && string.Equals(existingUrl.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new UrlRecognizer());
                }

                if (!_recognizers.Any(r => r is PhoneRecognizer existingPhone && string.Equals(existingPhone.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new PhoneRecognizer());
                }

                if (!_recognizers.Any(r => r is UsSsnRecognizer existingUsSsn && string.Equals(existingUsSsn.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new UsSsnRecognizer());
                }
            }
            else if (string.Equals(language, "fi", StringComparison.OrdinalIgnoreCase))
            {
                if (!_recognizers.Any(r => r is FiPersonalIdentityCodeRecognizer existingFi && string.Equals(existingFi.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new FiPersonalIdentityCodeRecognizer());
                }
            }
            else if (string.Equals(language, "pl", StringComparison.OrdinalIgnoreCase))
            {
                if (!_recognizers.Any(r => r is PlPeselRecognizer existingPl && string.Equals(existingPl.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
                {
                    AddRecognizer(new PlPeselRecognizer());
                }
            }
        }
    }

    public void AddNlpRecognizer(INlpEngine nlpEngine)
    {
        ArgumentNullException.ThrowIfNull(nlpEngine);
        var supportedEntities = nlpEngine.GetSupportedEntities();
        foreach (var language in nlpEngine.GetSupportedLanguages())
        {
            _supportedLanguages.Add(language);
            if (_recognizers.Any(r => r is OnnxNerRecognizer existing && string.Equals(existing.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var recognizer = new OnnxNerRecognizer(language, supportedEntities);
            _recognizers.Add(recognizer);
        }
    }
}
