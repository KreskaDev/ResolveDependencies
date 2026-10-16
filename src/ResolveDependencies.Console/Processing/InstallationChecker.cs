using ResolveDependencies.Console.Input;
using ResolveDependencies.Core;

namespace ResolveDependencies.Console.Processing;

/// <summary>
/// Checks one installation request given as the text of an input file: runs it through
/// sanitizer, parser and resolver and turns the outcome into an
/// <see cref="InstallationReport"/>. A format error is a regular result, not a failure.
/// </summary>
public sealed class InstallationChecker
{
    private readonly InputSanitizer _sanitizer = new();
    private readonly InputParser _parser = new();
    private readonly DependencyResolver _resolver = new();

    public InstallationReport Check(string text)
    {
        try
        {
            var request = _parser.Parse(_sanitizer.Sanitize(text));
            var result = _resolver.Resolve(request);
            return result.IsValid ? InstallationReport.Pass() : InstallationReport.Fail(result.Conflicts);
        }
        catch (InputFormatException error)
        {
            return InstallationReport.Error(error.Message);
        }
    }
}
