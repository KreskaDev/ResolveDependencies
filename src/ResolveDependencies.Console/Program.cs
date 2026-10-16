using ResolveDependencies.Console;

const string InputPrefix = "input";
const string OutputPrefix = "output";

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("Usage: ResolveDependencies.Console <input-folder> [output-folder]");
    return 1;
}

var inputFolder = args[0];
var outputFolder = args.Length == 2 ? args[1] : inputFolder;

if (!Directory.Exists(inputFolder))
{
    Console.Error.WriteLine($"Input folder not found: {inputFolder}");
    return 1;
}

try
{
    Directory.CreateDirectory(outputFolder);

    var inputFiles = Directory
        .EnumerateFiles(inputFolder, "*.txt")
        .Where(path => Path.GetFileName(path).StartsWith(InputPrefix, StringComparison.Ordinal))
        .OrderBy(Path.GetFileName, StringComparer.Ordinal);

    var processor = new InputFileProcessor();
    foreach (var inputPath in inputFiles)
    {
        var inputName = Path.GetFileName(inputPath);
        var outputName = string.Concat(OutputPrefix, inputName.AsSpan(InputPrefix.Length));

        var report = processor.Process(File.ReadAllText(inputPath));

        File.WriteAllText(Path.Combine(outputFolder, outputName), report.ToOutputFileContent());
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
