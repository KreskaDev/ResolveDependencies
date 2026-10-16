namespace ResolveDependencies.Console;

/// <summary>
/// Thrown by <see cref="InputParser"/> when the input breaks a format rule. Carries the
/// one-based line number and the reason; the message is the output file's second line.
/// </summary>
public sealed class InputFormatException(int lineNumber, string reason)
    : FormatException($"line {lineNumber}: {reason}")
{
    public int LineNumber { get; } = lineNumber;

    public string Reason { get; } = reason;
}
