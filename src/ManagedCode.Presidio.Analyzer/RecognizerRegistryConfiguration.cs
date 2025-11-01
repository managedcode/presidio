using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// Declarative configuration for seeding the recognizer registry.
/// </summary>
public sealed class RecognizerRegistryConfiguration(
    IReadOnlyCollection<string> supportedLanguages,
    RegexOptions globalRegexOptions,
    IReadOnlyCollection<RecognizerDefinition> recognizers,
    string? source = null)
{
    public IReadOnlyCollection<string> SupportedLanguages { get; } = supportedLanguages ?? throw new ArgumentNullException(nameof(supportedLanguages));

    public RegexOptions GlobalRegexOptions { get; } = globalRegexOptions;

    public IReadOnlyCollection<RecognizerDefinition> Recognizers { get; } = recognizers ?? throw new ArgumentNullException(nameof(recognizers));

    public string? Source { get; } = source;
}

/// <summary>
/// Declarative description of a recognizer instance to be created.
/// </summary>
public sealed class RecognizerDefinition
{
    public RecognizerDefinition(
        string name,
        string type,
        bool enabled,
        IReadOnlyCollection<RecognizerLanguageConfiguration> languages,
        IReadOnlyDictionary<string, object?> parameters)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Recognizer definition requires a name.", nameof(name));
        }

        Name = name;
        Type = string.IsNullOrWhiteSpace(type) ? "predefined" : type;
        Enabled = enabled;
        Languages = languages ?? Array.Empty<RecognizerLanguageConfiguration>();
        Parameters = parameters ?? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>());
    }

    public string Name { get; }

    public string Type { get; }

    public bool Enabled { get; }

    public IReadOnlyCollection<RecognizerLanguageConfiguration> Languages { get; }

    public IReadOnlyDictionary<string, object?> Parameters { get; }
}

/// <summary>
/// Language-specific configuration including optional context hints.
/// </summary>
public sealed class RecognizerLanguageConfiguration
{
    public RecognizerLanguageConfiguration(string language, IReadOnlyCollection<string> context)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            throw new ArgumentException("Language must be provided.", nameof(language));
        }

        Language = language;
        Context = context ?? Array.Empty<string>();
    }

    public string Language { get; }

    public IReadOnlyCollection<string> Context { get; }
}

internal static class RecognizerRegistryConfigurationLoader
{
    private const string DefaultEmbeddedResourceName = "ManagedCode.Presidio.Analyzer.conf.default_recognizers.yaml";

    public static RecognizerRegistryConfiguration Load(string? configurationPath = null)
    {
        if (!string.IsNullOrWhiteSpace(configurationPath))
        {
            using var fileStream = File.OpenRead(configurationPath);
            return LoadFromStream(fileStream, configurationPath);
        }

        var assembly = typeof(RecognizerRegistryConfigurationLoader).Assembly;
        using var resourceStream = assembly.GetManifestResourceStream(DefaultEmbeddedResourceName)
            ?? throw new InvalidOperationException($"Embedded registry configuration '{DefaultEmbeddedResourceName}' not found.");
        return LoadFromStream(resourceStream, source: DefaultEmbeddedResourceName);
    }

    private static RecognizerRegistryConfiguration LoadFromStream(Stream stream, string? source)
    {
        using var reader = new StreamReader(stream);
        var yaml = new YamlStream();
        yaml.Load(reader);

        if (yaml.Documents.Count == 0)
        {
            throw new InvalidOperationException("Recognizer registry configuration YAML is empty.");
        }

        if (yaml.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new InvalidOperationException("Recognizer registry configuration must be a YAML mapping.");
        }

        var rootDictionary = ConvertMapping(root);

        var supportedLanguages = ExtractSupportedLanguages(rootDictionary);
        var regexOptions = ExtractRegexOptions(rootDictionary);
        var recognizers = ExtractRecognizers(rootDictionary, supportedLanguages);

        return new RecognizerRegistryConfiguration(
            supportedLanguages,
            regexOptions,
            recognizers,
            source);
    }

    private static IReadOnlyCollection<string> ExtractSupportedLanguages(IReadOnlyDictionary<string, object?> root)
    {
        if (!root.TryGetValue("supported_languages", out var value))
        {
            return Array.Empty<string>();
        }

        return ConvertToStringList(value);
    }

    private static RegexOptions ExtractRegexOptions(IReadOnlyDictionary<string, object?> root)
    {
        if (!root.TryGetValue("global_regex_flags", out var value) || value is not int flags)
        {
            return RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;
        }

        var options = RegexOptions.Compiled;
        if ((flags & 0x02) != 0)
        {
            options |= RegexOptions.IgnoreCase;
        }

        if ((flags & 0x08) != 0)
        {
            options |= RegexOptions.Multiline;
        }

        if ((flags & 0x10) != 0)
        {
            options |= RegexOptions.Singleline;
        }

        return options;
    }

    private static IReadOnlyCollection<RecognizerDefinition> ExtractRecognizers(
        IReadOnlyDictionary<string, object?> root,
        IReadOnlyCollection<string> defaultLanguages)
    {
        if (!root.TryGetValue("recognizers", out var value))
        {
            return Array.Empty<RecognizerDefinition>();
        }

        if (value is not IList<object?> recognizerList)
        {
            throw new InvalidOperationException("The 'recognizers' element must be a sequence.");
        }

        var definitions = new List<RecognizerDefinition>();
        foreach (var entry in recognizerList)
        {
            if (entry is null)
            {
                continue;
            }

            if (entry is string recognizerName)
            {
                var languages = defaultLanguages.Count == 0
                    ? Array.Empty<RecognizerLanguageConfiguration>()
                    : defaultLanguages
                        .Select(language => new RecognizerLanguageConfiguration(language, Array.Empty<string>()))
                        .ToArray();

                definitions.Add(new RecognizerDefinition(
                    recognizerName,
                    "predefined",
                    enabled: true,
                    languages,
                    new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>())));
                continue;
            }

            if (entry is not IDictionary<string, object?> recognizerMap)
            {
                throw new InvalidOperationException("Each recognizer entry must be a mapping or scalar.");
            }

            var definition = CreateRecognizerDefinition(recognizerMap, defaultLanguages);
            if (definition.Enabled)
            {
                definitions.Add(definition);
            }
        }

        return definitions;
    }

    private static RecognizerDefinition CreateRecognizerDefinition(
        IDictionary<string, object?> recognizerMap,
        IReadOnlyCollection<string> defaultLanguages)
    {
        var working = new Dictionary<string, object?>(recognizerMap, StringComparer.OrdinalIgnoreCase);

        if (!TryExtractString(working, "name", out var name) || string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Recognizer definition missing required 'name'.");
        }

        var type = TryExtractString(working, "type", out var explicitType)
            ? explicitType!
            : "predefined";

        var enabled = !TryExtractBool(working, "enabled", out var enabledValue) || enabledValue;

        var defaultContext = TryExtractStringList(working, "context", out var contextList)
            ? contextList
            : Array.Empty<string>();

        var languages = TryExtractLanguages(working, defaultLanguages, defaultContext, out var languageConfigurations)
            ? languageConfigurations
            : defaultLanguages.Select(language => new RecognizerLanguageConfiguration(language, defaultContext)).ToArray();

        // Remove keys we have consumed.
        working.Remove("name");
        working.Remove("type");
        working.Remove("enabled");
        working.Remove("context");
        working.Remove("supported_languages");

        var parameters = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(working, StringComparer.OrdinalIgnoreCase));

        return new RecognizerDefinition(
            name!,
            type,
            enabled,
            languages,
            parameters);
    }

    private static bool TryExtractLanguages(
        IDictionary<string, object?> working,
        IReadOnlyCollection<string> defaultLanguages,
        IReadOnlyCollection<string> defaultContext,
        out IReadOnlyCollection<RecognizerLanguageConfiguration> languages)
    {
        languages = Array.Empty<RecognizerLanguageConfiguration>();
        if (!working.TryGetValue("supported_languages", out var value) || value is null)
        {
            return false;
        }

        if (value is not IList<object?> list)
        {
            throw new InvalidOperationException("'supported_languages' must be a sequence.");
        }

        var result = new List<RecognizerLanguageConfiguration>();
        foreach (var entry in list)
        {
            switch (entry)
            {
                case null:
                    continue;

                case string language:
                    result.Add(new RecognizerLanguageConfiguration(language, defaultContext));
                    break;

                case IDictionary<string, object?> languageMap:
                    if (!TryExtractString(languageMap, "language", out var languageName) || string.IsNullOrWhiteSpace(languageName))
                    {
                        throw new InvalidOperationException("Language mapping must include 'language'.");
                    }

                    var context = TryExtractStringList(languageMap, "context", out var explicitContext)
                        ? explicitContext
                        : defaultContext;

                    result.Add(new RecognizerLanguageConfiguration(languageName!, context));
                    break;

                default:
                    throw new InvalidOperationException("Each 'supported_languages' entry must be a string or mapping.");
            }
        }

        languages = result;
        return result.Count > 0;
    }

    private static bool TryExtractString(IDictionary<string, object?> source, string key, out string? value)
    {
        if (source.TryGetValue(key, out var raw) && raw is string stringValue)
        {
            value = stringValue;
            return true;
        }

        value = null;
        return false;
    }

    private static bool TryExtractBool(IDictionary<string, object?> source, string key, out bool value)
    {
        if (source.TryGetValue(key, out var raw) && raw is bool boolValue)
        {
            value = boolValue;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryExtractStringList(IDictionary<string, object?> source, string key, out IReadOnlyCollection<string> values)
    {
        if (source.TryGetValue(key, out var raw))
        {
            values = ConvertToStringList(raw);
            return values.Count > 0;
        }

        values = Array.Empty<string>();
        return false;
    }

    private static IReadOnlyDictionary<string, object?> ConvertMapping(YamlMappingNode mapping)
    {
        var dictionary = ConvertMappingToDictionary(mapping);
        return new ReadOnlyDictionary<string, object?>(dictionary);
    }

    private static object? ConvertYamlNode(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalarNode => ConvertScalar(scalarNode),
            YamlSequenceNode sequenceNode => sequenceNode.Children.Select(ConvertYamlNode).ToList(),
            YamlMappingNode mappingNode => ConvertMappingToDictionary(mappingNode),
            _ => null,
        };
    }

    private static Dictionary<string, object?> ConvertMappingToDictionary(YamlMappingNode mapping)
    {
        var dictionary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (keyNode, valueNode) in mapping.Children)
        {
            if (keyNode is not YamlScalarNode scalarKey)
            {
                throw new InvalidOperationException("Only scalar keys are supported in recognizer configuration.");
            }

            var key = scalarKey.Value ?? string.Empty;
            dictionary[key] = ConvertYamlNode(valueNode);
        }

        return dictionary;
    }

    private static object? ConvertScalar(YamlScalarNode scalar)
    {
        if (scalar.Tag == "tag:yaml.org,2002:null" || string.Equals(scalar.Value, "null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (bool.TryParse(scalar.Value, out var boolValue))
        {
            return boolValue;
        }

        if (int.TryParse(scalar.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (double.TryParse(scalar.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return doubleValue;
        }

        return scalar.Value ?? string.Empty;
    }

    private static IReadOnlyCollection<string> ConvertToStringList(object? value)
    {
        if (value is null)
        {
            return Array.Empty<string>();
        }

        if (value is IList<object?> list)
        {
            var result = new List<string>();
            foreach (var item in list)
            {
                switch (item)
                {
                    case null:
                        continue;
                    case string str:
                        result.Add(str);
                        break;
                    default:
                        throw new InvalidOperationException("Expected a string value inside the sequence.");
                }
            }

            return result;
        }

        if (value is string single)
        {
            return new[] { single };
        }

        throw new InvalidOperationException("Expected a sequence or string while parsing string list.");
    }
}
