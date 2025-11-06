namespace ManagedCode.Presidio.Anonymizer;

public sealed class MaskOperator : Operator
{
    public const string CharsToMaskKey = "chars_to_mask";
    public const string FromEndKey = "from_end";
    public const string MaskingCharKey = "masking_char";

    public override string Operate(string text, IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        var charsToMask = Convert.ToInt32(effectiveParams[CharsToMaskKey]!);
        var fromEnd = Convert.ToBoolean(effectiveParams[FromEndKey]!);
        var maskingChar = effectiveParams[MaskingCharKey]?.ToString() ?? string.Empty;
        var input = text ?? string.Empty;

        var effectiveChars = charsToMask > 0 ? Math.Min(input.Length, charsToMask) : 0;
        if (effectiveChars == 0)
        {
            return input;
        }

        var mask = new string(maskingChar[0], effectiveChars);
        return fromEnd
            ? string.Concat(input[..(input.Length - effectiveChars)], mask)
            : string.Concat(mask, input[effectiveChars..]);
    }

    public override void Validate(IDictionary<string, object?>? parameters = null)
    {
        var effectiveParams = parameters ?? new Dictionary<string, object?>();
        Validators.ValidateParameter(effectiveParams.TryGetValue(MaskingCharKey, out var mask) ? mask : null, MaskingCharKey, typeof(string));
        if (mask is string maskString && maskString.Length > 1)
        {
            throw new InvalidParamException($"Invalid input, {MaskingCharKey} must be a character");
        }

        Validators.ValidateParameter(effectiveParams.TryGetValue(CharsToMaskKey, out var chars) ? chars : null, CharsToMaskKey, typeof(int));
        Validators.ValidateParameter(effectiveParams.TryGetValue(FromEndKey, out var fromEnd) ? fromEnd : null, FromEndKey, typeof(bool));
    }

    public override string OperatorName => "mask";

    public override OperatorType Type => OperatorType.Anonymize;
}
