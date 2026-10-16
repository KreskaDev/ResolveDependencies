using ResolveDependencies.Core.Validation;

namespace ResolveDependencies.Console.Processing;

public enum Verdict
{
    Pass,
    Fail,
    Error,
}

/// <summary>
/// Answer to one installation request, rendered to the output file and the console.
/// <paramref name="Conflicts"/> is filled for <see cref="Verdict.Fail"/>,
/// <paramref name="Reason"/> for <see cref="Verdict.Error"/>.
/// </summary>
public sealed record InstallationReport(Verdict Verdict, IReadOnlyList<Conflict> Conflicts, string? Reason)
{
    public static InstallationReport Pass() => new(Verdict.Pass, [], null);

    public static InstallationReport Fail(IReadOnlyList<Conflict> conflicts) => new(Verdict.Fail, conflicts, null);

    public static InstallationReport Error(string reason) => new(Verdict.Error, [], reason);

    /// <summary>The verdict, followed by the reason for an error. Every line ends with <c>\n</c>.</summary>
    public string ToOutputFileContent() =>
        Reason is null ? $"{VerdictKeyword}\n" : $"{VerdictKeyword}\n{Reason}\n";

    /// <summary>The summary line for the file, followed by the indented diagnostics.</summary>
    public IEnumerable<string> ToConsoleLines(string fileName)
    {
        yield return $"{fileName}: {VerdictKeyword}";

        foreach (var conflict in Conflicts)
        {
            yield return $"  {conflict.Package}: {string.Join(", ", conflict.Versions)}";
        }

        if (Reason is not null)
        {
            yield return $"  {Reason}";
        }
    }

    private string VerdictKeyword => Verdict switch
    {
        Verdict.Pass => "PASS",
        Verdict.Fail => "FAIL",
        Verdict.Error => "ERROR",
        _ => throw new InvalidOperationException($"Unknown verdict {Verdict}"),
    };
}
