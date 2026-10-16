using ResolveDependencies.Core;

namespace ResolveDependencies.Console;

/// <summary>
/// Runs the text of one input file through sanitizer, parser and resolver and turns the
/// outcome into a <see cref="FileReport"/>. A format error is a regular result, not a
/// failure.
/// </summary>
public sealed class InputFileProcessor
{
    private readonly InputSanitizer _sanitizer = new();
    private readonly InputParser _parser = new();
    private readonly DependencyResolver _resolver = new();

    public FileReport Process(string text)
    {
        try
        {
            var request = _parser.Parse(_sanitizer.Sanitize(text));
            var result = _resolver.Resolve(request);
            return result.IsValid ? FileReport.Pass() : FileReport.Fail(result.Conflicts);
        }
        catch (InputFormatException error)
        {
            return FileReport.Error(error.Message);
        }
    }
}
