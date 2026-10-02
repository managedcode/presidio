using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Holds the collection of recognizers available to the analyzer engine.
/// </summary>
public sealed class RecognizerRegistry
{
    private readonly List<EntityRecognizer> _recognizers;
    private readonly HashSet<string> _supportedLanguages;
    private static readonly ConcurrentDictionary<string, Type?> RecognizerTypeCache = new(StringComparer.Ordinal);
    private RegexOptions _globalRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;

    public RecognizerRegistry(
        IEnumerable<EntityRecognizer>? recognizers = null,
        IEnumerable<string>? supportedLanguages = null,
        TimeSpan? regexMatchTimeout = null)
    {
        RegexMatchTimeout = ValidateRegexMatchTimeout(regexMatchTimeout ?? TimeSpan.FromSeconds(2));
        _recognizers = recognizers?.ToList() ?? new List<EntityRecognizer>();
        _supportedLanguages = supportedLanguages is null
            ? new HashSet<string>(_recognizers.Select(r => r.SupportedLanguage), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(supportedLanguages, StringComparer.OrdinalIgnoreCase);
        foreach (var recognizer in _recognizers)
        {
            ApplyGlobalRegexOptions(recognizer);
        }
    }

    public IReadOnlyCollection<EntityRecognizer> RegisteredRecognizers => _recognizers;

    public IReadOnlyCollection<string> SupportedLanguages => _supportedLanguages;

    public TimeSpan RegexMatchTimeout { get; private set; } = TimeSpan.FromSeconds(2);

    public void SetRegexMatchTimeout(TimeSpan timeout)
    {
        RegexMatchTimeout = ValidateRegexMatchTimeout(timeout);
        foreach (var recognizer in _recognizers)
        {
            ApplyGlobalRegexOptions(recognizer);
        }
    }

    public void AddRecognizer(EntityRecognizer recognizer)
    {
        ArgumentNullException.ThrowIfNull(recognizer);
        ApplyGlobalRegexOptions(recognizer);
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

        var candidates = _recognizers
            .Concat(adHocRecognizers ?? Array.Empty<EntityRecognizer>())
            .Where(recognizer => string.Equals(recognizer.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        if (!allFields)
        {
            var requestedEntities = entities ?? Array.Empty<string>();
            var availableEntities = candidates.SelectMany(recognizer => recognizer.SupportedEntities)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var unsupportedEntities = requestedEntities
                .Where(entity => !availableEntities.Contains(entity))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (unsupportedEntities.Length > 0)
            {
                throw new InvalidOperationException(
                    $"No recognizers are configured for entity type(s) '{string.Join("', '", unsupportedEntities)}' in language '{language}'.");
            }
        }

        var filtered = allFields
            ? candidates
            : candidates.Where(recognizer =>
                recognizer.SupportedEntities.Intersect(entities ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase).Any());

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
            .Where(recognizer => targetLanguages.Contains(recognizer.SupportedLanguage, StringComparer.OrdinalIgnoreCase))
            .SelectMany(recognizer => recognizer.SupportedEntities)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public void LoadPredefinedRecognizers(
        RecognizerRegistryConfiguration configuration,
        INlpEngine? nlpEngine = null,
        IReadOnlyCollection<string>? languages = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _globalRegexOptions = configuration.GlobalRegexOptions;
        SetRegexMatchTimeout(configuration.RegexMatchTimeout);
        if (configuration.SupportedLanguages.Count > 0)
        {
            _supportedLanguages.UnionWith(configuration.SupportedLanguages);
        }

        foreach (var definition in configuration.Recognizers)
        {
            if (!definition.Enabled)
            {
                continue;
            }

            if (!string.Equals(definition.Type, "predefined", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(
                    $"Recognizer type '{definition.Type}' is not supported for enabled definition '{definition.Name}' in configuration '{configuration.Source}'.");
            }

            var recognizerType = ResolveRecognizerType(definition.Name) ?? throw new InvalidOperationException(
                    $"Configured recognizer '{definition.Name}' was not found in this assembly (configuration '{configuration.Source}').");

            foreach (var languageConfiguration in definition.Languages)
            {
                if (HasRecognizer(recognizerType, languageConfiguration.Language))
                {
                    continue;
                }

                var recognizer = InstantiatePredefinedRecognizer(recognizerType, languageConfiguration) ?? throw new InvalidOperationException(
                        $"Configured recognizer '{definition.Name}' could not be constructed for language '{languageConfiguration.Language}'.");

                AddRecognizer(recognizer);
            }
        }

        if (nlpEngine is not null)
        {
            AddNlpRecognizer(nlpEngine);
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

    private static Type? ResolveRecognizerType(string recognizerName)
    {
        if (RecognizerTypeCache.TryGetValue(recognizerName, out var cached))
        {
            return cached;
        }

        var assembly = typeof(RecognizerRegistry).Assembly;
        var type = assembly
            .GetTypes()
            .FirstOrDefault(candidate =>
                typeof(EntityRecognizer).IsAssignableFrom(candidate) &&
                string.Equals(candidate.Name, recognizerName, StringComparison.Ordinal));

        RecognizerTypeCache[recognizerName] = type;
        return type;
    }

    private bool HasRecognizer(Type recognizerType, string language) =>
        _recognizers.Any(existing =>
            existing.GetType() == recognizerType &&
            string.Equals(existing.SupportedLanguage, language, StringComparison.OrdinalIgnoreCase));

    private EntityRecognizer? InstantiatePredefinedRecognizer(
        Type recognizerType,
        RecognizerLanguageConfiguration languageConfiguration)
    {
        var constructors = recognizerType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(ctor => ctor.GetParameters().Length);

        foreach (var constructor in constructors)
        {
            try
            {
                var arguments = BuildConstructorArguments(constructor.GetParameters(), languageConfiguration);
                if (constructor.Invoke(arguments) is EntityRecognizer recognizer)
                {
                    ApplyGlobalRegexOptions(recognizer);
                    return recognizer;
                }
            }
            catch (TargetInvocationException)
            {
                // Skip constructors that throw due to unsupported arguments and try the next overload.
            }
            catch (ArgumentException)
            {
                // Skip constructors that cannot be invoked with the generated arguments.
            }
        }

        return null;
    }

    private object?[] BuildConstructorArguments(
        IReadOnlyList<ParameterInfo> parameters,
        RecognizerLanguageConfiguration languageConfiguration)
    {
        var arguments = new object?[parameters.Count];
        var context = languageConfiguration.Context.Count > 0
            ? languageConfiguration.Context.ToArray()
            : Array.Empty<string>();

        for (var index = 0; index < parameters.Count; index++)
        {
            var parameter = parameters[index];
            var parameterType = parameter.ParameterType;

            if (IsPatternEnumerable(parameterType))
            {
                arguments[index] = null;
            }
            else if (typeof(IEnumerable<string>).IsAssignableFrom(parameterType))
            {
                arguments[index] = context.Length > 0 ? context : null;
            }
            else if (parameterType == typeof(string))
            {
                if (string.Equals(parameter.Name, "supportedLanguage", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(parameter.Name, "supported_language", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(parameter.Name, "language", StringComparison.OrdinalIgnoreCase))
                {
                    arguments[index] = languageConfiguration.Language;
                }
                else if (parameter.HasDefaultValue)
                {
                    arguments[index] = parameter.DefaultValue;
                }
                else
                {
                    arguments[index] = string.Empty;
                }
            }
            else if (parameterType == typeof(RegexOptions) || parameterType == typeof(RegexOptions?))
            {
                arguments[index] = _globalRegexOptions;
            }
            else if (parameter.HasDefaultValue)
            {
                arguments[index] = parameter.DefaultValue;
            }
            else if (parameterType.IsValueType)
            {
                arguments[index] = Activator.CreateInstance(parameterType);
            }
            else
            {
                arguments[index] = null;
            }
        }

        return arguments;
    }

    private static bool IsPatternEnumerable(Type parameterType)
    {
        return parameterType.IsGenericType &&
               parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
               parameterType.GenericTypeArguments.Length == 1 &&
               parameterType.GenericTypeArguments[0] == typeof(Pattern);
    }

    private void ApplyGlobalRegexOptions(EntityRecognizer recognizer)
    {
        if (recognizer is PatternRecognizer patternRecognizer)
        {
            patternRecognizer.SetGlobalRegexOptions(_globalRegexOptions);
            patternRecognizer.SetRegexMatchTimeout(RegexMatchTimeout);
        }
    }

    private static TimeSpan ValidateRegexMatchTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout == Regex.InfiniteMatchTimeout)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Regex match timeout must be finite and greater than zero.");
        }

        return timeout;
    }
}
