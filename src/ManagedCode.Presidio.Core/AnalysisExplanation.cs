namespace ManagedCode.Presidio.Core;

/// <summary>
/// Captures the rationale behind a recognizer decision, mirroring the Python AnalysisExplanation.
/// </summary>
public sealed class AnalysisExplanation
{
    public AnalysisExplanation(
        string recognizer,
        double originalScore,
        string? patternName = null,
        string? pattern = null,
        double? validationResult = null,
        string? textualExplanation = null,
        int? regexFlags = null)
    {
        if (string.IsNullOrWhiteSpace(recognizer))
        {
            throw new ArgumentException("Recognizer name must be provided.", nameof(recognizer));
        }

        Recognizer = recognizer;
        PatternName = patternName;
        Pattern = pattern;
        OriginalScore = originalScore;
        Score = originalScore;
        TextualExplanation = textualExplanation;
        ValidationResult = validationResult;
        RegexFlags = regexFlags;
    }

    public string Recognizer { get; }

    public string? PatternName { get; }

    public string? Pattern { get; }

    public double OriginalScore { get; }

    public double Score { get; private set; }

    public string? TextualExplanation { get; private set; }

    public double ScoreContextImprovement { get; private set; }

    public string SupportiveContextWord { get; private set; } = string.Empty;

    public double? ValidationResult { get; }

    public int? RegexFlags { get; }

    public void SetImprovedScore(double score)
    {
        Score = score;
        ScoreContextImprovement = Score - OriginalScore;
    }

    public void SetSupportiveContextWord(string word)
    {
        SupportiveContextWord = word ?? string.Empty;
    }

    public void AppendTextualExplanationLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        TextualExplanation = TextualExplanation is null
            ? text
            : $"{TextualExplanation}{Environment.NewLine}{text}";
    }

    public IReadOnlyDictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            { nameof(Recognizer), Recognizer },
            { nameof(PatternName), PatternName },
            { nameof(Pattern), Pattern },
            { nameof(OriginalScore), OriginalScore },
            { nameof(Score), Score },
            { nameof(TextualExplanation), TextualExplanation },
            { nameof(ScoreContextImprovement), ScoreContextImprovement },
            { nameof(SupportiveContextWord), SupportiveContextWord },
            { nameof(ValidationResult), ValidationResult },
            { nameof(RegexFlags), RegexFlags },
        };
    }

    public override string ToString() => $"{Recognizer} ({OriginalScore:F2}->{Score:F2})";
}
