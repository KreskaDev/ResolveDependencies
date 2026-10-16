using System.Globalization;
using ResolveDependencies.Core;

namespace ResolveDependencies.Console;

/// <summary>
/// Reads the sanitized lines of one input file into an <see cref="InstallationRequest"/>.
/// The only place that knows the line format. Line numbers in errors are one-based and
/// match the input file, because the sanitizer never removes lines before the end.
/// </summary>
public sealed class InputParser
{
    private const char FieldSeparator = ',';

    /// <exception cref="InputFormatException">The lines break a format rule.</exception>
    public InstallationRequest Parse(IReadOnlyList<string> lines)
    {
        var index = 0;

        var packageCount = ParseCount(NextLine(lines, ref index, "the number of packages"), index);
        var packages = new List<PackageVersion>(packageCount);
        for (var i = 0; i < packageCount; i++)
        {
            var line = NextLine(lines, ref index, $"{packageCount} package lines, got {i}");
            packages.Add(ParsePackageLine(line, index));
        }

        var dependencies = new List<Dependency>();
        if (index < lines.Count)
        {
            var dependencyCount = ParseCount(NextLine(lines, ref index, "the number of dependencies"), index);
            for (var i = 0; i < dependencyCount; i++)
            {
                var line = NextLine(lines, ref index, $"{dependencyCount} dependency lines, got {i}");
                ParseDependencyLine(line, index, dependencies);
            }

            if (index < lines.Count)
            {
                throw new InputFormatException(index + 1, "unexpected line after the dependency section");
            }
        }

        return new InstallationRequest(packages, dependencies);
    }

    private static string NextLine(IReadOnlyList<string> lines, ref int index, string expectation)
    {
        if (index >= lines.Count)
        {
            throw new InputFormatException(index + 1, $"expected {expectation}");
        }

        return lines[index++];
    }

    private static int ParseCount(string line, int lineNumber)
    {
        RejectEmptyLine(line, lineNumber);
        if (!int.TryParse(line, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
        {
            throw new InputFormatException(lineNumber, $"expected a non-negative integer, got '{line}'");
        }

        return count;
    }

    private static PackageVersion ParsePackageLine(string line, int lineNumber)
    {
        var fields = SplitFields(line, lineNumber);
        if (fields.Length != 2)
        {
            throw new InputFormatException(lineNumber, $"expected 2 fields, got {fields.Length}");
        }

        return new PackageVersion(fields[0], fields[1]);
    }

    private static void ParseDependencyLine(string line, int lineNumber, List<Dependency> dependencies)
    {
        var fields = SplitFields(line, lineNumber);
        if (fields.Length % 2 != 0)
        {
            throw new InputFormatException(lineNumber, $"expected an even number of fields, got {fields.Length}");
        }

        if (fields.Length < 4)
        {
            throw new InputFormatException(lineNumber, $"expected at least 4 fields, got {fields.Length}");
        }

        var dependent = new PackageVersion(fields[0], fields[1]);
        for (var i = 2; i < fields.Length; i += 2)
        {
            dependencies.Add(new Dependency(dependent, new PackageVersion(fields[i], fields[i + 1])));
        }
    }

    private static string[] SplitFields(string line, int lineNumber)
    {
        RejectEmptyLine(line, lineNumber);
        var fields = line.Split(FieldSeparator);
        if (fields.Any(field => field.Length == 0))
        {
            throw new InputFormatException(lineNumber, "empty field");
        }

        return fields;
    }

    private static void RejectEmptyLine(string line, int lineNumber)
    {
        if (line.Length == 0)
        {
            throw new InputFormatException(lineNumber, "empty line");
        }
    }
}
