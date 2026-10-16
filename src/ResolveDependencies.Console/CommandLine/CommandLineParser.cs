namespace ResolveDependencies.Console.CommandLine;

/// <summary>
/// Reads the input and output folder from the arguments. Each folder may be given
/// positionally (input first, output second) or by its option (<c>--input</c>,
/// <c>--output</c>), in any order, each at most once. The input folder is required, the
/// output folder defaults to it.
/// </summary>
public sealed class CommandLineParser
{
    public const string InputOption = "--input";
    public const string OutputOption = "--output";

    public const string Usage =
        "Usage: ResolveDependencies.Console <input-folder> [output-folder]\n" +
        "       ResolveDependencies.Console --input <folder> [--output <folder>]";

    /// <exception cref="CommandLineException">The arguments do not form a valid call.</exception>
    public CommandLineOptions Parse(IReadOnlyList<string> args)
    {
        string? input = null;
        string? output = null;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg == InputOption)
            {
                input = Assign(input, OptionValue(args, ref i), "input folder");
            }
            else if (arg == OutputOption)
            {
                output = Assign(output, OptionValue(args, ref i), "output folder");
            }
            else if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new CommandLineException($"Unknown option: {arg}");
            }
            else if (input is null)
            {
                input = Folder(arg);
            }
            else if (output is null)
            {
                output = Folder(arg);
            }
            else
            {
                throw new CommandLineException($"Unexpected argument: {arg}");
            }
        }

        if (input is null)
        {
            throw new CommandLineException("Input folder is required");
        }

        return new CommandLineOptions(input, output ?? input);
    }

    private static string OptionValue(IReadOnlyList<string> args, ref int index)
    {
        if (index + 1 >= args.Count)
        {
            throw new CommandLineException($"Missing value for {args[index]}");
        }

        return Folder(args[++index]);
    }

    private static string Folder(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CommandLineException("A folder must not be empty");
        }

        return value;
    }

    private static string Assign(string? current, string value, string what)
    {
        if (current is not null)
        {
            throw new CommandLineException($"The {what} is given more than once");
        }

        return value;
    }
}
