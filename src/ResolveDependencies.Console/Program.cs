using ResolveDependencies.Console.CommandLine;
using ResolveDependencies.Console.Processing;

const string InputPrefix = "input";
const string OutputPrefix = "output";

CommandLineOptions options;
try
{
    options = new CommandLineParser().Parse(args);
}
catch (CommandLineException exception)
{
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine(CommandLineParser.Usage);
    return 1;
}

if (!Directory.Exists(options.InputFolder))
{
    Console.Error.WriteLine($"Input folder not found: {options.InputFolder}");
    return 1;
}

try
{
    Directory.CreateDirectory(options.OutputFolder);

    var inputFiles = Directory
        .EnumerateFiles(options.InputFolder, "*.txt")
        .Where(path => Path.GetFileName(path).StartsWith(InputPrefix, StringComparison.Ordinal))
        .OrderBy(Path.GetFileName, StringComparer.Ordinal);

    var processor = new InputFileProcessor();
    foreach (var inputPath in inputFiles)
    {
        var inputName = Path.GetFileName(inputPath);
        var outputName = string.Concat(OutputPrefix, inputName.AsSpan(InputPrefix.Length));

        var report = processor.Process(File.ReadAllText(inputPath));

        File.WriteAllText(Path.Combine(options.OutputFolder, outputName), report.ToOutputFileContent());
        foreach (var line in report.ToConsoleLines(inputName))
        {
            Console.WriteLine(line);
        }
    }

    return 0;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
