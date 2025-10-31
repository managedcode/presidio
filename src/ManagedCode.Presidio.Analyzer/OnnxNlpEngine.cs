using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace ManagedCode.Presidio.Analyzer;

/// <summary>
/// ONNX-backed NLP engine that performs token classification using a fine-tuned BERT model.
/// </summary>
public sealed class OnnxNlpEngine : INlpEngine
{
    private static readonly ImmutableDictionary<string, string> EntityTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["PER"] = "PERSON",
        ["ORG"] = "ORGANIZATION",
        ["LOC"] = "LOCATION",
        ["MISC"] = "MISC",
    }.ToImmutableDictionary();

    private static readonly ImmutableHashSet<string> StopWords = new[]
    {
        "a", "an", "and", "are", "as", "at", "be", "for", "from", "has", "in", "is", "it",
        "of", "on", "or", "that", "the", "to", "was", "were", "with"
    }.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

    private readonly OnnxNlpEngineOptions _options;
    private readonly InferenceSession _session;
    private readonly BertTokenizer _tokenizer;
    private readonly IReadOnlyDictionary<int, string> _labelMap;
    private readonly IReadOnlyList<string> _labelIndex;
    private readonly IReadOnlyList<string> _inputNames;
    private readonly string _outputName;

    public OnnxNlpEngine(OnnxNlpEngineOptions? options = null)
    {
        _options = options ?? new OnnxNlpEngineOptions();

        OnnxModelArtifactManager.EnsureModelArtifacts(_options);
        ValidateModelArtifacts(_options);

        _session = new InferenceSession(_options.ModelPath);
        var tokenizerOptions = new BertOptions
        {
            ApplyBasicTokenization = true,
            LowerCaseBeforeTokenization = false,
            SplitOnSpecialTokens = false,
        };
        _tokenizer = BertTokenizer.Create(_options.VocabularyPath, tokenizerOptions);
        _labelMap = LoadLabelMap(_options.ConfigurationPath);
        _labelIndex = Enumerable.Range(0, _labelMap.Count)
            .Select(id => _labelMap.TryGetValue(id, out var label) ? label : "O")
            .ToArray();

        _inputNames = _session.InputMetadata.Keys.ToArray();
        _outputName = ResolveOutputName(_session.OutputMetadata.Keys, _options.OutputName);
    }

    public bool IsLoaded { get; private set; }

    public string PrimaryLanguage => _options.Language;

    public void Load()
    {
        if (IsLoaded)
        {
            return;
        }

        // Run a short warm-up pass to prime the ONNX session.
        _ = ProcessText(string.Empty, _options.Language);

        IsLoaded = true;
    }

    public NlpArtifacts ProcessText(string text, string language)
    {
        if (!string.Equals(language, _options.Language, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Language '{language}' is not supported by this ONNX model.");
        }

        text ??= string.Empty;

        var encodedTokens = _tokenizer.EncodeToTokens(text, out _);
        if (encodedTokens.Count == 0)
        {
            return new NlpArtifacts(language);
        }

        var tokensForSequence = TrimTokensToModelWindow(encodedTokens, _options.MaxSequenceLength);
        var tokenIds = tokensForSequence.Select(token => token.Id).ToList();

        var paddedInput = BuildPaddedInputs(tokenIds, _tokenizer, _options.MaxSequenceLength);

        var logits = RunInference(paddedInput);

        var entities = ExtractEntities(text, tokensForSequence, logits, paddedInput.ActualSequenceLength);

        var tokens = ConvertTokens(tokensForSequence);
        var keywords = ExtractKeywords(tokens, language);
        var scores = entities.Select(e => e.Score).ToArray();

        return new NlpArtifacts(language, tokens, keywords, scores, entities);
    }

    public IReadOnlyCollection<string> GetSupportedLanguages() => new[] { _options.Language };

    public IReadOnlyCollection<string> GetSupportedEntities() => EntityTypeMap.Values.ToArray();

    public bool IsStopWord(string token, string language)
    {
        ArgumentNullException.ThrowIfNull(token);
        return StopWords.Contains(token);
    }

    public bool IsPunctuation(string token, string language)
    {
        ArgumentNullException.ThrowIfNull(token);
        return token.Length == 1 && char.IsPunctuation(token[0]);
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    private static void ValidateModelArtifacts(OnnxNlpEngineOptions options)
    {
        var missing = new List<string>();
        if (!File.Exists(options.ModelPath))
        {
            missing.Add(options.ModelPath);
        }

        if (!File.Exists(options.VocabularyPath))
        {
            missing.Add(options.VocabularyPath);
        }

        if (!File.Exists(options.ConfigurationPath))
        {
            missing.Add(options.ConfigurationPath);
        }

        if (missing.Count > 0)
        {
            throw new FileNotFoundException($"ONNX NLP engine requires model artifacts. Missing: {string.Join(", ", missing)}");
        }
    }

    private static IReadOnlyDictionary<int, string> LoadLabelMap(string configPath)
    {
        using var stream = File.OpenRead(configPath);
        using var document = JsonDocument.Parse(stream);
        if (!document.RootElement.TryGetProperty("id2label", out var id2LabelElement))
        {
            throw new InvalidOperationException("Configuration file does not contain 'id2label'.");
        }

        var map = new Dictionary<int, string>();
        foreach (var property in id2LabelElement.EnumerateObject())
        {
            if (int.TryParse(property.Name, out var id))
            {
                map[id] = property.Value.GetString() ?? "O";
            }
        }

        return map;
    }

    private static string ResolveOutputName(IEnumerable<string> availableNames, string preferred)
    {
        var names = availableNames as IList<string> ?? availableNames.ToList();
        return names.Contains(preferred) ? preferred : names.First();
    }

    private static IReadOnlyList<EncodedToken> TrimTokensToModelWindow(
        IReadOnlyList<EncodedToken> tokens,
        int maxSequenceLength)
    {
        var maxTokens = Math.Max(1, maxSequenceLength - 2); // account for [CLS] and [SEP]
        if (tokens.Count <= maxTokens)
        {
            return tokens;
        }

        return tokens.Take(maxTokens).ToArray();
    }

    private InferenceInputs BuildPaddedInputs(
        IReadOnlyList<int> tokenIds,
        BertTokenizer tokenizer,
        int maxSequenceLength)
    {
        Span<int> buffer = tokenIds.Count + 2 <= 512
            ? stackalloc int[tokenIds.Count + 2]
            : new int[tokenIds.Count + 2];

        if (tokenizer.BuildInputsWithSpecialTokens(tokenIds, buffer, out var valuesWritten) != OperationStatus.Done)
        {
            valuesWritten = Math.Min(buffer.Length, tokenIds.Count + 2);
        }

        var seqLength = Math.Min(valuesWritten, maxSequenceLength);

        var inputIds = new long[maxSequenceLength];
        var attentionMask = new long[maxSequenceLength];
        var tokenTypeIds = new long[maxSequenceLength];

        for (var i = 0; i < seqLength; i++)
        {
            inputIds[i] = buffer[i];
            attentionMask[i] = 1;
        }

        for (var i = seqLength; i < maxSequenceLength; i++)
        {
            inputIds[i] = tokenizer.PaddingTokenId;
            attentionMask[i] = 0;
        }

        return new InferenceInputs(inputIds, attentionMask, tokenTypeIds, seqLength);
    }

    private Tensor<float> RunInference(InferenceInputs inputs)
    {
        static DenseTensor<long> CreateTensor(long[] data)
        {
            var tensor = new DenseTensor<long>(new[] { 1, data.Length });
            for (var i = 0; i < data.Length; i++)
            {
                tensor[0, i] = data[i];
            }

            return tensor;
        }

        var namedInputs = new List<NamedOnnxValue>(_inputNames.Count);
        foreach (var inputName in _inputNames)
        {
            if (string.Equals(inputName, _options.InputIdsName, StringComparison.OrdinalIgnoreCase))
            {
                namedInputs.Add(NamedOnnxValue.CreateFromTensor(inputName, CreateTensor(inputs.InputIds)));
            }
            else if (string.Equals(inputName, _options.AttentionMaskName, StringComparison.OrdinalIgnoreCase))
            {
                namedInputs.Add(NamedOnnxValue.CreateFromTensor(inputName, CreateTensor(inputs.AttentionMask)));
            }
            else if (string.Equals(inputName, _options.TokenTypeIdsName, StringComparison.OrdinalIgnoreCase))
            {
                namedInputs.Add(NamedOnnxValue.CreateFromTensor(inputName, CreateTensor(inputs.TokenTypeIds)));
            }
            else
            {
                throw new InvalidOperationException($"ONNX model input '{inputName}' is not supported by this engine.");
            }
        }

        using var results = _session.Run(namedInputs);
        var output = results.FirstOrDefault(result => string.Equals(result.Name, _outputName, StringComparison.OrdinalIgnoreCase))
                     ?? results[0];
        return output.AsTensor<float>();
    }

    private IReadOnlyList<NlpEntity> ExtractEntities(
        string text,
        IReadOnlyList<EncodedToken> tokens,
        Tensor<float> logits,
        int actualSequenceLength)
    {
        var entities = new List<NlpEntity>();

        if (logits.Dimensions.Length != 3)
        {
            throw new InvalidOperationException("Unexpected ONNX output shape. Expected [batch, sequence, labels].");
        }

        var sequenceLength = Math.Min(logits.Dimensions[1], actualSequenceLength);
        var labelCount = logits.Dimensions[2];
        var tokenCount = Math.Min(tokens.Count, sequenceLength - 2);

        EntityBuilder? current = null;
        for (var tokenIndex = 0; tokenIndex < tokenCount; tokenIndex++)
        {
            var logitsIndex = tokenIndex + 1; // skip [CLS]
            var (labelIndex, probability, distribution) = GetLabelPrediction(logits, logitsIndex, labelCount);
            var rawLabel = _labelIndex[labelIndex];

            if (!TryParseLabel(rawLabel, out var tag, out var rawType))
            {
                FinalizeCurrentEntity(ref current, entities);
                continue;
            }

            var normalizedType = NormalizeType(rawType);
            var encodedToken = tokens[tokenIndex];
            var start = GetOffsetValue(encodedToken.Offset.Start, text.Length);
            var end = GetOffsetValue(encodedToken.Offset.End, text.Length);

            if (tag == "B")
            {
                FinalizeCurrentEntity(ref current, entities);
                current = new EntityBuilder(normalizedType, rawType, start, end);
                current.AddScore(probability);
                current.SetDistribution(distribution);
            }
            else if (tag == "I" && current is not null && string.Equals(current.RawType, rawType, StringComparison.OrdinalIgnoreCase))
            {
                current.Extend(end, probability);
                current.CombineDistribution(distribution);
            }
            else if (tag == "I")
            {
                FinalizeCurrentEntity(ref current, entities);
                current = new EntityBuilder(normalizedType, rawType, start, end);
                current.AddScore(probability);
                current.SetDistribution(distribution);
            }
            else
            {
                FinalizeCurrentEntity(ref current, entities);
            }
        }

        FinalizeCurrentEntity(ref current, entities);

        return entities;
    }

    private static IReadOnlyList<Token> ConvertTokens(IReadOnlyList<EncodedToken> encodedTokens)
    {
        var tokens = new List<Token>(encodedTokens.Count);
        foreach (var encoded in encodedTokens)
        {
            tokens.Add(new Token(
                encoded.Value.Replace("##", string.Empty),
                GetOffsetValue(encoded.Offset.Start, 0),
                GetOffsetValue(encoded.Offset.End, 0),
                encoded.Value.Replace("##", string.Empty).ToLowerInvariant(),
                string.Empty));
        }

        return tokens;
    }

    private IReadOnlyList<string> ExtractKeywords(IReadOnlyList<Token> tokens, string language)
    {
        return tokens
            .Select(token => token.Text)
            .Where(text => !IsStopWord(text, language) && !IsPunctuation(text, language))
            .Select(text => text.ToLowerInvariant())
            .Distinct()
            .ToArray();
    }

    private static (int LabelIndex, double Probability, Embedding<float> Distribution) GetLabelPrediction(Tensor<float> logits, int tokenIndex, int labelCount)
    {
        var exponentials = new double[labelCount];
        var maxLogit = double.MinValue;
        for (var label = 0; label < labelCount; label++)
        {
            var logit = logits[0, tokenIndex, label];
            exponentials[label] = logit;
            if (logit > maxLogit)
            {
                maxLogit = logit;
            }
        }

        double sum = 0;
        for (var label = 0; label < labelCount; label++)
        {
            var exp = Math.Exp(exponentials[label] - maxLogit);
            exponentials[label] = exp;
            sum += exp;
        }

        var probabilities = new float[labelCount];
        var bestIndex = 0;
        double bestProbability = 0;
        for (var label = 0; label < labelCount; label++)
        {
            var probability = sum > 0 ? exponentials[label] / sum : 0;
            probabilities[label] = (float)probability;
            if (probability > bestProbability)
            {
                bestProbability = probability;
                bestIndex = label;
            }
        }

        return (bestIndex, bestProbability, new Embedding<float>(probabilities));
    }

    private static bool TryParseLabel(string label, out string tag, out string rawType)
    {
        tag = string.Empty;
        rawType = string.Empty;

        if (string.Equals(label, "O", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parts = label.Split('-', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        tag = parts[0].ToUpperInvariant();
        rawType = parts[1].ToUpperInvariant();
        return true;
    }

    private static string NormalizeType(string rawType)
    {
        return EntityTypeMap.TryGetValue(rawType, out var normalized)
            ? normalized
            : rawType.ToUpperInvariant();
    }

    private static int GetOffsetValue(Index index, int textLength)
    {
        return index.IsFromEnd ? textLength - index.Value : index.Value;
    }

    private static void FinalizeCurrentEntity(ref EntityBuilder? builder, ICollection<NlpEntity> target)
    {
        if (builder is null)
        {
            return;
        }

        target.Add(builder.ToEntity());
        builder = null;
    }

    private sealed class EntityBuilder(string type, string rawType, int start, int end)
    {
        private double _scoreSum;
        private int _scoreCount;
        private float[]? _distributionSum;

        public string Type { get; } = type;
        public string RawType { get; } = rawType;
        public int Start { get; } = start;
        public int End { get; private set; } = end;

        public void Extend(int end, double score)
        {
            End = Math.Max(End, end);
            AddScore(score);
        }

        public void AddScore(double score)
        {
            _scoreSum += score;
            _scoreCount++;
        }

        public void SetDistribution(Embedding<float> distribution)
        {
            _distributionSum = distribution.Vector.ToArray();
        }

        public void CombineDistribution(Embedding<float> distribution)
        {
            if (_distributionSum is null)
            {
                SetDistribution(distribution);
                return;
            }

            var vector = distribution.Vector.Span;
            if (_distributionSum.Length != vector.Length)
            {
                return;
            }

            for (var i = 0; i < _distributionSum.Length; i++)
            {
                _distributionSum[i] += vector[i];
            }
        }

        public NlpEntity ToEntity()
        {
            var averageScore = _scoreCount == 0 ? 0 : _scoreSum / _scoreCount;
            Embedding<float>? embedding = null;
            if (_distributionSum is not null && _scoreCount > 0)
            {
                var averaged = new float[_distributionSum.Length];
                for (var i = 0; i < averaged.Length; i++)
                {
                    averaged[i] = _distributionSum[i] / _scoreCount;
                }

                embedding = new Embedding<float>(averaged);
            }

            return new NlpEntity(Type, Start, End, averageScore, RawType, "onnx", embedding);
        }
    }

    private sealed record InferenceInputs(long[] InputIds, long[] AttentionMask, long[] TokenTypeIds, int ActualSequenceLength);

}
