namespace ResolveDependencies.Console.Input;

/// <summary>
/// Turns the raw text of an input file into clean lines: line endings <c>\n</c> and
/// <c>\r\n</c> are equivalent, a missing trailing newline is tolerated, every line is
/// trimmed and empty lines at the end of the file are dropped. Empty lines inside the
/// file are kept so the parser can report them.
/// </summary>
public sealed class InputSanitizer
{
    public IReadOnlyList<string> Sanitize(string text)
    {
        var lines = text
            .ReplaceLineEndings("\n")
            .Split('\n')
            .Select(line => line.Trim())
            .ToList();

        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }
}
