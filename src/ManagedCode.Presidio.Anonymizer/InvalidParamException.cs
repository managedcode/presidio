namespace ManagedCode.Presidio.Anonymizer;

/// <summary>
/// Exception thrown when anonymizer inputs fail validation.
/// Mirrors the Python InvalidParamError semantics.
/// </summary>
public sealed class InvalidParamException : Exception
{
    public InvalidParamException()
    {
    }

    public InvalidParamException(string message)
        : base(message)
    {
    }

    public InvalidParamException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
